namespace UserManagementService.Application.Services;

public interface IUserDisplayNameResolver
{
    /// <summary>
    /// Resolves a user UUID to "FirstName LastName". Returns empty string if not found.
    /// </summary>
    Task<string> ResolveAsync(string? userId, CancellationToken cancellationToken = default);
}
