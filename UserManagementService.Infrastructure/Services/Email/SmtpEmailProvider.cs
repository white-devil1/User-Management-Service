using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UserManagementService.Domain.Configuration;

namespace UserManagementService.Infrastructure.Services.Email;

public class SmtpEmailProvider : IEmailProvider
{
    private readonly SmtpSettings _smtpSettings;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SmtpEmailProvider> _logger;

    public SmtpEmailProvider(
        IOptions<EmailSettings> emailSettings,
        ILogger<SmtpEmailProvider> logger)
    {
        _emailSettings = emailSettings.Value;
        _smtpSettings = emailSettings.Value.Smtp;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 [SMTP] Sending email to {ToEmail} - Subject: {Subject}", toEmail, subject);

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(toEmail, toName));

            if (!string.IsNullOrEmpty(textBody))
            {
                message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(textBody, null, "text/plain"));
            }

            using var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
            {
                Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl,
                Timeout = 30000
            };

            _logger.LogInformation("📧 [SMTP] Connecting to {Host}:{Port}", _smtpSettings.Host, _smtpSettings.Port);

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("✅ [SMTP] Email sent successfully to {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [SMTP] Failed to send email to {ToEmail}. Error: {Error}", toEmail, ex.Message);
            return false;
        }
    }
}