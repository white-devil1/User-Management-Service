using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserManagementService.Application.DTOs.Users;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.User;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserDisplayNameResolver _resolver;

    public UserService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IUserDisplayNameResolver resolver)
    {
        _context = context;
        _userManager = userManager;
        _resolver = resolver;
    }

    public async Task<UserListResponse> GetUsersAsync(
        string? search,
        List<string> status,
        Guid? tenantId,
        Guid? branchId,
        string? roleId,
        int page,
        int pageSize,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var query = _context.Users.AsQueryable();

        if (status.Any())
        {
            var isActive = status.Contains("active");
            var isDeactivated = status.Contains("deactivated");
            var isDeleted = status.Contains("deleted");

            query = query.Where(u =>
                (isActive && u.IsActive && !u.IsDeleted) ||
                (isDeactivated && !u.IsActive && !u.IsDeleted) ||
                (isDeleted && u.IsDeleted)
            );
        }

        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(u =>
                u.Email!.ToLower().Contains(searchLower) ||
                u.UserName!.ToLower().Contains(searchLower) ||
                (u.FirstName != null && u.FirstName.ToLower().Contains(searchLower)) ||
                (u.LastName != null && u.LastName.ToLower().Contains(searchLower))
            );
        }

        if (tenantId.HasValue)
            query = query.Where(u => u.TenantId == tenantId.Value);

        if (branchId.HasValue)
            query = query.Where(u => u.BranchId == branchId.Value);

        if (!string.IsNullOrEmpty(roleId))
        {
            var roleUsers = await _context.UserRoles
                .Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken);

            query = query.Where(u => roleUsers.Contains(u.Id));
        }

        query = sortOrder.ToLower() == "asc"
            ? ApplySortingAscending(query, sortBy)
            : ApplySortingDescending(query, sortBy);

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Batch-resolve all unique audit UUIDs in one pass
        var allIds = users.SelectMany(u => new[] { u.CreatedBy, u.UpdatedBy, u.DeletedBy });
        var names = await BuildNameCacheAsync(allIds, cancellationToken);

        var userResponses = new List<UserResponse>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userResponses.Add(MapToUserResponse(user, roles.ToList(), names));
        }

        return new UserListResponse
        {
            Users = userResponses,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserResponse?> GetUserByIdWithRolesAsync(
        string userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null) return null;

        var names = await BuildNameCacheAsync(new[] { user.CreatedBy, user.UpdatedBy, user.DeletedBy }, cancellationToken);
        var roles = await _userManager.GetRolesAsync(user);
        return MapToUserResponse(user, roles.ToList(), names);
    }

    public async Task<List<RoleDto>> GetAvailableRolesAsync(
        bool isSuperAdmin, string? tenantId, CancellationToken cancellationToken)
    {
        var query = _context.Roles
            .Where(r => !r.IsDeleted)
            .AsQueryable();

        if (!isSuperAdmin && !string.IsNullOrEmpty(tenantId))
        {
            var tid = Guid.Parse(tenantId);
            query = query.Where(r => r.TenantId == tid);
        }

        return await query.Select(r => new RoleDto
        {
            Id = r.Id,
            Name = r.Name!,
            TenantId = r.TenantId,
            BranchId = r.BranchId,
            Description = r.Description,
            Scope = r.Scope.ToString(),
            IsDefault = r.IsDefault
        }).ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<string, string>> BuildNameCacheAsync(
        IEnumerable<string?> userIds, CancellationToken ct)
    {
        var cache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in userIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct()!)
            cache[id!] = await _resolver.ResolveAsync(id, ct);
        return cache;
    }

    private static UserResponse MapToUserResponse(
        ApplicationUser user, List<string> roles, Dictionary<string, string> names) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        UserName = user.UserName!,
        FirstName = user.FirstName,
        LastName = user.LastName,
        ProfileImagePath = user.ProfileImagePath,
        ProfileThumbPath = user.ProfileThumbPath,
        TenantId = user.TenantId,
        BranchId = user.BranchId,
        IsSuperAdmin = user.IsSuperAdmin,
        IsActive = user.IsActive,
        IsDeleted = user.IsDeleted,
        IsTemporaryPassword = user.IsTemporaryPassword,
        LastLoginAt = user.LastLoginAt,
        CreatedAt = user.CreatedAt,
        CreatedBy = user.CreatedBy != null && names.TryGetValue(user.CreatedBy, out var cb) ? cb : null,
        UpdatedAt = user.UpdatedAt,
        UpdatedBy = user.UpdatedBy != null && names.TryGetValue(user.UpdatedBy, out var ub) ? ub : null,
        DeletedAt = user.DeletedAt,
        DeletedBy = user.DeletedBy != null && names.TryGetValue(user.DeletedBy, out var db) ? db : null,
        Roles = roles
    };

    private IQueryable<ApplicationUser> ApplySortingAscending(IQueryable<ApplicationUser> query, string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "email" => query.OrderBy(u => u.Email),
            "username" => query.OrderBy(u => u.UserName),
            "firstname" => query.OrderBy(u => u.FirstName),
            "lastname" => query.OrderBy(u => u.LastName),
            "createdat" => query.OrderBy(u => u.CreatedAt),
            "updatedat" => query.OrderBy(u => u.UpdatedAt),
            "lastloginat" => query.OrderBy(u => u.LastLoginAt),
            _ => query.OrderBy(u => u.CreatedAt)
        };
    }

    private IQueryable<ApplicationUser> ApplySortingDescending(IQueryable<ApplicationUser> query, string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "email" => query.OrderByDescending(u => u.Email),
            "username" => query.OrderByDescending(u => u.UserName),
            "firstname" => query.OrderByDescending(u => u.FirstName),
            "lastname" => query.OrderByDescending(u => u.LastName),
            "createdat" => query.OrderByDescending(u => u.CreatedAt),
            "updatedat" => query.OrderByDescending(u => u.UpdatedAt),
            "lastloginat" => query.OrderByDescending(u => u.LastLoginAt),
            _ => query.OrderByDescending(u => u.CreatedAt)
        };
    }
}
