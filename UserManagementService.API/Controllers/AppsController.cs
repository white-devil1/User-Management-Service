using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementService.Application.Commands.Apps;
using UserManagementService.Application.Common;
using UserManagementService.Application.DTOs.Apps;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public class AppsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AppsController(IMediator mediator) => _mediator = mediator;

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AppListResponse>>> GetApps(
        [FromQuery] string? search,
        [FromQuery] string? code,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "DisplayOrder",
        [FromQuery] string sortOrder = "asc")
    {
        var command = new ListAppsCommand
        {
            Search = search,
            Code = code,
            IsActive = isActive,
            IncludeDeleted = includeDeleted,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AppListResponse>.Ok(
            result, "Apps retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AppDto>>> GetAppById(Guid id)
    {
        var result = await _mediator.Send(
            new GetAppByIdCommand { Id = id });
        return Ok(ApiResponse<AppDto>.Ok(
            result, "App retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AppDto>>> CreateApp(
        [FromBody] CreateAppRequest request)
    {
        var command = new CreateAppCommand
        {
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            DisplayOrder = request.DisplayOrder,
            CreatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return StatusCode(201, ApiResponse<AppDto>.Created(
            result, "App created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AppDto>>> UpdateApp(
        Guid id, [FromBody] UpdateAppRequest request)
    {
        var command = new UpdateAppCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            UpdatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AppDto>.Ok(
            result, "App updated successfully"));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<AppDto>>> ToggleAppStatus(
        Guid id, [FromBody] ToggleAppStatusRequest request)
    {
        var command = new ToggleAppStatusCommand
        { Id = id, IsActive = request.IsActive, UpdatedBy = GetUserId() };
        var result = await _mediator.Send(command);
        var msg = request.IsActive ? "App activated" : "App deactivated";
        return Ok(ApiResponse<AppDto>.Ok(result, msg));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteApp(Guid id)
    {
        var command = new DeleteAppCommand
        { Id = id, DeletedBy = GetUserId() };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.Ok(
            result, "App deleted successfully"));
    }
}
