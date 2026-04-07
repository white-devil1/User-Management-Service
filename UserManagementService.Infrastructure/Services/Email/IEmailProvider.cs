namespace UserManagementService.Infrastructure.Services.Email;

public interface IEmailProvider
{
    Task<bool> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default);
}