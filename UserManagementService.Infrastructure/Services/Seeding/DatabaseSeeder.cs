using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Domain.Entities.RBAC;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Seeding;

public static class DatabaseSeeder
{
    // ✅ Fixed Super Admin userId for consistent audit tracking
    private const string SuperAdminUserId = "e71a9f33-1832-4055-a852-7beb05ec44d0";
    private static readonly DateTime SeedTimestamp = DateTime.UtcNow;

    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        // ✅ Skip if already seeded (check Apps table)
        if (await context.Apps.AnyAsync())
        {
            Console.WriteLine("⚠️  Database already seeded. Skipping...");
            return;
        }

        Console.WriteLine("🌱 Seeding database (Auth + Apps + RBAC)...");

        try
        {
            // ========================================
            // 1. SEED APPS (with audit fields)
            // ========================================
            Console.WriteLine("📱 Seeding Apps...");

            var userMgmtApp = new App
            {
                Id = Guid.NewGuid(),
                Name = "User Management",
                Code = "USERMAN",  // ✅ Auto-generated code format
                Description = "Manage users, roles, and permissions",
                Icon = "users",
                DisplayOrder = 1,
                IsActive = true,
                CreatedBy = SuperAdminUserId,
                UpdatedBy = SuperAdminUserId,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp
            };

            var apps = new List<App>
            {
                userMgmtApp,
                new App {
                    Id = Guid.NewGuid(),
                    Name = "Asset Management",
                    Code = "ASSETMAN",
                    Description = "Manage company assets",
                    Icon = "box",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedBy = SuperAdminUserId,
                    UpdatedBy = SuperAdminUserId,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = SeedTimestamp
                },
                new App {
                    Id = Guid.NewGuid(),
                    Name = "Attendance",
                    Code = "ATTEND",
                    Description = "Track employee attendance",
                    Icon = "clock",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedBy = SuperAdminUserId,
                    UpdatedBy = SuperAdminUserId,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = SeedTimestamp
                },
                new App {
                    Id = Guid.NewGuid(),
                    Name = "Settings",
                    Code = "SETT",
                    Description = "Company and system settings",
                    Icon = "settings",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedBy = SuperAdminUserId,
                    UpdatedBy = SuperAdminUserId,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = SeedTimestamp
                }
            };

            await context.Apps.AddRangeAsync(apps);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {apps.Count} Apps seeded.");

            // ========================================
            // 2. SEED PAGES (under User Management App)
            // ========================================
            Console.WriteLine("📄 Seeding Pages...");

            var usersPage = new Page
            {
                Id = Guid.NewGuid(),
                AppId = userMgmtApp.Id,
                Name = "Users",
                Code = "USERS",
                Description = "User management page",
                DisplayOrder = 1,
                IsActive = true,
                CreatedBy = SuperAdminUserId,
                UpdatedBy = SuperAdminUserId,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp
            };

            var rolesPage = new Page
            {
                Id = Guid.NewGuid(),
                AppId = userMgmtApp.Id,
                Name = "Roles",
                Code = "ROLES",
                Description = "Role management page",
                DisplayOrder = 2,
                IsActive = true,
                CreatedBy = SuperAdminUserId,
                UpdatedBy = SuperAdminUserId,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp
            };

            var permissionsPage = new Page
            {
                Id = Guid.NewGuid(),
                AppId = userMgmtApp.Id,
                Name = "Permissions",
                Code = "PERMISSIONS",
                Description = "Permission management page",
                DisplayOrder = 3,
                IsActive = true,
                CreatedBy = SuperAdminUserId,
                UpdatedBy = SuperAdminUserId,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp
            };

            var pages = new List<Page> { usersPage, rolesPage, permissionsPage };
            await context.Pages.AddRangeAsync(pages);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {pages.Count} Pages seeded.");

            // ========================================
            // 3. SEED ACTIONS (under each Page)
            // ========================================
            Console.WriteLine("⚡ Seeding Actions...");

            var actions = new List<AppAction>();
            var actionTypes = new[] { "View", "Create", "Edit", "Delete" };

            foreach (var page in pages)
            {
                foreach (var actionType in actionTypes)
                {
                    actions.Add(new AppAction
                    {
                        Id = Guid.NewGuid(),
                        PageId = page.Id,
                        Name = actionType,
                        Code = actionType.ToUpper(),
                        Type = "Button",
                        Description = $"{actionType} action for {page.Name}",
                        DisplayOrder = Array.IndexOf(actionTypes, actionType),
                        IsActive = true,
                        CreatedBy = SuperAdminUserId,
                        UpdatedBy = SuperAdminUserId,
                        CreatedAt = SeedTimestamp,
                        UpdatedAt = SeedTimestamp
                    });
                }
            }

            await context.Actions.AddRangeAsync(actions);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {actions.Count} Actions seeded.");

            // ========================================
            // 4. SEED PERMISSIONS (Auto-generated format)
            // ========================================
            Console.WriteLine("🔐 Seeding Permissions...");

            var permissions = new List<Permission>();

            foreach (var action in actions)
            {
                var page = pages.First(p => p.Id == action.PageId);
                var permissionCode = $"{userMgmtApp.Code}_{page.Code}_{action.Code}";
                var permissionName = $"{userMgmtApp.Name} - {page.Name} - {action.Name}";

                permissions.Add(new Permission
                {
                    Id = Guid.NewGuid(),
                    AppId = userMgmtApp.Id,
                    PageId = page.Id,
                    ActionId = action.Id,
                    PermissionCode = permissionCode,
                    Name = permissionName,
                    IsEnabled = false,  // ⚠️ Default: INACTIVE (Super Admin will activate)
                    CreatedBy = SuperAdminUserId,
                    UpdatedBy = SuperAdminUserId,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = SeedTimestamp
                });
            }

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {permissions.Count} Permissions seeded (all INACTIVE).");

            // ========================================
            // 5. SEED ROLES (Super Admin with Scope)
            // ========================================
            Console.WriteLine("👥 Seeding Roles...");

            var superAdminRole = new ApplicationRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Super Admin",
                NormalizedName = "SUPER ADMIN",
                TenantId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                BranchId = null,
                Scope = RoleScope.Global,  // ✅ NEW: Global scope
                Description = "Full system access - can manage all tenants and roles",
                IsDefault = false,
                CreatedBy = SuperAdminUserId,
                UpdatedBy = SuperAdminUserId,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
                IsDeleted = false
            };

