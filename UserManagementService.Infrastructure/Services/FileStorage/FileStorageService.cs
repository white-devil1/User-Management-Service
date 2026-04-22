using Microsoft.AspNetCore.Hosting;
using UserManagementService.Application.Services;

namespace UserManagementService.Infrastructure.Services.FileStorage;

public class FileStorageService : IFileStorageService
{
    private readonly string _webRootPath;

    public FileStorageService(IWebHostEnvironment env)
    {
        _webRootPath = env.WebRootPath
            ?? Path.Combine(env.ContentRootPath, "wwwroot");
    }

    public async Task<string?> SaveProfileImageAsync(
        byte[] data, string userId, string imageType, string extension,
        CancellationToken cancellationToken = default)
    {
        if (data.Length == 0) return null;

        var folder = Path.Combine(_webRootPath, "uploads", "profiles", userId);
        Directory.CreateDirectory(folder);

        var fileName = $"{imageType}{extension}";
        var fullPath = Path.Combine(folder, fileName);

        await File.WriteAllBytesAsync(fullPath, data, cancellationToken);

        return $"uploads/profiles/{userId}/{fileName}";
    }

    public Task DeleteProfileImagesAsync(string userId)
    {
        var folder = Path.Combine(_webRootPath, "uploads", "profiles", userId);
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
        return Task.CompletedTask;
    }
}
