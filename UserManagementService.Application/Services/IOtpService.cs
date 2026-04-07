namespace UserManagementService.Application.Services;

public interface IOtpService
{
    Task<string> GenerateAndSendOtpAsync(string email, string? userId, string purpose, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);

    Task<bool> ValidateOtpAsync(string email, string otp, string purpose, CancellationToken cancellationToken = default);

    Task<bool> InvalidateOtpAsync(string email, string otp, string purpose, CancellationToken cancellationToken = default);

    Task<int> GetRemainingOtpAttemptsAsync(string email, CancellationToken cancellationToken = default);
}