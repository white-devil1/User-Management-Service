using UserManagementService.Application.DTOs.Email;

namespace UserManagementService.Application.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailDto emailDto, CancellationToken cancellationToken = default);

    Task<bool> SendOtpEmailAsync(string toEmail, string displayName, string otp, string purpose, CancellationToken cancellationToken = default);

    Task<bool> SendAdminResetPasswordEmailAsync(string toEmail, string displayName, string loginEmail, string tempPassword, CancellationToken cancellationToken = default);

    Task<bool> SendWelcomeEmailAsync(string toEmail, string displayName, string loginEmail, string tempPassword, CancellationToken cancellationToken = default);
}
