using MediatR;
using UserManagementService.Application.DTOs.Auth;

namespace UserManagementService.Application.Commands.Auth;

public record LoginCommand(string Email, string Password, string? IpAddress = null, string? UserAgent = null) : IRequest<LoginResponse>;