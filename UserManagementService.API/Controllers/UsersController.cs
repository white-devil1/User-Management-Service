using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagementService.Application.Commands.Users;
using UserManagementService.Application.Common;
using UserManagementService.Application.DTOs.Users;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    public UsersController(IMediator mediator) => _mediator = mediator;

    private string GetUserId()
        => User.FindFirst("UserId")?.Value ?? string.Empty;
    private bool IsSuperAdmin()
        => User.FindFirst("IsSuperAdmin")?.Value == "True";
    private string? GetTenantIdStr()
        => User.FindFirst("TenantId")?.Value;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserListResponse>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? status,  // Changed from List<string> to string
        [FromQuery] Guid? tenantId,
        [FromQuery] Guid? branchId,
        [FromQuery] string? roleId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortOrder = "desc")
    {
        // Parse status: supports both comma-separated and multiple query params
        var statusList = new List<string>();
        if (!string.IsNullOrEmpty(status))
        {
            statusList = status.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim().ToLower())
                .ToList();
        }
        
        if (!IsSuperAdmin() && !string.IsNullOrEmpty(GetTenantIdStr()))
            tenantId = Guid.Parse(GetTenantIdStr()!);
        if (!IsSuperAdmin() && statusList.Any() && statusList.Contains("deleted"))
        {
            statusList = statusList.Where(s => s != "deleted").ToList();
            if (!statusList.Any()) statusList = new List<string> { "active" };
        }
        var command = new ListUsersCommand
        {
            Search = search,
            Status = statusList.Any() ? statusList : new List<string> { "active" },
            TenantId = tenantId,
            BranchId = branchId,
            RoleId = roleId,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<UserListResponse>.Ok(
            result, "Users retrieved successfully"));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUser(string id)
    {
        var command = new GetUserByIdCommand
        {
            Id = id,
            IsSuperAdmin = IsSuperAdmin(),
            CallerTenantId = GetTenantIdStr()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<UserResponse>.Ok(
            result, "User retrieved successfully"));
    }

    [HttpPost]
    [Authorize(Policy = "SuperAdminOrTenantAdmin")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser(
        [FromBody] CreateUserRequest request)
    {
        if (!IsSuperAdmin() && !string.IsNullOrEmpty(GetTenantIdStr()))
            request.TenantId = Guid.Parse(GetTenantIdStr()!);
        var command = new CreateUserCommand
        {
            Email = request.Email,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            TenantId = request.TenantId,
            BranchId = request.BranchId,
            IsActive = request.IsActive,
            RoleNames = request.RoleNames,
            CreatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return StatusCode(201, ApiResponse<UserResponse>.Created(
            result, "User created successfully"));
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "SuperAdminOrTenantAdmin")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(
        string id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand
        {
            Id = id,
            Email = request.Email,
            UserName = request.UserName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = request.IsActive,
            BranchId = request.BranchId,
            RoleNames = request.RoleNames,
            UpdatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<UserResponse>.Ok(
            result, "User updated successfully"));
    }

    [HttpPatch("{id}/status")]
    [Authorize(Policy = "SuperAdminOrTenantAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleUserStatus(
        string id, [FromBody] ToggleUserStatusRequest request)
    {
        var command = new ToggleUserStatusCommand
        { Id = id, IsActive = request.IsActive, UpdatedBy = GetUserId() };
        var result = await _mediator.Send(command);
        var msg = request.IsActive ? "User activated" : "User deactivated";
        return Ok(ApiResponse<bool>.Ok(result, msg));
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "SuperAdminOrTenantAdmin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUser(string id)
    {
        var command = new DeleteUserCommand
        { Id = id, DeletedBy = GetUserId() };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.Ok(
            result, "User deleted successfully"));
    }

    [HttpPut("{id}/restore")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<ActionResult<ApiResponse<bool>>> RestoreUser(string id)
    {
        var result = await _mediator.Send(
            new RestoreUserCommand { Id = id });
        return Ok(ApiResponse<bool>.Ok(
            result, "User restored successfully"));
    }

    [HttpGet("available-roles")]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAvailableRoles()
    {
        var command = new GetAvailableRolesCommand
        {
            IsSuperAdmin = IsSuperAdmin(),
            CallerTenantId = GetTenantIdStr()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<List<RoleDto>>.Ok(
            result, "Available roles retrieved"));
    }
}
