using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementService.Application.Commands.Subscriptions;
using UserManagementService.Application.Common;
using UserManagementService.Application.DTOs.Subscriptions;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public SubscriptionsController(IMediator mediator) => _mediator = mediator;

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SubscriptionListResponse>>> GetSubscriptions(
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "Name",
        [FromQuery] string sortOrder = "asc")
    {
        var result = await _mediator.Send(new ListSubscriptionsCommand
        {
            Search = search,
            IsActive = isActive,
            IncludeDeleted = includeDeleted,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        });
        return Ok(ApiResponse<SubscriptionListResponse>.Ok(result, "Subscriptions retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> GetSubscriptionById(Guid id)
    {
        var result = await _mediator.Send(new GetSubscriptionByIdCommand { Id = id });
        return Ok(ApiResponse<SubscriptionDto>.Ok(result, "Subscription retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> CreateSubscription(
        [FromBody] CreateSubscriptionRequest request)
    {
        var result = await _mediator.Send(new CreateSubscriptionCommand
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency,
            IsActive = request.IsActive,
            AppIds = request.AppIds,
            CreatedBy = GetUserId()
        });
        return StatusCode(201, ApiResponse<SubscriptionDto>.Created(result, "Subscription created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> UpdateSubscription(
        Guid id, [FromBody] UpdateSubscriptionRequest request)
    {
        var result = await _mediator.Send(new UpdateSubscriptionCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Currency = request.Currency,
            IsActive = request.IsActive,
            AppIds = request.AppIds,
            UpdatedBy = GetUserId()
        });
        return Ok(ApiResponse<SubscriptionDto>.Ok(result, "Subscription updated successfully"));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<SubscriptionDto>>> ToggleSubscriptionStatus(
        Guid id, [FromBody] ToggleSubscriptionStatusRequest request)
    {
        var result = await _mediator.Send(new ToggleSubscriptionStatusCommand
        {
            Id = id,
            IsActive = request.IsActive,
            UpdatedBy = GetUserId()
        });
        var msg = request.IsActive ? "Subscription activated" : "Subscription deactivated";
        return Ok(ApiResponse<SubscriptionDto>.Ok(result, msg));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSubscription(Guid id)
    {
        var result = await _mediator.Send(new DeleteSubscriptionCommand
        {
            Id = id,
            DeletedBy = GetUserId()
        });
        return Ok(ApiResponse<bool>.Ok(result, "Subscription deleted successfully"));
    }
}
