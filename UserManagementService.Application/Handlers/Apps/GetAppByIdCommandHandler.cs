using MediatR;
using UserManagementService.Application.Commands.Apps;
using UserManagementService.Application.DTOs.Apps;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Apps;

public class GetAppByIdCommandHandler : IRequestHandler<GetAppByIdCommand, AppDto>
{
    private readonly IAppService _appService;

    public GetAppByIdCommandHandler(IAppService appService)
    {
        _appService = appService;
    }

    public async Task<AppDto> Handle(GetAppByIdCommand request, CancellationToken cancellationToken)
    {
        return await _appService.GetAppByIdAsync(request.Id, cancellationToken);
    }
}