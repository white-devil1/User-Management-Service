using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Commands.Auth;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Auth;

public class ResetPasswordCommandHandler
    : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IOtpService _otpService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogPublisher _logPublisher;

    public ResetPasswordCommandHandler(
        IOtpService otpService,
        UserManager<ApplicationUser> userManager,
        ILogPublisher logPublisher)
    {
        _otpService = otpService;
        _userManager = userManager;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(
        ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.NewPassword != request.ConfirmPassword)
            throw new ValidationException("Passwords do not match.");

        var isValidOtp = await _otpService.ValidateOtpAsync(
            request.Email, request.OTP, "ForgotPassword", cancellationToken);
        if (!isValidOtp)
            throw new ValidationException("Invalid or expired OTP.");

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive || user.IsDeleted)
            throw new NotFoundException("User not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(
            user, token, request.NewPassword);
        if (!result.Succeeded)
            throw new ValidationException(
                result.Errors.Select(e => e.Description).ToList());

        user.MustChangePassword = true;
        user.IsTemporaryPassword = true;
        user.TemporaryPasswordExpiresAt = DateTime.UtcNow.AddHours(24);
        user.LastPasswordChangedAt = DateTime.UtcNow;
        user.PasswordChangedCount += 1;
        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        await _otpService.InvalidateOtpAsync(
            request.Email, request.OTP, "ForgotPassword", cancellationToken);

        // Pre-auth — user fetched from DB so full context is available
        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 72,  // PasswordReset
            EntityType = 0,   // User
            EntityId = user.Id,
            Description = $"Password reset via OTP for {user.Email}",
            UserId = user.Id,
            UserEmail = user.Email,
            TenantId = user.TenantId,
            BranchId = user.BranchId
        });

        return true;
    }
}