using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.DTOs.AppPermissions;
using UserManagementService.Application.DTOs.Auth;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.AppPermissions;

public class AppPermissionResolverService : IAppPermissionResolverService
{
    private readonly ApplicationDbContext _context;

    public AppPermissionResolverService(ApplicationDbContext context)
        => _context = context;

    public async Task<List<string>> GetUserPermissionsAsync(
        ApplicationUser user,
        IList<string> roles,
        CancellationToken ct = default)
    {
        // SuperAdmin bypasses permission lookup — IsSuperAdmin claim handles it
        if (user.IsSuperAdmin) return new List<string>();

        var roleIds = await _context.Roles
            .Where(r => roles.Contains(r.Name!) && !r.IsDeleted)
            .Select(r => r.Id)
            .ToListAsync(ct);

        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted)
            .Join(_context.Permissions,
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p)
            .Where(p => p.IsEnabled && !p.IsDeleted)
            .Select(p => p.PermissionCode)
            .Distinct()
            .ToListAsync(ct);

        return permissions;
    }

    public async Task<UserAccessDto> GetUserAccessAsync(
        ApplicationUser user,
        IList<string> roles,
        CancellationToken ct = default)
    {
        // SuperAdmin: skip the tree — IsSuperAdmin already grants full access.
        if (user.IsSuperAdmin)
        {
            return new UserAccessDto();
        }

        var roleIds = await _context.Roles
            .Where(r => roles.Contains(r.Name!) && !r.IsDeleted)
            .Select(r => r.Id)
            .ToListAsync(ct);

        // Pull every enabled permission the user has via their roles, with the
        // app/page/action metadata needed to mirror the role-permissions tree.
        var permissionRows = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId) && !rp.IsDeleted)
            .Join(_context.Permissions,
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p)
            .Where(p => p.IsEnabled && !p.IsDeleted
                && p.App!.IsActive && !p.App!.IsDeleted
                && p.Page!.IsActive && !p.Page!.IsDeleted)
            .Select(p => new
            {
                PermissionId = p.Id,
                p.PermissionCode,
                p.AppId,
                AppName = p.App!.Name,
                AppCode = p.App!.Code,
                AppDisplayOrder = p.App!.DisplayOrder,
                p.PageId,
                PageName = p.Page!.Name,
                PageDisplayOrder = p.Page!.DisplayOrder,
                ActionName = p.Action!.Name
            })
            .Distinct()
            .ToListAsync(ct);

        var permissionCodes = permissionRows
            .Select(r => r.PermissionCode)
            .Distinct()
            .ToList();

        var apps = permissionRows
            .GroupBy(r => new { r.AppId, r.AppName, r.AppCode, r.AppDisplayOrder })
            .OrderBy(g => g.Key.AppDisplayOrder)
            .ThenBy(g => g.Key.AppName)
            .Select(g => new UserAppDto
            {
                AppId = g.Key.AppId,
                AppName = g.Key.AppName,
                Code = g.Key.AppCode
            })
            .ToList();

        return new UserAccessDto
        {
            Permissions = permissionCodes,
            Apps = apps
        };
    }
}
