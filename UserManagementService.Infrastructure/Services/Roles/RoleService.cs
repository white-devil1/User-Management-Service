using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.DTOs.Roles;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Domain.Entities.RBAC;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.Roles;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserDisplayNameResolver _resolver;

    public RoleService(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        IUserDisplayNameResolver resolver)
    {
        _context = context;
        _roleManager = roleManager;
        _resolver = resolver;
    }

    public async Task<RoleResponse> CreateRoleAsync(
        string name, string? description,
        string callerUserId, bool callerIsSuperAdmin,
        Guid callerTenantId, Guid? callerBranchId,
        CancellationToken ct = default)
    {
        var scope = callerIsSuperAdmin ? RoleScope.Global
            : callerBranchId.HasValue ? RoleScope.Branch
            : RoleScope.Tenant;

        var exists = await _context.Roles.AnyAsync(
            r => r.TenantId == callerTenantId && r.Name == name && !r.IsDeleted, ct);
        if (exists)
            throw new ConflictException($"A role named '{name}' already exists in this tenant.");

        var role = new ApplicationRole
        {
            Name = name,
            NormalizedName = name.ToUpper(),
            Description = description,
            TenantId = callerTenantId,
            BranchId = callerBranchId,
            Scope = scope,
            IsDefault = false,
            CreatedBy = callerUserId,
            UpdatedBy = callerUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            throw new ValidationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var names = await BuildNameCacheAsync(new[] { role.CreatedBy, role.UpdatedBy }, ct);
        return MapToResponse(role, new RolePermissionsGrouped(), names);
    }

    public async Task<RoleResponse> UpdateRoleAsync(
        string roleId, string name, string? description,
        string callerUserId, bool callerIsSuperAdmin,
        Guid callerTenantId, CancellationToken ct = default)
    {
        var role = await GetAndGuardRole(roleId, callerIsSuperAdmin, callerTenantId, ct);

        var nameTaken = await _context.Roles.AnyAsync(
            r => r.TenantId == callerTenantId && r.Name == name && r.Id != roleId && !r.IsDeleted, ct);
        if (nameTaken)
            throw new ConflictException($"A role named '{name}' already exists in this tenant.");

        role.Name = name;
        role.NormalizedName = name.ToUpper();
        role.Description = description;
        role.UpdatedBy = callerUserId;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleManager.UpdateAsync(role);

        var permissions = await GetRolePermissionsGrouped(roleId, ct);
        var names = await BuildNameCacheAsync(new[] { role.CreatedBy, role.UpdatedBy }, ct);
        return MapToResponse(role, permissions, names);
    }

    public async Task<bool> DeleteRoleAsync(
        string roleId, string callerUserId,
        bool callerIsSuperAdmin, Guid callerTenantId,
        CancellationToken ct = default)
    {
        var role = await GetAndGuardRole(roleId, callerIsSuperAdmin, callerTenantId, ct);

        var isAssigned = await _context.UserRoles.AnyAsync(ur => ur.RoleId == roleId, ct);
        if (isAssigned)
            throw new ConflictException(
                "Cannot delete this role because it is assigned to one or more users. Remove the role from all users first.");

        role.IsDeleted = true;
        role.DeletedAt = DateTime.UtcNow;
        role.DeletedBy = callerUserId;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleManager.UpdateAsync(role);
        return true;
    }

    public async Task<RoleResponse> GetRoleByIdAsync(
        string roleId, bool callerIsSuperAdmin,
        Guid callerTenantId, CancellationToken ct = default)
    {
        var role = await GetAndGuardRole(roleId, callerIsSuperAdmin, callerTenantId, ct);
        var permissions = await GetRolePermissionsGrouped(roleId, ct);
        var names = await BuildNameCacheAsync(new[] { role.CreatedBy, role.UpdatedBy, role.DeletedBy }, ct);
        return MapToResponse(role, permissions, names);
    }

    public async Task<RoleListResponse> ListRolesAsync(
        string? search, bool? isDeleted,
        bool callerIsSuperAdmin, Guid callerTenantId,
        int page, int pageSize, string sortBy, string sortOrder,
        CancellationToken ct = default)
    {
        var query = _context.Roles.AsQueryable();

        if (!callerIsSuperAdmin)
            query = query.Where(r => r.TenantId == callerTenantId);

        if (isDeleted.HasValue)
            query = query.Where(r => r.IsDeleted == isDeleted.Value);
        else
            query = query.Where(r => !r.IsDeleted);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(r => r.Name!.Contains(search));

        query = sortOrder.ToLower() == "asc"
            ? query.OrderBy(r => r.Name)
            : query.OrderByDescending(r => r.Name);

        var total = await query.CountAsync(ct);
        var roles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var allIds = roles.SelectMany(r => new[] { r.CreatedBy, r.UpdatedBy, r.DeletedBy });
        var names = await BuildNameCacheAsync(allIds, ct);

        var responses = new List<RoleResponse>();
        foreach (var role in roles)
        {
            var perms = await GetRolePermissionsGrouped(role.Id, ct);
            responses.Add(MapToResponse(role, perms, names));
        }

        return new RoleListResponse
        {
            Roles = responses,
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
    }

    public async Task<RoleResponse> AssignPermissionsAsync(
        string roleId, List<Guid> permissionIds,
        string callerUserId, bool callerIsSuperAdmin,
        Guid callerTenantId, CancellationToken ct = default)
    {
        var role = await GetAndGuardRole(roleId, callerIsSuperAdmin, callerTenantId, ct);

        if (!callerIsSuperAdmin)
        {
            var callerRoleIds = await _context.UserRoles
                .Where(ur => _context.Users
                    .Where(u => u.Id == callerUserId)
                    .Select(u => u.Id)
                    .Contains(ur.UserId))
                .Select(ur => ur.RoleId)
                .ToListAsync(ct);

            var callerPermissionIds = await _context.RolePermissions
                .Where(rp => callerRoleIds.Contains(rp.RoleId) && !rp.IsDeleted)
                .Join(_context.Permissions,
                    rp => rp.PermissionId, p => p.Id, (rp, p) => p)
                .Where(p => p.IsEnabled && !p.IsDeleted)
                .Select(p => p.Id)
                .ToListAsync(ct);

            var unauthorized = permissionIds.Except(callerPermissionIds).ToList();
            if (unauthorized.Any())
                throw new UnauthorizedException("You cannot assign permissions you do not own.");
        }

        var validPerms = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id) && p.IsEnabled && !p.IsDeleted)
            .ToListAsync(ct);

        if (validPerms.Count != permissionIds.Count)
            throw new ValidationException("One or more permissions are invalid or not enabled.");

        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .ToListAsync(ct);
        foreach (var rp in existing)
        {
            rp.IsDeleted = true;
            rp.DeletedAt = DateTime.UtcNow;
            rp.DeletedBy = callerUserId;
        }

        var newAssignments = permissionIds.Select(pid => new RolePermission
        {
            Id = Guid.NewGuid(),
            RoleId = roleId,
            PermissionId = pid,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = callerUserId,
            CreatedBy = callerUserId,
            UpdatedBy = callerUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await _context.RolePermissions.AddRangeAsync(newAssignments, ct);
        await _context.SaveChangesAsync(ct);

        var permCodes = await GetRolePermissionsGrouped(roleId, ct);
        var names = await BuildNameCacheAsync(new[] { role.CreatedBy, role.UpdatedBy }, ct);
        return MapToResponse(role, permCodes, names);
    }

    public async Task<RolePermissionsGrouped> GetAvailablePermissionsAsync(
        string callerUserId, bool callerIsSuperAdmin,
        CancellationToken ct = default)
    {
        List<Domain.Entities.RBAC.Permission> permissions;

        if (callerIsSuperAdmin)
        {
            permissions = await _context.Permissions
                .Include(p => p.App)
                .Include(p => p.Page)
                .Include(p => p.Action)
                .Where(p => p.IsEnabled && !p.IsDeleted)
                .ToListAsync(ct);
        }
        else
        {
            var callerRoleIds = await _context.UserRoles
                .Where(ur => ur.UserId == callerUserId)
                .Select(ur => ur.RoleId)
                .ToListAsync(ct);

            var assignedPermissionIds = await _context.RolePermissions
                .Where(rp => callerRoleIds.Contains(rp.RoleId) && !rp.IsDeleted)
                .Select(rp => rp.PermissionId)
                .ToListAsync(ct);

            permissions = await _context.Permissions
                .Include(p => p.App)
                .Include(p => p.Page)
                .Include(p => p.Action)
                .Where(p => assignedPermissionIds.Contains(p.Id) && p.IsEnabled && !p.IsDeleted)
                .ToListAsync(ct);
        }

        var grouped = new RolePermissionsGrouped();

        foreach (var appGroup in permissions
            .GroupBy(p => new { p.AppId, AppName = p.App?.Name ?? "Unknown" })
            .OrderBy(g => g.Key.AppName))
        {
            var appDto = new RoleAppPermissionsDto
            {
                AppId = appGroup.Key.AppId,
                AppName = appGroup.Key.AppName
            };

            foreach (var pageGroup in appGroup
                .GroupBy(p => new { p.PageId, PageName = p.Page?.Name ?? "Unknown" })
                .OrderBy(g => g.Key.PageName))
            {
                appDto.Pages.Add(new RolePagePermissionsDto
                {
                    PageId = pageGroup.Key.PageId,
                    PageName = pageGroup.Key.PageName,
                    Permissions = pageGroup
                        .OrderBy(p => p.Action?.Name)
                        .Select(p => new RolePermissionActionDto
                        {
                            Id = p.Id,
                            ActionName = p.Action?.Name ?? "Unknown"
                        })
                        .ToList()
                });
            }

            grouped.Apps.Add(appDto);
        }

        return grouped;
    }

    private async Task<Dictionary<string, string>> BuildNameCacheAsync(
        IEnumerable<string?> userIds, CancellationToken ct)
    {
        var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct()!)
            cache[id!] = await _resolver.ResolveAsync(id, ct);
        return cache;
    }

    private async Task<ApplicationRole> GetAndGuardRole(
        string roleId, bool callerIsSuperAdmin,
        Guid callerTenantId, CancellationToken ct)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, ct);

        if (role == null || role.IsDeleted)
            throw new NotFoundException("Role", roleId);

        if (!callerIsSuperAdmin && role.TenantId != callerTenantId)
            throw new UnauthorizedException("You do not have access to this role.");

        return role;
    }

    private async Task<RolePermissionsGrouped> GetRolePermissionsGrouped(string roleId, CancellationToken ct)
    {
        var assignedPermissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Join(_context.Permissions
                    .Include(p => p.App)
                    .Include(p => p.Page)
                    .Include(p => p.Action),
                rp => rp.PermissionId, p => p.Id, (rp, p) => p)
            .ToListAsync(ct);

        var grouped = new RolePermissionsGrouped();

        foreach (var appGroup in assignedPermissions
            .GroupBy(p => new { p.AppId, AppName = p.App?.Name ?? "Unknown" })
            .OrderBy(g => g.Key.AppName))
        {
            var appDto = new RoleAppPermissionsDto
            {
                AppId = appGroup.Key.AppId,
                AppName = appGroup.Key.AppName
            };

            foreach (var pageGroup in appGroup
                .GroupBy(p => new { p.PageId, PageName = p.Page?.Name ?? "Unknown" })
                .OrderBy(g => g.Key.PageName))
            {
                appDto.Pages.Add(new RolePagePermissionsDto
                {
                    PageId = pageGroup.Key.PageId,
                    PageName = pageGroup.Key.PageName,
                    Permissions = pageGroup
                        .OrderBy(p => p.Action?.Name)
                        .Select(p => new RolePermissionActionDto
                        {
                            Id = p.Id,
                            ActionName = p.Action?.Name ?? "Unknown"
                        })
                        .ToList()
                });
            }

            grouped.Apps.Add(appDto);
        }

        return grouped;
    }

    private static RoleResponse MapToResponse(
        ApplicationRole role, RolePermissionsGrouped permissions, Dictionary<string, string> names) => new()
    {
        Id = role.Id,
        Name = role.Name!,
        Description = role.Description,
        Scope = role.Scope.ToString(),
        TenantId = role.TenantId,
        BranchId = role.BranchId,
        IsDefault = role.IsDefault,
        IsDeleted = role.IsDeleted,
        CreatedAt = role.CreatedAt,
        CreatedBy = role.CreatedBy != null && names.TryGetValue(role.CreatedBy, out var cb) ? cb : null,
        UpdatedAt = role.UpdatedAt,
        UpdatedBy = role.UpdatedBy != null && names.TryGetValue(role.UpdatedBy, out var ub) ? ub : null,
        Permissions = permissions
    };
}
