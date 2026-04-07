using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using UserManagementService.Application.Commands.Auth;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Auth;

public class ForgotPasswordCommandHandler
    : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IOtpService _otpService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;
    private readonly ILogPublisher _logPublisher;

    public ForgotPasswordCommandHandler(
        IOtpService otpService,
        UserManager<ApplicationUser> userManager,
        ILogger<ForgotPasswordCommandHandler> logger,
        ILogPublisher logPublisher)
    {
        _otpService = otpService;
        _userManager = userManager;
        _logger = logger;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(
        ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Forgot password request for email: {Email}", request.Email);

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            throw new NotFoundException("User", request.Email);

        if (user.IsDeleted || !user.IsActive)
            throw new BadRequestException("This account is inactive or has been deleted.");

        try
        {
            var otp = await _otpService.GenerateAndSendOtpAsync(
                request.Email, user.Id, "ForgotPassword",
                request.IPAddress, request.UserAgent, cancellationToken);

            _logger.LogInformation(
                "OTP generated and sent to {Email}. OTP (DEBUG): {OTP}",
                request.Email, otp);

            // ActivityLog — OTP sent successfully (pre-auth, user fetched from DB)
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 70,  // ForgotPasswordRequested
                EntityType = 0,   // User
                EntityId = user.Id,
                Description = $"Password reset OTP sent to {user.Email}",
                UserId = user.Id,
                UserEmail = user.Email,
                TenantId = user.TenantId,
                BranchId = user.BranchId
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send OTP to {Email}", request.Email);
            throw;
        }
    }
}