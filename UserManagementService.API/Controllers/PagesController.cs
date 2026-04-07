using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementService.Application.Commands.Pages;
using UserManagementService.Application.Common;
using UserManagementService.Application.DTOs.Pages;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly")]
public class PagesController : ControllerBase
{
    private readonly IMediator _mediator;
    public PagesController(IMediator mediator) => _mediator = mediator;

    private string GetUserId()
        => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PageListResponse>>> GetPages(
        [FromQuery] Guid? appId,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "DisplayOrder",
        [FromQuery] string sortOrder = "asc")
    {
        var command = new ListPagesCommand
        {
            AppId = appId,
            Search = search,
            IsActive = isActive,
            IncludeDeleted = includeDeleted,
            Page = page,
            PageSize = pageSize,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<PageListResponse>.Ok(
            result, "Pages retrieved successfully"));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PageDto>>> GetPageById(Guid id)
    {
        var result = await _mediator.Send(
            new GetPageByIdCommand { Id = id });
        return Ok(ApiResponse<PageDto>.Ok(
            result, "Page retrieved successfully"));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PageDto>>> CreatePage(
        [FromBody] CreatePageRequest request)
    {
        var command = new CreatePageCommand
        {
            AppId = request.AppId,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            CreatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return StatusCode(201, ApiResponse<PageDto>.Created(
            result, "Page created successfully"));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PageDto>>> UpdatePage(
        Guid id, [FromBody] UpdatePageRequest request)
    {
        var command = new UpdatePageCommand
        {
            Id = id,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            UpdatedBy = GetUserId()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<PageDto>.Ok(
            result, "Page updated successfully"));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<PageDto>>> TogglePageStatus(
        Guid id, [FromBody] TogglePageStatusRequest request)
    {
        var command = new TogglePageStatusCommand
        { Id = id, IsActive = request.IsActive, UpdatedBy = GetUserId() };
        var result = await _mediator.Send(command);
        var msg = request.IsActive ? "Page activated" : "Page deactivated";
        return Ok(ApiResponse<PageDto>.Ok(result, msg));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePage(Guid id)
    {
        var command = new DeletePageCommand
        { Id = id, DeletedBy = GetUserId() };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.Ok(
            result, "Page deleted successfully"));
    }
}
