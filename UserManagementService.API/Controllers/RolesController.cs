using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.Application.Commands.Roles;
using UserManagementService.Application.Common;
using UserManagementService.Application.DTOs.Roles;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;
    public RolesController(IMediator mediator) => _mediator = mediator;

    private string GetUserId()
        => User.FindFirst("UserId")?.Value ?? string.Empty;
    private bool IsSuperAdmin()
        => User.FindFirst("IsSuperAdmin")?.Value == "True";
    private Guid GetTenantId()
        => Guid.TryParse(User.FindFirst("TenantId")?.Value, out var g)
            ? g : Guid.Empty;
    private Guid? GetBranchId()
        => Guid.TryParse(User.FindFirst("BranchId")?.Value, out var g)
            ? g : null;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<RoleListResponse>>> GetRoles(
        [FromQuery] string? search,
        [FromQuery] bool? isDeleted,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "name",
        [FromQuery] string sortOrder = "asc")
    {
        var command = new ListRolesCommand
        {
            Search = search,
            IsDeleted = isDeleted,
            CallerIsSuperAdmin = IsSuperAdmin(),
            CallerTenantId = GetTenantId(),
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<RoleListResponse>.Ok(
            result, "Roles retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> GetRole(string id)
    {
        var command = new GetRoleByIdCommand
        {
            Id = id,
            CallerIsSuperAdmin = IsSuperAdmin(),
            CallerTenantId = GetTenantId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<RoleResponse>.Ok(
            result, "Role retrieved successfully"));
    }

    [HttpPost]
    [Authorize(Policy = "SuperAdminOrTenantAdmin")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> CreateRole(
        [FromBody] CreateRoleRequest request)
    {
        var command = new CreateRoleCommand
        {
            Name = request.Name,
            Description = request.Description,
            CallerUserId = GetUserId(),
            CallerIsSuperAdmin = IsSuperAdmin(),
            CallerTenantId = GetTenantId(),
            CallerBranchId = GetBranchId()
        };
        var result = await _mediator.Send(command);
        return StatusCode(201, ApiResponse<RoleResponse>.Created(
            result, "Role created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "SuperAdminOrTenantAdmin")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> UpdateRole(
        string id, [FromBody] UpdateRoleRequest request)
    {
        var command = new UpdateRoleCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            CallerUserId = GetUserId(),
            CallerIsSuperAdmin = IsSuperAdmin(),
            CallerTenantId = GetTenantId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<RoleResponse>.Ok(
            result, "Role updated successfully"));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SuperAdminOrTenantAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteRole(string id)
    {
        var command = new DeleteRoleCommand
        {
            Id = id,
            CallerUserId = GetUserId(),
            CallerIsSuperAdmin = IsSuperAdmin(),
            CallerTenantId = GetTenantId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.Ok(
            result, "Role deleted successfully"));
    }

    [HttpGet("available-permissions")]
    public async Task<ActionResult<ApiResponse<RolePermissionsGrouped>>> GetAvailablePermissions()
    {
        var command = new GetAvailablePermissionsCommand
        {
            CallerUserId = GetUserId(),
            CallerIsSuperAdmin = IsSuperAdmin()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<RolePermissionsGrouped>.Ok(
            result, "Available permissions retrieved successfully"));
    }

    [HttpPost("{id}/permissions")]
    [Authorize(Policy = "SuperAdminOrTenantAdmin")]
    public async Task<ActionResult<ApiResponse<RoleResponse>>> AssignPermissions(
        string id, [FromBody] AssignPermissionsRequest request)
    {
        var command = new AssignPermissionsCommand
        {
            RoleId = id,
            PermissionIds = request.PermissionIds,
            CallerUserId = GetUserId(),
            CallerIsSuperAdmin = IsSuperAdmin(),
            CallerTenantId = GetTenantId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<RoleResponse>.Ok(
            result, "Permissions assigned successfully"));
    }
}
