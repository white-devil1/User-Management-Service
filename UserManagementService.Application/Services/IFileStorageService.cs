namespace UserManagementService.Application.Services;

public interface IFileStorageService
{
    Task<string?> SaveProfileImageAsync(byte[] data, string userId, string imageType, string extension, CancellationToken cancellationToken = default);
    Task DeleteProfileImagesAsync(string userId);
}
