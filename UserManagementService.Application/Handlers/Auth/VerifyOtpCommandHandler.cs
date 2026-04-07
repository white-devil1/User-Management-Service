using MediatR;
using UserManagementService.Application.Commands.Auth;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;

namespace UserManagementService.Application.Handlers.Auth;

public class VerifyOtpCommandHandler
    : IRequestHandler<VerifyOtpCommand, bool>
{
    private readonly IOtpService _otpService;
    private readonly ILogPublisher _logPublisher;

    public VerifyOtpCommandHandler(
        IOtpService otpService,
        ILogPublisher logPublisher)
    {
        _otpService = otpService;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(
        VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var isValid = await _otpService.ValidateOtpAsync(
            request.Email, request.OTP, request.Purpose, cancellationToken);

        if (isValid)
        {
            _logPublisher.PublishActivity(new ActivityLogEvent
            {
                ActionType = 71,  // OtpVerified
                EntityType = 0,   // User
                Description = $"OTP verified for {request.Email}",
                UserEmail = request.Email
            });
        }

        return isValid;
    }
}