            if (!await roleManager.RoleExistsAsync(superAdminRole.Name!))
            {
                await roleManager.CreateAsync(superAdminRole);
                Console.WriteLine($"✅ Role 'Super Admin' created (Scope: Global).");
            }
            else
            {
                Console.WriteLine($"⚠️  Role 'Super Admin' already exists.");
            }

            await context.SaveChangesAsync();

            // ========================================
            // 6. ACTIVATE CORE PERMISSIONS FOR SUPER ADMIN
            // ========================================
            Console.WriteLine("🔑 Activating core permissions...");

            // Activate ALL permissions for User Management app (Super Admin needs full access)
            var userMgmtPermissions = permissions.Where(p => p.AppId == userMgmtApp.Id).ToList();
            foreach (var perm in userMgmtPermissions)
            {
                perm.IsEnabled = true;  // ✅ Activate for Super Admin
                perm.UpdatedBy = SuperAdminUserId;
                perm.UpdatedAt = SeedTimestamp;
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {userMgmtPermissions.Count} permissions activated for Super Admin.");

            // ========================================
            // 7. ASSIGN PERMISSIONS TO SUPER ADMIN ROLE
            // ========================================
            Console.WriteLine("🔗 Assigning permissions to Super Admin role...");

            var rolePermissions = userMgmtPermissions.Select(p => new RolePermission
            {
                Id = Guid.NewGuid(),
                RoleId = superAdminRole.Id,
                PermissionId = p.Id,
                AssignedAt = SeedTimestamp,
                AssignedBy = SuperAdminUserId,
                CreatedBy = SuperAdminUserId,
                UpdatedBy = SuperAdminUserId,
                CreatedAt = SeedTimestamp,
                UpdatedAt = SeedTimestamp,
                IsDeleted = false
            }).ToList();

            await context.RolePermissions.AddRangeAsync(rolePermissions);
            await context.SaveChangesAsync();
            Console.WriteLine($"✅ {rolePermissions.Count} permissions assigned to Super Admin role.");

            // ========================================
            // 8. SEED SUPER ADMIN USER
            // ========================================
            Console.WriteLine("👤 Seeding Super Admin User...");

            var superAdminEmail = "aneeshsolomon59@gmail.com";
            var superAdminUserName = "aneeshsolomon59";

            var existingUser = await userManager.FindByEmailAsync(superAdminEmail);

            if (existingUser == null)
            {
                var superAdminUser = new ApplicationUser
                {
                    Id = SuperAdminUserId,
                    UserName = superAdminUserName,
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    NormalizedEmail = superAdminEmail.ToUpper(),
                    NormalizedUserName = superAdminUserName.ToUpper(),
                    TenantId = Guid.Parse("00000000-0000-0000-0000-000000000000"),
                    BranchId = null,
                    IsSuperAdmin = true,
                    IsActive = true,
                    IsDeleted = false,
                    IsTemporaryPassword = false,
                    MustChangePassword = false,
                    TemporaryPasswordExpiresAt = null,
                    LastPasswordChangedAt = SeedTimestamp,
                    PasswordChangedCount = 0,
                    LastLoginAt = null,
                    FailedLoginAttempts = 0,
                    LastFailedLoginAt = null,
                    CreatedBy = SuperAdminUserId,
                    UpdatedBy = SuperAdminUserId,
                    CreatedAt = SeedTimestamp,
                    UpdatedAt = SeedTimestamp,
                    DeletedAt = null,
                    DeletedBy = null
                };

                var result = await userManager.CreateAsync(superAdminUser, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "Super Admin");
                    Console.WriteLine($"✅ Super Admin user created: {superAdminEmail}");
                    Console.WriteLine($"   Password: Admin@123");
                    Console.WriteLine($"   UserId: {SuperAdminUserId}");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    Console.WriteLine($"❌ Failed to create Super Admin: {errors}");
                    throw new Exception($"Failed to create Super Admin: {errors}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️  Super Admin user already exists: {superAdminEmail}");
            }

            await context.SaveChangesAsync();

            // ========================================
            // SEEDING COMPLETE
            // ========================================
            Console.WriteLine("");
            Console.WriteLine("✅✅✅ DATABASE SEEDING COMPLETED SUCCESSFULLY! ✅✅✅");
            Console.WriteLine("");
            Console.WriteLine("📊 Seeded Summary:");
            Console.WriteLine($"   - Apps: {apps.Count}");
            Console.WriteLine($"   - Pages: {pages.Count} (under User Management)");
            Console.WriteLine($"   - Actions: {actions.Count} (4 per page)");
            Console.WriteLine($"   - Permissions: {permissions.Count} (auto-generated codes)");
            Console.WriteLine($"   - Roles: 1 (Super Admin - Global scope)");
            Console.WriteLine($"   - Users: 1 (Super Admin)");
            Console.WriteLine($"   - RolePermissions: {rolePermissions.Count}");
            Console.WriteLine("");
            Console.WriteLine("🔐 Permission Codes Generated:");
            foreach (var perm in userMgmtPermissions.Take(5))
            {
                Console.WriteLine($"   - {perm.PermissionCode}");
            }
            Console.WriteLine($"   ... and {userMgmtPermissions.Count - 5} more");
            Console.WriteLine("");
            Console.WriteLine("🔑 Login Credentials:");
            Console.WriteLine($"   Email: {superAdminEmail}");
            Console.WriteLine($"   Password: Admin@123");
            Console.WriteLine("");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Seeding failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }
}