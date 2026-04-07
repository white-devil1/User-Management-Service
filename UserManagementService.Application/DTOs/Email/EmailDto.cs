namespace UserManagementService.Application.DTOs.Email;

public class EmailDto
{
    public string ToEmail { get; set; } = default!;
    public string ToName { get; set; } = default!;
    public string Subject { get; set; } = default!;
    public string HtmlBody { get; set; } = default!;
    public string? TextBody { get; set; }
    public string? FromEmail { get; set; }
    public string? FromName { get; set; }
}