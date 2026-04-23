using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using UserManagementService.Domain.Configuration;

namespace UserManagementService.Infrastructure.Services.Email;

public class SmtpEmailProvider : IEmailProvider
{
    private readonly SmtpSettings _smtpSettings;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<SmtpEmailProvider> _logger;
    private readonly ResiliencePipeline _pipeline;

    public SmtpEmailProvider(
        IOptions<EmailSettings> emailSettings,
        ILogger<SmtpEmailProvider> logger,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _emailSettings = emailSettings.Value;
        _smtpSettings = emailSettings.Value.Smtp;
        _logger = logger;
        _pipeline = pipelineProvider.GetPipeline("smtp");
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[SMTP] Sending email to {ToEmail} - Subject: {Subject}", toEmail, subject);

        try
        {
            return await _pipeline.ExecuteAsync<bool>(async ct =>
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                if (!string.IsNullOrEmpty(textBody))
                    bodyBuilder.TextBody = textBody;
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(
                    _smtpSettings.Host, _smtpSettings.Port,
                    SecureSocketOptions.StartTls, ct);
                await client.AuthenticateAsync(
                    _smtpSettings.Username, _smtpSettings.Password, ct);
                await client.SendAsync(message, ct);
                await client.DisconnectAsync(quit: true, ct);

                _logger.LogInformation(
                    "[SMTP] Email sent successfully to {ToEmail}", toEmail);
                return true;

            }, cancellationToken);
        }
        catch (BrokenCircuitException)
        {
            _logger.LogWarning(
                "[SMTP] Circuit is open — skipping email to {ToEmail}", toEmail);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[SMTP] Failed to send email to {ToEmail} after retries", toEmail);
            return false;
        }
    }
}
