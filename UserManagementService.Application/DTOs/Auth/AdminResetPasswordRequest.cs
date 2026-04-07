namespace UserManagementService.Application.DTOs.Auth;

public class AdminResetPasswordRequest
{
    public string UserId { get; set; } = default!;
    public string? NewPassword { get; set; } // Optional - if null, system generates
    public bool? ForceChangeOnLogin { get; set; } = true;
}