using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.AppPermissions;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.RBAC;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.AppPermissions;

public class AppPermissionService : IAppPermissionService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserDisplayNameResolver _resolver;

    public AppPermissionService(ApplicationDbContext context, IUserDisplayNameResolver resolver)
    {
        _context = context;
        _resolver = resolver;
    }

    public async Task<AppPermissionListResponse> GetPermissionsAsync(
        Guid? appId,
        Guid? pageId,
        Guid? actionId,
        bool? isEnabled,
        bool includeDeleted,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Permissions.AsQueryable();

        // ✅ Apply Filters
        if (appId.HasValue)
        {
            query = query.Where(p => p.AppId == appId.Value);
        }

        if (pageId.HasValue)
        {
            query = query.Where(p => p.PageId == pageId.Value);
        }

        if (actionId.HasValue)
        {
            query = query.Where(p => p.ActionId == actionId.Value);
        }

        if (isEnabled.HasValue)
        {
            query = query.Where(p => p.IsEnabled == isEnabled.Value);
        }

        // ✅ Include deleted only if requested (Super Admin)
        if (!includeDeleted)
        {
            query = query.Where(p => !p.IsDeleted);
        }

        // ✅ Get total count before pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // ✅ Apply Sorting
        query = sortBy.ToLower() switch
        {
            "permissioncode" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(p => p.PermissionCode)
                : query.OrderByDescending(p => p.PermissionCode),
            "name" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(p => p.Name)
                : query.OrderByDescending(p => p.Name),
            "createdat" => sortOrder.ToLower() == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt),
            _ => sortOrder.ToLower() == "asc"
                ? query.OrderBy(p => p.CreatedAt)
                : query.OrderByDescending(p => p.CreatedAt)
        };

        // ✅ Apply Pagination
        var permissions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var permissionDtos = permissions.Select(MapToDto).ToList();

        return new AppPermissionListResponse
        {
            Permissions = permissionDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<AppPermissionDto> GetPermissionByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (permission == null)
        {
            throw new NotFoundException("Permission", id);
        }

        return MapToDto(permission);
    }

    public async Task<AppPermissionDto> TogglePermissionAsync(
        Guid id,
        bool isEnabled,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var updatedByName = await _resolver.ResolveAsync(updatedBy, cancellationToken);

        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (permission == null)
        {
            throw new NotFoundException("Permission", id);
        }

        permission.IsEnabled = isEnabled;
        permission.UpdatedBy = updatedByName;
        permission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(permission);
    }

    public async Task<TogglePermissionResponseDto> TogglePermissionWithActionNameAsync(
        Guid id,
        bool isEnabled,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var updatedByName = await _resolver.ResolveAsync(updatedBy, cancellationToken);

        var permission = await _context.Permissions
            .Include(p => p.Action).ThenInclude(a => a!.Page).ThenInclude(p => p!.App)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (permission == null)
        {
            throw new NotFoundException("Permission", id);
        }

        permission.IsEnabled = isEnabled;
        permission.UpdatedBy = updatedByName;
        permission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new TogglePermissionResponseDto
        {
            Id = permission.Id,
            ActionName = permission.Action?.Name ?? "Unknown",
            IsEnabled = permission.IsEnabled,
            UpdatedAt = permission.UpdatedAt,
            UpdatedBy = permission.UpdatedBy
        };
    }

    public async Task<GroupedPermissionResponse> GetGroupedPermissionsAsync(
        Guid? appId,
        Guid? pageId,
        bool? isEnabled,
        CancellationToken cancellationToken = default)
    {
        // ✅ Load permissions with navigation properties
        var query = _context.Permissions
            .Include(p => p.Action).ThenInclude(a => a!.Page).ThenInclude(p => p!.App)
            .AsQueryable();

        // ✅ Always filter out soft-deleted (no includeDeleted param)
        query = query.Where(p => !p.IsDeleted);

        // ✅ Apply appId filter ONLY if provided
        if (appId.HasValue)
        {
            query = query.Where(p => p.AppId == appId.Value);
        }

        // ✅ Apply pageId filter ONLY if provided
        if (pageId.HasValue)
        {
            query = query.Where(p => p.PageId == pageId.Value);
        }

        // ✅ Apply isEnabled filter ONLY if provided
        if (isEnabled.HasValue)
        {
            query = query.Where(p => p.IsEnabled == isEnabled.Value);
        }

        var permissions = await query.ToListAsync(cancellationToken);

        // ✅ Group in memory: App → Page → Permission
        var groupedResponse = new GroupedPermissionResponse();

        var appGroups = permissions
            .GroupBy(p => new { p.AppId, AppName = GetAppName(p) })
            .OrderBy(g => g.Key.AppName);

        foreach (var appGroup in appGroups)
        {
            var appDto = new GroupedAppDto
            {
                AppId = appGroup.Key.AppId,
                AppName = appGroup.Key.AppName
            };

            var pageGroups = appGroup
                .GroupBy(p => new { p.PageId, PageName = GetPageName(p) })
                .OrderBy(g => g.Key.PageName);

            foreach (var pageGroup in pageGroups)
            {
                var pageDto = new GroupedPageDto
                {
                    PageId = pageGroup.Key.PageId,
                    PageName = pageGroup.Key.PageName,
                    Permissions = pageGroup
                        .OrderBy(p => p.Action?.Name)
                        .Select(p => new GroupedPermissionDto
                        {
                            Id = p.Id,
                            ActionName = p.Action?.Name ?? "Unknown",
                            IsEnabled = p.IsEnabled
                        })
                        .ToList()
                };

                appDto.Pages.Add(pageDto);
            }

            groupedResponse.Apps.Add(appDto);
        }

        return groupedResponse;
    }

    // ✅ Helper: Resolve App name from Permission (uses Name field as fallback)
    private static string GetAppName(Permission p)
    {
        if (p.Action?.Page?.App?.Name != null) return p.Action.Page.App.Name;
        // Fallback: extract from Name field (format: "AppName - PageName - ActionName")
        if (!string.IsNullOrEmpty(p.Name))
        {
            var parts = p.Name.Split(" - ");
            if (parts.Length >= 1) return parts[0];
        }
        return "Unknown App";
    }

    // ✅ Helper: Resolve Page name from Permission (uses Name field as fallback)
    private static string GetPageName(Permission p)
    {
        if (p.Action?.Page?.Name != null) return p.Action.Page.Name;
        // Fallback: extract from Name field
        if (!string.IsNullOrEmpty(p.Name))
        {
            var parts = p.Name.Split(" - ");
            if (parts.Length >= 2) return parts[1];
        }
        return "Unknown Page";
    }

    // ✅ Helper: Map Entity to DTO
    private static AppPermissionDto MapToDto(Permission permission)
    {
        return new AppPermissionDto
        {
            Id = permission.Id,
            AppId = permission.AppId,
            PageId = permission.PageId,
            ActionId = permission.ActionId,
            PermissionCode = permission.PermissionCode,
            Name = permission.Name,
            IsEnabled = permission.IsEnabled,
            CreatedAt = permission.CreatedAt,
            CreatedBy = permission.CreatedBy,
            UpdatedAt = permission.UpdatedAt,
            UpdatedBy = permission.UpdatedBy,
            IsDeleted = permission.IsDeleted,
            DeletedAt = permission.DeletedAt,
            DeletedBy = permission.DeletedBy
        };
    }

    public async Task<BulkTogglePermissionResponse> BulkTogglePermissionStatusAsync(
        List<BulkToggleItem> permissionStatuses,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        var updatedByName = await _resolver.ResolveAsync(updatedBy, cancellationToken);
        var results = new List<TogglePermissionResponseDto>();
        var now = DateTime.UtcNow;

        foreach (var item in permissionStatuses)
        {
            var permission = await _context.Permissions
                .Include(p => p.Action).ThenInclude(a => a!.Page).ThenInclude(p => p!.App)
                .FirstOrDefaultAsync(p => p.Id == item.PermissionId, cancellationToken);

            if (permission == null)
            {
                throw new NotFoundException("Permission", item.PermissionId);
            }

            permission.IsEnabled = item.IsEnabled;
            permission.UpdatedBy = updatedByName;
            permission.UpdatedAt = now;

            results.Add(new TogglePermissionResponseDto
            {
                Id = permission.Id,
                ActionName = permission.Action?.Name ?? "Unknown",
                IsEnabled = permission.IsEnabled,
                UpdatedAt = permission.UpdatedAt,
                UpdatedBy = permission.UpdatedBy
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new BulkTogglePermissionResponse
        {
            UpdatedCount = results.Count,
            Results = results
        };
    }
}