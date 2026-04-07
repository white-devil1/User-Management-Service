using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserManagementService.Application.Commands.Auth;
using UserManagementService.Application.Common;
using UserManagementService.Application.DTOs.Auth;

namespace UserManagementService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login(
        [FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(
            new LoginCommand(request.Email, request.Password));
        return Ok(ApiResponse<LoginResponse>.Ok(
            result, "Login successful"));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Register(
        [FromBody] RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand(
            request.Email, request.UserName, request.Password,
            request.TenantId, request.BranchId, request.IsSuperAdmin));
        return Ok(ApiResponse<LoginResponse>.Ok(
            result, "Registration successful"));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request)
    {
        var result = await _mediator.Send(
            new RefreshTokenCommand(request.RefreshToken));
        return Ok(ApiResponse<LoginResponse>.Ok(
            result, "Token refreshed"));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        var userId = User.FindFirst("UserId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException(
                "User not found in token");
        var result = await _mediator.Send(
            new ChangePasswordCommand(
                userId, request.CurrentPassword, request.NewPassword));
        return Ok(ApiResponse<bool>.Ok(
            result, "Password changed successfully"));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request)
    {
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers.UserAgent.ToString();
        await _mediator.Send(new ForgotPasswordCommand
        {
            Email = request.Email,
            IPAddress = clientIp,
            UserAgent = userAgent
        });
        return Ok(ApiResponse<object>.Ok(
            null, "If the email exists, an OTP has been sent."));
    }

    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> VerifyOtp(
        [FromBody] VerifyOtpRequest request)
    {
        var isValid = await _mediator.Send(new VerifyOtpCommand
        {
            Email = request.Email,
            OTP = request.OTP,
            Purpose = request.Purpose ?? "ForgotPassword"
        });
        if (!isValid)
            return BadRequest(ApiResponse<object>.Fail(
                400, "Invalid or expired OTP"));
        return Ok(ApiResponse<object>.Ok(
            null, "OTP verified successfully"));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        [FromBody] ResetPasswordRequest request)
    {
        await _mediator.Send(new ResetPasswordCommand
        {
            Email = request.Email,
            OTP = request.OTP,
            NewPassword = request.NewPassword,
            ConfirmPassword = request.ConfirmPassword
        });
        return Ok(ApiResponse<object>.Ok(
            null, "Password reset successfully"));
    }

    [HttpPost("admin-reset-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object>>> AdminResetPassword(
        [FromBody] AdminResetPasswordRequest request)
    {
        var userId = User.FindFirst("UserId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException(
                "User not found in token");
        await _mediator.Send(new AdminResetPasswordCommand
        {
            UserId = request.UserId,
            NewPassword = request.NewPassword,
            ForceChangeOnLogin = request.ForceChangeOnLogin ?? true,
            ResetByUserId = userId
        });
        return Ok(ApiResponse<object>.Ok(
            null, "Password reset. Credentials sent to user email."));
    }

    [HttpGet("test")]
    [Authorize]
    public ActionResult<ApiResponse<string>> Test()
    {
        var userId = User.FindFirst("UserId")?.Value;
        return Ok(ApiResponse<string>.Ok(
            $"Authenticated. UserId: {userId}",
            "Auth test successful"));
    }
}
