using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementService.Application.Commands.AppPermissions;
using UserManagementService.Application.Common;
using UserManagementService.Application.DTOs.AppPermissions;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public class AppPermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AppPermissionsController(IMediator mediator) => _mediator = mediator;

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AppPermissionListResponse>>>
        GetPermissions(
        [FromQuery] Guid? appId,
        [FromQuery] Guid? pageId,
        [FromQuery] Guid? actionId,
        [FromQuery] bool? isEnabled,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "CreatedAt",
        [FromQuery] string sortOrder = "desc")
    {
        var command = new ListAppPermissionsCommand
        {
            AppId = appId,
            PageId = pageId,
            ActionId = actionId,
            IsEnabled = isEnabled,
            IncludeDeleted = includeDeleted,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<AppPermissionListResponse>.Ok(
            result, "Permissions retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AppPermissionDto>>>
        GetPermissionById(Guid id)
    {
        var result = await _mediator.Send(
            new GetAppPermissionByIdCommand { Id = id });
        return Ok(ApiResponse<AppPermissionDto>.Ok(
            result, "Permission retrieved successfully"));
    }

    [HttpPatch("{id:guid}/toggle")]
    public async Task<ActionResult<ApiResponse<AppPermissionDto>>>
        TogglePermission(
        Guid id, [FromBody] ToggleAppPermissionRequest request)
    {
        var command = new ToggleAppPermissionCommand
        {
            Id = id,
            IsEnabled = request.IsEnabled,
            UpdatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        var msg = request.IsEnabled
            ? "Permission enabled" : "Permission disabled";
        return Ok(ApiResponse<AppPermissionDto>.Ok(result, msg));
    }
}
