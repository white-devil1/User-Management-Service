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
public class UserProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    public UserProfileController(IMediator mediator) => _mediator = mediator;

    private string GetUserId()
        => User.FindFirst("UserId")?.Value ?? string.Empty;
    private bool IsSuperAdmin()
        => User.FindFirst("IsSuperAdmin")?.Value == "True";
    private string? GetTenantIdStr()
        => User.FindFirst("TenantId")?.Value;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetProfile()
    {
        var command = new GetUserByIdCommand
        {
            Id = GetUserId(),
            IsSuperAdmin = IsSuperAdmin(),
            CallerTenantId = GetTenantIdStr()
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<UserResponse>.Ok(result, "Profile retrieved successfully"));
    }

    [HttpPatch]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateProfile(
        [FromForm] UpdateUserProfileRequest request,
        IFormFile? profileImage,
        IFormFile? profileThumbImage)
    {
        byte[]? imageBytes = null;
        string? imageExt = null;
        byte[]? thumbBytes = null;
        string? thumbExt = null;

        if (profileImage != null && profileImage.Length > 0)
        {
            using var ms = new MemoryStream();
            await profileImage.CopyToAsync(ms);
            imageBytes = ms.ToArray();
            imageExt = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
        }
        if (profileThumbImage != null && profileThumbImage.Length > 0)
        {
            using var ms = new MemoryStream();
            await profileThumbImage.CopyToAsync(ms);
            thumbBytes = ms.ToArray();
            thumbExt = Path.GetExtension(profileThumbImage.FileName).ToLowerInvariant();
        }

        var command = new UpdateUserProfileCommand
        {
            UserId = GetUserId(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            ProfileImageBytes = imageBytes,
            ProfileImageExtension = imageExt,
            ProfileThumbBytes = thumbBytes,
            ProfileThumbExtension = thumbExt
        };
        var result = await _mediator.Send(command);
        return Ok(ApiResponse<UserResponse>.Ok(result, "Profile updated successfully"));
    }
}
