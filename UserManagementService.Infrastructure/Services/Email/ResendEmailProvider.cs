using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Polly;
using Polly.CircuitBreaker;
using UserManagementService.Domain.Configuration;

namespace UserManagementService.Infrastructure.Services.Email;

public class ResendEmailProvider : IEmailProvider
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _emailSettings;
    private readonly ResiliencePipeline _pipeline;
    private readonly string _baseUrl = "https://api.resend.com/emails";

    public ResendEmailProvider(
        HttpClient httpClient,
        IOptions<EmailSettings> emailSettings,
        ResiliencePipelineProvider<string> pipelineProvider)
    {
        _httpClient = httpClient;
        _emailSettings = emailSettings.Value;
        _pipeline = pipelineProvider.GetPipeline("resend");

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
            return await _pipeline.ExecuteAsync<bool>(async ct =>
            {
                var requestBody = new
                {
                    from = $"{_emailSettings.FromName} <{_emailSettings.FromEmail}>",
                    to = new[] { $"{toName} <{toEmail}>" },
                    subject,
                    html = htmlBody,
                    text = textBody
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl, content, ct);

                if (response.IsSuccessStatusCode)
                    return true;

                // Treat non-success HTTP as a failure so Polly can retry
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException(
                    $"Resend API returned {(int)response.StatusCode}: {error}");

            }, cancellationToken);
        }
        catch (BrokenCircuitException)
        {
            Console.WriteLine("⚡ Resend circuit is open — email skipped");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Resend failed after retries: {ex.Message}");
            return false;
        }
    }
}
