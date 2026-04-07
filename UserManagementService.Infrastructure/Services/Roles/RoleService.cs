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

    public RoleService(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager)
    {
        _context = context;
        _roleManager = roleManager;
    }

    public async Task<RoleResponse> CreateRoleAsync(
        string name, string? description,
        string callerUserId, bool callerIsSuperAdmin,
        Guid callerTenantId, Guid? callerBranchId,
        CancellationToken ct = default)
    {
        // Determine scope from caller identity
        var scope = callerIsSuperAdmin ? RoleScope.Global
            : callerBranchId.HasValue ? RoleScope.Branch
            : RoleScope.Tenant;

        // Uniqueness check: same name cannot exist twice in same tenant (active roles)
        var exists = await _context.Roles.AnyAsync(
            r => r.TenantId == callerTenantId &&
                 r.Name == name &&
                 !r.IsDeleted, ct);
        if (exists)
            throw new ConflictException(
                $"A role named '{name}' already exists in this tenant.");


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
            throw new ValidationException(string.Join(", ",
                result.Errors.Select(e => e.Description)));


        return MapToResponse(role, new List<string>());
    }

    public async Task<RoleResponse> UpdateRoleAsync(
        string roleId, string name, string? description,
        string callerUserId, bool callerIsSuperAdmin,
        Guid callerTenantId, CancellationToken ct = default)
    {
        var role = await GetAndGuardRole(
            roleId, callerIsSuperAdmin, callerTenantId, ct);

        // Check new name is not taken by another role in the same tenant
        var nameTaken = await _context.Roles.AnyAsync(
            r => r.TenantId == callerTenantId &&
                 r.Name == name &&
                 r.Id != roleId &&
                 !r.IsDeleted, ct);
        if (nameTaken)
            throw new ConflictException(
                $"A role named '{name}' already exists in this tenant.");

        role.Name = name;
        role.NormalizedName = name.ToUpper();
        role.Description = description;
        role.UpdatedBy = callerUserId;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleManager.UpdateAsync(role);

        var permissions = await GetRolePermissionCodes(roleId, ct);
        return MapToResponse(role, permissions);
    }

    public async Task<bool> DeleteRoleAsync(
        string roleId, string callerUserId,
        bool callerIsSuperAdmin, Guid callerTenantId,
        CancellationToken ct = default)
    {
        var role = await GetAndGuardRole(
            roleId, callerIsSuperAdmin, callerTenantId, ct);

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
        var role = await GetAndGuardRole(
            roleId, callerIsSuperAdmin, callerTenantId, ct);
        var permissions = await GetRolePermissionCodes(roleId, ct);
        return MapToResponse(role, permissions);
    }

    public async Task<RoleListResponse> ListRolesAsync(
        string? search, bool? isDeleted,
        bool callerIsSuperAdmin, Guid callerTenantId,
        int page, int pageSize, string sortBy, string sortOrder,
        CancellationToken ct = default)
    {
        var query = _context.Roles.AsQueryable();

        // Scope: SuperAdmin sees all, others see only their tenant
        if (!callerIsSuperAdmin)
            query = query.Where(r => r.TenantId == callerTenantId);

        // Soft delete filter
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

        var responses = new List<RoleResponse>();
        foreach (var role in roles)
        {
            var perms = await GetRolePermissionCodes(role.Id, ct);
            responses.Add(MapToResponse(role, perms));
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
        var role = await GetAndGuardRole(
            roleId, callerIsSuperAdmin, callerTenantId, ct);

        // ── CRITICAL: 'you can only assign what you own' rule ──
        // SuperAdmin bypasses this check — they own all permissions
        if (!callerIsSuperAdmin)
        {
            // Load all permission IDs the caller currently owns
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

            // Reject any permission the caller does not own
            var unauthorized = permissionIds
                .Except(callerPermissionIds).ToList();
            if (unauthorized.Any())
                throw new UnauthorizedException(
                    "You cannot assign permissions you do not own.");
        }

        // Validate all permission IDs exist and are enabled
        var validPerms = await _context.Permissions
            .Where(p => permissionIds.Contains(p.Id) &&
                        p.IsEnabled && !p.IsDeleted)
            .ToListAsync(ct);

        if (validPerms.Count != permissionIds.Count)
            throw new ValidationException(
                "One or more permissions are invalid or not enabled.");

        // Remove existing assignments for this role (full replace)
        var existing = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .ToListAsync(ct);
        foreach (var rp in existing)
        {
            rp.IsDeleted = true;
            rp.DeletedAt = DateTime.UtcNow;
            rp.DeletedBy = callerUserId;
        }

        // Add new assignments
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

        var permCodes = await GetRolePermissionCodes(roleId, ct);
        return MapToResponse(role, permCodes);
    }

    // ── Private helpers ──

    private async Task<ApplicationRole> GetAndGuardRole(
        string roleId, bool callerIsSuperAdmin,
        Guid callerTenantId, CancellationToken ct)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == roleId, ct);

        if (role == null || role.IsDeleted)
            throw new NotFoundException("Role", roleId);

        // Non-SuperAdmin can only touch roles in their own tenant
        if (!callerIsSuperAdmin && role.TenantId != callerTenantId)
            throw new UnauthorizedException(
                "You do not have access to this role.");

        return role;
    }

    private async Task<List<string>> GetRolePermissionCodes(
        string roleId, CancellationToken ct)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
            .Join(_context.Permissions,
                rp => rp.PermissionId, p => p.Id, (rp, p) => p.PermissionCode)
            .ToListAsync(ct);
    }

    private static RoleResponse MapToResponse(
        ApplicationRole role, List<string> permissions)
    {
        return new RoleResponse
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
            CreatedBy = role.CreatedBy,
            UpdatedAt = role.UpdatedAt,
            UpdatedBy = role.UpdatedBy,
            Permissions = permissions
        };
    }
}
