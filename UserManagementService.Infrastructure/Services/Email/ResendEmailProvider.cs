using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UserManagementService.Domain.Configuration;
using Microsoft.Extensions.Options;

namespace UserManagementService.Infrastructure.Services.Email;

public class ResendEmailProvider : IEmailProvider
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _emailSettings;
    private readonly string _baseUrl = "https://api.resend.com/emails";

    public ResendEmailProvider(HttpClient httpClient, IOptions<EmailSettings> emailSettings)
    {
        _httpClient = httpClient;
        _emailSettings = emailSettings.Value;

        // Configure HttpClient
        _httpClient.BaseAddress = new Uri("https://api.resend.com");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _emailSettings.Resend.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<bool> SendEmailAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestBody = new
            {
                from = $"{_emailSettings.FromName} <{_emailSettings.FromEmail}>",
                to = new[] { $"{toName} <{toEmail}>" },
                subject,
                html = htmlBody,
                text = textBody
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseUrl, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"✅ Email sent successfully: {responseContent}");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine($"❌ Email failed: {response.StatusCode} - {errorContent}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Email exception: {ex.Message}");
            return false;
        }
    }
}