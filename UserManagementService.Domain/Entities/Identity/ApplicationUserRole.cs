using Microsoft.AspNetCore.Identity;

namespace UserManagementService.Domain.Entities.Identity;

// ✅ ApplicationUserRole - NO navigation properties!
// IdentityUserRole<string> already has UserId and RoleId
// Adding navigation properties creates shadow properties (UserId1, RoleId1)
public class ApplicationUserRole : IdentityUserRole<string>
{
    // ✅ EMPTY - Identity handles the relationship automatically
}