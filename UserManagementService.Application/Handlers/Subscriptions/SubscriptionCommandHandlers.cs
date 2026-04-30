using MediatR;
using UserManagementService.Application.Commands.Subscriptions;
using UserManagementService.Application.DTOs.Subscriptions;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Subscriptions;

public class CreateSubscriptionCommandHandler
    : IRequestHandler<CreateSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionService _service;
    public CreateSubscriptionCommandHandler(ISubscriptionService service) => _service = service;

    public Task<SubscriptionDto> Handle(CreateSubscriptionCommand request, CancellationToken cancellationToken)
        => _service.CreateSubscriptionAsync(new CreateSubscriptionRequest
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency,
            IsActive = request.IsActive,
            AppIds = request.AppIds
        }, request.CreatedBy, cancellationToken);
}

public class UpdateSubscriptionCommandHandler
    : IRequestHandler<UpdateSubscriptionCommand, SubscriptionDto>
{
    private readonly ISubscriptionService _service;
    public UpdateSubscriptionCommandHandler(ISubscriptionService service) => _service = service;

    public Task<SubscriptionDto> Handle(UpdateSubscriptionCommand request, CancellationToken cancellationToken)
        => _service.UpdateSubscriptionAsync(request.Id, new UpdateSubscriptionRequest
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency,
            IsActive = request.IsActive,
            AppIds = request.AppIds
        }, request.UpdatedBy, cancellationToken);
}

public class DeleteSubscriptionCommandHandler
    : IRequestHandler<DeleteSubscriptionCommand, bool>
{
    private readonly ISubscriptionService _service;
    public DeleteSubscriptionCommandHandler(ISubscriptionService service) => _service = service;

    public Task<bool> Handle(DeleteSubscriptionCommand request, CancellationToken cancellationToken)
        => _service.DeleteSubscriptionAsync(request.Id, request.DeletedBy, cancellationToken);
}

public class ToggleSubscriptionStatusCommandHandler
    : IRequestHandler<ToggleSubscriptionStatusCommand, SubscriptionDto>
{
    private readonly ISubscriptionService _service;
    public ToggleSubscriptionStatusCommandHandler(ISubscriptionService service) => _service = service;

    public Task<SubscriptionDto> Handle(ToggleSubscriptionStatusCommand request, CancellationToken cancellationToken)
        => _service.ToggleSubscriptionStatusAsync(request.Id, request.IsActive, request.UpdatedBy, cancellationToken);
}

public class GetSubscriptionByIdCommandHandler
    : IRequestHandler<GetSubscriptionByIdCommand, SubscriptionDto>
{
    private readonly ISubscriptionService _service;
    public GetSubscriptionByIdCommandHandler(ISubscriptionService service) => _service = service;

    public Task<SubscriptionDto> Handle(GetSubscriptionByIdCommand request, CancellationToken cancellationToken)
        => _service.GetSubscriptionByIdAsync(request.Id, cancellationToken);
}

public class ListSubscriptionsCommandHandler
    : IRequestHandler<ListSubscriptionsCommand, SubscriptionListResponse>
{
    private readonly ISubscriptionService _service;
    public ListSubscriptionsCommandHandler(ISubscriptionService service) => _service = service;

    public Task<SubscriptionListResponse> Handle(ListSubscriptionsCommand request, CancellationToken cancellationToken)
        => _service.GetSubscriptionsAsync(
            request.Search, request.IsActive, request.IncludeDeleted,
            request.Page, request.PageSize, request.SortBy, request.SortOrder, cancellationToken);
}
