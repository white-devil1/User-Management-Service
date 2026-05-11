using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.Identity;

public class SoftDeleteAwareRoleValidator : IRoleValidator<ApplicationRole>
{
    private readonly ApplicationDbContext _context;

    public SoftDeleteAwareRoleValidator(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IdentityResult> ValidateAsync(
        RoleManager<ApplicationRole> manager, ApplicationRole role)
    {
        if (string.IsNullOrWhiteSpace(role.Name))
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidRoleName",
                Description = "Role name cannot be empty."
            });

        // Check for duplicate name within the same tenant, ignoring soft-deleted roles
        var duplicate = await _context.Roles
            .IgnoreQueryFilters()
            .AnyAsync(r =>
                r.TenantId == role.TenantId &&
                r.NormalizedName == role.NormalizedName &&
                r.Id != role.Id &&
                !r.IsDeleted);

        if (duplicate)
            return IdentityResult.Failed(new IdentityError
            {
                Code = "DuplicateRoleName",
                Description = $"Role name '{role.Name}' is already taken."
            });

        return IdentityResult.Success;
    }
}
