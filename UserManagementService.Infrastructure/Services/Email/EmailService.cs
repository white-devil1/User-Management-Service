using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserManagementService.Application.DTOs.Email;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Configuration;
using UserManagementService.Infrastructure.Services.Email;

namespace UserManagementService.Infrastructure.Services.Email;

public class EmailService : IEmailService
{
    private readonly IEmailProvider _emailProvider;
    private readonly EmailSettings _emailSettings;
    private readonly IEmailTemplateService _templateService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IEmailProvider emailProvider,
        IOptions<EmailSettings> emailSettings,
        IEmailTemplateService templateService,
        ILogger<EmailService> logger)
    {
        _emailProvider = emailProvider;
        _emailSettings = emailSettings.Value;
        _templateService = templateService;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(EmailDto emailDto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 Sending email to {ToEmail} - Subject: {Subject}",
            emailDto.ToEmail, emailDto.Subject);

        try
        {
            var result = await _emailProvider.SendEmailAsync(
                emailDto.ToEmail,
                emailDto.ToName,
                emailDto.Subject,
                emailDto.HtmlBody,
                emailDto.TextBody,
                cancellationToken
            );

            if (result)
            {
                _logger.LogInformation("✅ Email sent successfully to {ToEmail}", emailDto.ToEmail);
            }
            else
            {
                _logger.LogError("❌ Email failed to send to {ToEmail}", emailDto.ToEmail);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Email exception for {ToEmail}: {Error}", emailDto.ToEmail, ex.Message);
            throw;
        }
    }

    public async Task<bool> SendOtpEmailAsync(
        string toEmail,
        string toName,
        string otp,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 Sending OTP email to {ToEmail} - Purpose: {Purpose}", toEmail, purpose);
        _logger.LogInformation("🔢 OTP (DEBUG): {OTP}", otp);

        var subject = purpose == "ForgotPassword"
            ? "Password Reset OTP"
            : "Verification Code";

        var htmlBody = _templateService.GenerateOtpEmailTemplate(
            toName,
            otp,
            _emailSettings.CompanyName,
            _emailSettings.CompanyUrl
        );

        return await SendEmailAsync(new EmailDto
        {
            ToEmail = toEmail,
            ToName = toName,
            Subject = subject,
            HtmlBody = htmlBody
        }, cancellationToken);
    }

    public async Task<bool> SendAdminResetPasswordEmailAsync(
        string toEmail,
        string toName,
        string username,
        string tempPassword,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 Sending admin reset password email to {ToEmail}", toEmail);

        var subject = "Your Password Has Been Reset";

        var htmlBody = _templateService.GenerateAdminResetPasswordTemplate(
            toName,
            username,
            tempPassword,
            _emailSettings.CompanyName,
            _emailSettings.CompanyUrl
        );

        return await SendEmailAsync(new EmailDto
        {
            ToEmail = toEmail,
            ToName = toName,
            Subject = subject,
            HtmlBody = htmlBody
        }, cancellationToken);
    }

    public async Task<bool> SendWelcomeEmailAsync(
        string toEmail,
        string toName,
        string username,
        string tempPassword,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 Sending welcome email to {ToEmail}", toEmail);

        var subject = "Welcome! Your Account is Ready";

        var htmlBody = _templateService.GenerateWelcomeEmailTemplate(
            toName,
            username,
            tempPassword,
            _emailSettings.CompanyName,
            _emailSettings.CompanyUrl
        );

        return await SendEmailAsync(new EmailDto
        {
            ToEmail = toEmail,
            ToName = toName,
            Subject = subject,
            HtmlBody = htmlBody
        }, cancellationToken);
    }
}