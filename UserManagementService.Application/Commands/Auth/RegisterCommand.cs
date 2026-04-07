using MediatR;
using UserManagementService.Application.DTOs.Auth;

namespace UserManagementService.Application.Commands.Auth;

public record RegisterCommand(
    string Email,
    string UserName,
    string Password,
    Guid TenantId,
    Guid? BranchId,
    bool IsSuperAdmin = false
) : IRequest<LoginResponse>;