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

    public AppPermissionService(ApplicationDbContext context)
    {
        _context = context;
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
        var permission = await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (permission == null)
        {
            throw new NotFoundException("Permission", id);
        }

        permission.IsEnabled = isEnabled;
        permission.UpdatedBy = updatedBy;
        permission.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(permission);
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
}