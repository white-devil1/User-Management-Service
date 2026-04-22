using MediatR;
using UserManagementService.Application.DTOs.Users;

namespace UserManagementService.Application.Commands.Users;

public class UpdateUserProfileCommand : IRequest<UserResponse>
{
    public string UserId { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public byte[]? ProfileImageBytes { get; set; }
    public string? ProfileImageExtension { get; set; }
    public byte[]? ProfileThumbBytes { get; set; }
    public string? ProfileThumbExtension { get; set; }
}
