using UserManagementService.Application.DTOs.Email;

namespace UserManagementService.Application.Services;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailDto emailDto, CancellationToken cancellationToken = default);

    Task<bool> SendOtpEmailAsync(string toEmail, string toName, string otp, string purpose, CancellationToken cancellationToken = default);

    Task<bool> SendAdminResetPasswordEmailAsync(string toEmail, string toName, string username, string tempPassword, CancellationToken cancellationToken = default);
}