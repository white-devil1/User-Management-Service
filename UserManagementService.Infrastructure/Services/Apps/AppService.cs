using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Apps;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.RBAC;
using UserManagementService.Infrastructure.Persistence;

// ✅ ALIAS: Avoid conflict between namespace "Apps" and entity "App"
using AppEntity = UserManagementService.Domain.Entities.RBAC.App;

namespace UserManagementService.Infrastructure.Services.Apps;

public class AppService : IAppService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserDisplayNameResolver _resolver;

    public AppService(ApplicationDbContext context, IUserDisplayNameResolver resolver)
    {
        _context = context;
        _resolver = resolver;
    }

    public async Task<AppListResponse> GetAppsAsync(
        string? search,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Apps.AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            var searchTrimmed = search.Trim().ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(searchTrimmed) ||
                a.Code.ToLower().Contains(searchTrimmed) ||
                (a.Description != null && a.Description.ToLower().Contains(searchTrimmed)));
        }

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        if (!includeDeleted)
            query = query.Where(a => !a.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        query = sortBy.ToLower() switch
        {
            "name" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(a => a.Name)
                : query.OrderByDescending(a => a.Name),
            "code" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(a => a.Code)
                : query.OrderByDescending(a => a.Code),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(a => a.CreatedAt)
                : query.OrderByDescending(a => a.CreatedAt),
            _ => sortOrder.ToLower() == "asc"
                ? query.OrderBy(a => a.DisplayOrder)
                : query.OrderByDescending(a => a.DisplayOrder)
        };

        var apps = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var allIds = apps.SelectMany(a => new[] { a.CreatedBy, a.UpdatedBy, a.DeletedBy });
        var names = await BuildNameCacheAsync(allIds, cancellationToken);
        var appDtos = apps.Select(a => MapToDto(a, names)).ToList();

        return new AppListResponse
        {
            Apps = appDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<AppDto> GetAppByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
            throw new NotFoundException("App", id);

        var names = await BuildNameCacheAsync(new[] { app.CreatedBy, app.UpdatedBy, app.DeletedBy }, cancellationToken);
        return MapToDto(app, names);
    }

    public async Task<AppDto> CreateAppAsync(CreateAppRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        var code = GenerateAppCode(request.Name);

        if (await _context.Apps.AnyAsync(a => a.Code == code, cancellationToken))
            throw new ConflictException($"An app with code '{code}' already exists.");

        var app = new AppEntity
        {
            Name = request.Name,
            Code = code,
            Description = request.Description,
            Icon = request.Icon,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Apps.Add(app);
        await _context.SaveChangesAsync(cancellationToken);

        var names = await BuildNameCacheAsync(new[] { app.CreatedBy, app.UpdatedBy }, cancellationToken);
        return MapToDto(app, names);
    }

    public async Task<AppDto> UpdateAppAsync(Guid id, UpdateAppRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
            throw new NotFoundException("App", id);

        if (app.Name != request.Name)
        {
            var newCode = GenerateAppCode(request.Name);
            if (await _context.Apps.AnyAsync(a => a.Code == newCode && a.Id != id, cancellationToken))
                throw new ConflictException($"An app with code '{newCode}' already exists.");
            app.Code = newCode;
        }

        app.Name = request.Name;
        app.Description = request.Description;
        app.Icon = request.Icon;
        app.DisplayOrder = request.DisplayOrder;
        app.IsActive = request.IsActive;
        app.UpdatedBy = updatedBy;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var names = await BuildNameCacheAsync(new[] { app.CreatedBy, app.UpdatedBy }, cancellationToken);
        return MapToDto(app, names);
    }

    public async Task<bool> DeleteAppAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
            throw new NotFoundException("App", id);

        app.IsDeleted = true;
        app.DeletedAt = DateTime.UtcNow;
        app.DeletedBy = deletedBy;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AppDto> ToggleAppStatusAsync(Guid id, bool isActive, string updatedBy, CancellationToken cancellationToken = default)
    {
        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
            throw new NotFoundException("App", id);

        app.IsActive = isActive;
        app.UpdatedBy = updatedBy;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var names = await BuildNameCacheAsync(new[] { app.CreatedBy, app.UpdatedBy }, cancellationToken);
        return MapToDto(app, names);
    }

    private async Task<Dictionary<string, string>> BuildNameCacheAsync(
        IEnumerable<string?> userIds, CancellationToken ct)
    {
        var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct()!)
            cache[id!] = await _resolver.ResolveAsync(id, ct);
        return cache;
    }

    private static AppDto MapToDto(AppEntity app, Dictionary<string, string> names) => new()
    {
        Id = app.Id,
        Name = app.Name,
        Code = app.Code,
        Description = app.Description,
        Icon = app.Icon,
        DisplayOrder = app.DisplayOrder,
        IsActive = app.IsActive,
        CreatedAt = app.CreatedAt,
        CreatedBy = app.CreatedBy != null && names.TryGetValue(app.CreatedBy, out var cb) ? cb : null,
        UpdatedAt = app.UpdatedAt,
        UpdatedBy = app.UpdatedBy != null && names.TryGetValue(app.UpdatedBy, out var ub) ? ub : null,
        IsDeleted = app.IsDeleted,
        DeletedAt = app.DeletedAt,
        DeletedBy = app.DeletedBy != null && names.TryGetValue(app.DeletedBy, out var db) ? db : null
    };

    private static string GenerateAppCode(string appName)
    {
        var words = appName
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var code = string.Concat(words.Select(w =>
            w.Length <= 3 ? w.ToUpper() : w.Substring(0, 3).ToUpper()));

        return code.Length > 10 ? code.Substring(0, 10) : code;
    }
}
