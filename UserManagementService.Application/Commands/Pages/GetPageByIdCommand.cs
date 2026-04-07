using MediatR;
using UserManagementService.Application.DTOs.Pages;

namespace UserManagementService.Application.Commands.Pages;

public class GetPageByIdCommand : IRequest<PageDto>
{
    public Guid Id { get; set; }
}