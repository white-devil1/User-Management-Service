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

        // ✅ Apply Filters - Trim and case-insensitive search using ToLower()
        if (!string.IsNullOrEmpty(search))
        {
            var searchTrimmed = search.Trim().ToLower();
            query = query.Where(a => 
                a.Name.ToLower().Contains(searchTrimmed) || 
                a.Code.ToLower().Contains(searchTrimmed) || 
                (a.Description != null && a.Description.ToLower().Contains(searchTrimmed)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(a => a.IsActive == isActive.Value);
        }

        // ✅ Include deleted only if requested (Super Admin)
        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        // ✅ Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ Apply Sorting
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

        // ✅ Apply Pagination
        var apps = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var appDtos = apps.Select(MapToDto).ToList();

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
        {
            throw new NotFoundException("App", id);
        }

        return MapToDto(app);
    }

    public async Task<AppDto> CreateAppAsync(CreateAppRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        var createdByName = await _resolver.ResolveAsync(createdBy, cancellationToken);

        // ✅ Auto-generate App Code from Name
        var code = GenerateAppCode(request.Name);

        // ✅ Check if Code already exists
        if (await _context.Apps.AnyAsync(a => a.Code == code, cancellationToken))
        {
            throw new ConflictException($"An app with code '{code}' already exists.");
        }

        var app = new AppEntity
        {
            Name = request.Name,
            Code = code,
            Description = request.Description,
            Icon = request.Icon,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedBy = createdByName,
            CreatedAt = DateTime.UtcNow
        };

        _context.Apps.Add(app);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(app);
    }

    public async Task<AppDto> UpdateAppAsync(Guid id, UpdateAppRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        var updatedByName = await _resolver.ResolveAsync(updatedBy, cancellationToken);

        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
        {
            throw new NotFoundException("App", id);
        }

        // ✅ If Name changed, regenerate Code
        if (app.Name != request.Name)
        {
            var newCode = GenerateAppCode(request.Name);
            if (await _context.Apps.AnyAsync(a => a.Code == newCode && a.Id != id, cancellationToken))
            {
                throw new ConflictException($"An app with code '{newCode}' already exists.");
            }
            app.Code = newCode;
        }

        app.Name = request.Name;
        app.Description = request.Description;
        app.Icon = request.Icon;
        app.DisplayOrder = request.DisplayOrder;
        app.IsActive = request.IsActive;
        app.UpdatedBy = updatedByName;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(app);
    }

    public async Task<bool> DeleteAppAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var deletedByName = await _resolver.ResolveAsync(deletedBy, cancellationToken);

        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
        {
            throw new NotFoundException("App", id);
        }

        // ✅ Soft Delete
        app.IsDeleted = true;
        app.DeletedAt = DateTime.UtcNow;
        app.DeletedBy = deletedByName;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<AppDto> ToggleAppStatusAsync(Guid id, bool isActive, string updatedBy, CancellationToken cancellationToken = default)
    {
        var updatedByName = await _resolver.ResolveAsync(updatedBy, cancellationToken);

        var app = await _context.Apps
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (app == null)
        {
            throw new NotFoundException("App", id);
        }

        app.IsActive = isActive;
        app.UpdatedBy = updatedByName;
        app.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(app);
    }

    // ✅ Helper: Map Entity to DTO (using alias AppEntity)
    private static AppDto MapToDto(AppEntity app)
    {
        return new AppDto
        {
            Id = app.Id,
            Name = app.Name,
            Code = app.Code,
            Description = app.Description,
            Icon = app.Icon,
            DisplayOrder = app.DisplayOrder,
            IsActive = app.IsActive,
            CreatedAt = app.CreatedAt,
            CreatedBy = app.CreatedBy,
            UpdatedAt = app.UpdatedAt,
            UpdatedBy = app.UpdatedBy,
            IsDeleted = app.IsDeleted,
            DeletedAt = app.DeletedAt,
            DeletedBy = app.DeletedBy
        };
    }

    // ✅ Helper: Generate App Code from Name (Industry Standard)
    private static string GenerateAppCode(string appName)
    {
        // Remove special characters and split by spaces
        var words = appName
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Take first 2-3 letters of each word, uppercase
        var code = string.Concat(words.Select(w =>
            w.Length <= 3 ? w.ToUpper() : w.Substring(0, 3).ToUpper()));

        // Limit to 10 characters max
        return code.Length > 10 ? code.Substring(0, 10) : code;
    }
}