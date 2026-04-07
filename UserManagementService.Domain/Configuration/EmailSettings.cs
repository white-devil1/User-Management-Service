namespace UserManagementService.Domain.Configuration;

public class EmailSettings
{
    public string Provider { get; set; } = "Resend"; // Resend or Smtp
    public ResendSettings Resend { get; set; } = new();
    public SmtpSettings Smtp { get; set; } = new();
    public string FromEmail { get; set; } = default!;
    public string FromName { get; set; } = default!;
    public string CompanyName { get; set; } = default!;
    public string CompanyUrl { get; set; } = default!;
    public string SupportEmail { get; set; } = default!;
    public OtpSettings OtpSettings { get; set; } = new();
}

public class ResendSettings
{
    public string ApiKey { get; set; } = default!;
}

public class SmtpSettings
{
    public string Host { get; set; } = default!;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool EnableSsl { get; set; } = true;
}

public class OtpSettings
{
    public int CodeLength { get; set; } = 6;
    public int ExpirationMinutes { get; set; } = 10;
    public int MaxAttemptsPerHour { get; set; } = 10;
}