using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.AppActions;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.RBAC;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.AppActions;

public class AppActionService : IAppActionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserDisplayNameResolver _resolver;

    public AppActionService(ApplicationDbContext context, IUserDisplayNameResolver resolver)
    {
        _context = context;
        _resolver = resolver;
    }

    public async Task<AppActionListResponse> GetActionsAsync(
        Guid? appId,
        Guid? pageId,
        string? search,
        bool? isActive,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Actions
            .Include(a => a.Page).ThenInclude(p => p!.App)
            .AsQueryable();

        if (appId.HasValue)
            query = query.Where(a => a.Page != null && a.Page.AppId == appId.Value);

        if (pageId.HasValue)
            query = query.Where(a => a.PageId == pageId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmedSearch = search.Trim();
            query = query.Where(a =>
                EF.Functions.ILike(a.Name, $"%{trimmedSearch}%") ||
                EF.Functions.ILike(a.Code, $"%{trimmedSearch}%") ||
                (a.Description != null && EF.Functions.ILike(a.Description, $"%{trimmedSearch}%")));
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

        var actions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var allIds = actions.SelectMany(a => new[] { a.CreatedBy, a.UpdatedBy, a.DeletedBy });
        var names = await BuildNameCacheAsync(allIds, cancellationToken);
        var actionDtos = actions.Select(a => MapToDto(a, names)).ToList();

        return new AppActionListResponse
        {
            Actions = actionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<AppActionDto> GetActionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .Include(a => a.Page).ThenInclude(p => p!.App)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
            throw new NotFoundException("Action", id);

        var names = await BuildNameCacheAsync(new[] { action.CreatedBy, action.UpdatedBy, action.DeletedBy }, cancellationToken);
        return MapToDto(action, names);
    }

    public async Task<AppActionDto> CreateActionAsync(
        CreateAppActionRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        var page = await _context.Pages
            .Include(p => p.App)
            .FirstOrDefaultAsync(p => p.Id == request.PageId, cancellationToken);

        if (page == null)
            throw new NotFoundException("Page", request.PageId);

        var code = GenerateActionCode(request.Name);

        if (await _context.Actions.AnyAsync(a => a.PageId == request.PageId && a.Code == code, cancellationToken))
            throw new ConflictException($"An action with code '{code}' already exists within this page.");

        var action = new AppAction
        {
            PageId = request.PageId,
            Name = request.Name,
            Code = code,
            Type = request.Type,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        };

        _context.Actions.Add(action);

        if (page.App == null)
            throw new NotFoundException($"App not found for page {page.Id}");

        var permissionCode = $"{page.App.Code}_{page.Code}_{action.Code}";
        var permissionName = $"{page.App.Name} - {page.Name} - {action.Name}";

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            AppId = page.AppId,
            PageId = page.Id,
            ActionId = action.Id,
            PermissionCode = permissionCode,
            Name = permissionName,
            IsEnabled = false,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Permissions.AddAsync(permission);
        await _context.SaveChangesAsync(cancellationToken);

        var names = await BuildNameCacheAsync(new[] { action.CreatedBy, action.UpdatedBy }, cancellationToken);
        return MapToDto(action, names);
    }

    public async Task<AppActionDto> UpdateActionAsync(
        Guid id,
        UpdateAppActionRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
            throw new NotFoundException("Action", id);

        if (action.Name != request.Name)
        {
            var newCode = GenerateActionCode(request.Name);
            if (await _context.Actions.AnyAsync(a => a.PageId == action.PageId && a.Code == newCode && a.Id != id, cancellationToken))
                throw new ConflictException($"An action with code '{newCode}' already exists within this page.");
            action.Code = newCode;

            var page = await _context.Pages
                .Include(p => p.App)
                .FirstOrDefaultAsync(p => p.Id == action.PageId, cancellationToken);

            if (page != null && page.App != null)
            {
                var perm = await _context.Permissions
                    .FirstOrDefaultAsync(p => p.ActionId == action.Id, cancellationToken);

                if (perm != null)
                {
                    perm.PermissionCode = $"{page.App.Code}_{page.Code}_{action.Code}";
                    perm.Name = $"{page.App.Name} - {page.Name} - {action.Name}";
                    perm.UpdatedBy = updatedBy;
                    perm.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        action.Name = request.Name;
        action.Description = request.Description;
        action.Type = request.Type;
        action.DisplayOrder = request.DisplayOrder;
        action.IsActive = request.IsActive;
        action.UpdatedBy = updatedBy;
        action.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var names = await BuildNameCacheAsync(new[] { action.CreatedBy, action.UpdatedBy }, cancellationToken);
        return MapToDto(action, names);
    }

    public async Task<bool> DeleteActionAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
            throw new NotFoundException("Action", id);

        action.IsDeleted = true;
        action.DeletedAt = DateTime.UtcNow;
        action.DeletedBy = deletedBy;
        action.UpdatedAt = DateTime.UtcNow;

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.ActionId == id, cancellationToken);

        if (permission != null)
        {
            permission.IsDeleted = true;
            permission.DeletedAt = DateTime.UtcNow;
            permission.DeletedBy = deletedBy;
            permission.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<AppActionDto> ToggleActionStatusAsync(
        Guid id,
        bool isActive,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var action = await _context.Actions
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (action == null)
            throw new NotFoundException("Action", id);

        action.IsActive = isActive;
        action.UpdatedBy = updatedBy;
        action.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var names = await BuildNameCacheAsync(new[] { action.CreatedBy, action.UpdatedBy }, cancellationToken);
        return MapToDto(action, names);
    }

    private async Task<Dictionary<string, string>> BuildNameCacheAsync(
        IEnumerable<string?> userIds, CancellationToken ct)
    {
        var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct()!)
            cache[id!] = await _resolver.ResolveAsync(id, ct);
        return cache;
    }

    private static AppActionDto MapToDto(AppAction action, Dictionary<string, string> names) => new()
    {
        Id = action.Id,
        PageId = action.PageId,
        PageName = action.Page?.Name,
        AppId = action.Page?.AppId,
        AppName = action.Page?.App?.Name,
        Name = action.Name,
        Code = action.Code,
        Type = action.Type,
        Description = action.Description,
        DisplayOrder = action.DisplayOrder,
        IsActive = action.IsActive,
        CreatedAt = action.CreatedAt,
        CreatedBy = action.CreatedBy != null && names.TryGetValue(action.CreatedBy, out var cb) ? cb : null,
        UpdatedAt = action.UpdatedAt,
        UpdatedBy = action.UpdatedBy != null && names.TryGetValue(action.UpdatedBy, out var ub) ? ub : null,
        IsDeleted = action.IsDeleted,
        DeletedAt = action.DeletedAt,
        DeletedBy = action.DeletedBy != null && names.TryGetValue(action.DeletedBy, out var db) ? db : null
    };

    private static string GenerateActionCode(string actionName)
    {
        var words = actionName
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        var code = string.Concat(words.Select(w =>
            w.Length <= 3 ? w.ToUpper() : w.Substring(0, 3).ToUpper()));

        return code.Length > 10 ? code.Substring(0, 10) : code;
    }
}
