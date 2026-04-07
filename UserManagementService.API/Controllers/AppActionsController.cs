using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementService.Application.Commands.AppActions;
using UserManagementService.Application.Common;
using UserManagementService.Application.DTOs.AppActions;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public class AppActionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AppActionsController(IMediator mediator) => _mediator = mediator;

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AppActionListResponse>>> GetActions(
        [FromQuery] Guid? appId,
        [FromQuery] Guid? pageId,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "DisplayOrder",
        [FromQuery] string sortOrder = "asc")
    {
        var command = new ListAppActionsCommand
        {
            AppId = appId,
            PageId = pageId,
            Search = search,
            IsActive = isActive,
            IncludeDeleted = includeDeleted,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AppActionListResponse>.Ok(
            result, "Actions retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AppActionDto>>> GetActionById(
        Guid id)
    {
        var result = await _mediator.Send(
            new GetAppActionByIdCommand { Id = id });
        return Ok(ApiResponse<AppActionDto>.Ok(
            result, "Action retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AppActionDto>>> CreateAction(
        [FromBody] CreateAppActionRequest request)
    {
        var command = new CreateAppActionCommand
        {
            PageId = request.PageId,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            DisplayOrder = request.DisplayOrder,
            CreatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return StatusCode(201, ApiResponse<AppActionDto>.Created(
            result, "Action created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AppActionDto>>> UpdateAction(
        Guid id, [FromBody] UpdateAppActionRequest request)
    {
        var command = new UpdateAppActionCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            UpdatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AppActionDto>.Ok(
            result, "Action updated successfully"));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<AppActionDto>>> ToggleActionStatus(
        Guid id, [FromBody] ToggleAppActionStatusRequest request)
    {
        var command = new ToggleAppActionStatusCommand
        { Id = id, IsActive = request.IsActive, UpdatedBy = GetUserId() };
        var result = await _mediator.Send(command);
        var msg = request.IsActive ? "Action activated" : "Action deactivated";
        return Ok(ApiResponse<AppActionDto>.Ok(result, msg));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAction(Guid id)
    {
        var command = new DeleteAppActionCommand
        { Id = id, DeletedBy = GetUserId() };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.Ok(
            result, "Action deleted successfully"));
    }
}
