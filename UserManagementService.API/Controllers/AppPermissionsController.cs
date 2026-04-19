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

    // ✅ Endpoint 1: Get All Permissions (Grouped by App → Page → Permission)
    [HttpGet]
    public async Task<ActionResult<ApiResponse<GroupedPermissionResponse>>>
        GetPermissions(
        [FromQuery] Guid? appId,
        [FromQuery] Guid? pageId,
        [FromQuery] bool? isEnabled)
    {
        var command = new GetGroupedPermissionsCommand
        {
            AppId = appId,
            PageId = pageId,
            IsEnabled = isEnabled
        };
        var result = await _mediator.Send(command);

        if (result.Apps.Count == 0)
        {
            return Ok(ApiResponse<GroupedPermissionResponse>.Ok(
                result, "No permissions found"));
        }

        return Ok(ApiResponse<GroupedPermissionResponse>.Ok(
            result, "Permissions fetched successfully"));
    }

    // ✅ Get Permission by ID (kept for backward compat)
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AppPermissionDto>>>
        GetPermissionById(Guid id)
    {
        var result = await _mediator.Send(
            new GetAppPermissionByIdCommand { Id = id });
        return Ok(ApiResponse<AppPermissionDto>.Ok(
            result, "Permission retrieved successfully"));
    }

    // ✅ Endpoint 2: Toggle Single Permission Status
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<TogglePermissionResponseDto>>>
        TogglePermissionStatus(
        Guid id, [FromBody] ToggleAppPermissionRequest request)
    {
        var command = new TogglePermissionStatusCommand
        {
            Id = id,
            IsEnabled = request.IsEnabled,
            UpdatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<TogglePermissionResponseDto>.Ok(
            result, "Permission updated successfully"));
    }

    // ✅ Endpoint 3: Bulk Toggle Permission Status
    [HttpPatch("bulk-status")]
    public async Task<ActionResult<ApiResponse<BulkTogglePermissionResponse>>>
        BulkTogglePermissionStatus(
        [FromBody] BulkTogglePermissionRequest request)
    {
        var command = new BulkTogglePermissionStatusCommand
        {
            PermissionStatuses = request.PermissionStatuses,
            UpdatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<BulkTogglePermissionResponse>.Ok(
            result, $"{result.UpdatedCount} permissions updated successfully"));
    }
}
