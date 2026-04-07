using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Infrastructure.Persistence;
using UserManagementService.Application.Services;

namespace UserManagementService.Infrastructure.Services.AppPermissions;

    public class AppPermissionResolverService :IAppPermissionResolverService
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
    }

