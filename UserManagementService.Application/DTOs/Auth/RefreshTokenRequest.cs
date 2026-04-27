using System.ComponentModel.DataAnnotations;

namespace UserManagementService.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = default!;
}
