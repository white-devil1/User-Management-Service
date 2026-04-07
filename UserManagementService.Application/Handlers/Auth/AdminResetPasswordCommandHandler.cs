using MediatR;
using Microsoft.AspNetCore.Identity;
using UserManagementService.Application.Commands.Auth;
using UserManagementService.Application.Common.Exceptions;
using UserManagementService.Application.Events;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Entities.Identity;

namespace UserManagementService.Application.Handlers.Auth;

public class AdminResetPasswordCommandHandler
    : IRequestHandler<AdminResetPasswordCommand, bool>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly ILogPublisher _logPublisher;

    public AdminResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        ILogPublisher logPublisher)
    {
        _userManager = userManager;
        _emailService = emailService;
        _logPublisher = logPublisher;
    }

    public async Task<bool> Handle(
        AdminResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null || user.IsDeleted)
            throw new NotFoundException("User", request.UserId);

        var tempPassword = request.NewPassword ?? GenerateTempPassword();

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(
            user, token, tempPassword);
        if (!result.Succeeded)
            throw new ValidationException(
                result.Errors.Select(e => e.Description).ToList());

        user.MustChangePassword = request.ForceChangeOnLogin;
        user.IsTemporaryPassword = true;
        user.TemporaryPasswordExpiresAt = DateTime.UtcNow.AddHours(24);
        user.LastPasswordChangedAt = DateTime.UtcNow;
        user.PasswordChangedCount += 1;
        user.UpdatedAt = DateTime.UtcNow;
        user.DeletedBy = request.ResetByUserId;
        await _userManager.UpdateAsync(user);

        var userName = user.FirstName ?? user.UserName;
        await _emailService.SendAdminResetPasswordEmailAsync(
            user.Email!,
            userName ?? user.UserName ?? "User",
            user.UserName!,
            tempPassword,
            cancellationToken);

        _logPublisher.PublishActivity(new ActivityLogEvent
        {
            ActionType = 74,  // AdminPasswordReset
            EntityType = 0,   // User
            EntityId = user.Id,
            Description = $"Admin reset password for user {user.Email}",
            UserId = request.ResetByUserId,
            UserEmail = user.Email,
            TenantId = user.TenantId,
            BranchId = user.BranchId
        });

        return true;
    }

    private string GenerateTempPassword()
    {
        var random = new Random();
        var uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[random.Next(26)].ToString();
        var lowercase = "abcdefghijklmnopqrstuvwxyz"[random.Next(26)].ToString();
        var digit = "0123456789"[random.Next(10)].ToString();
        var special = "@#$!%^&*"[random.Next(8)].ToString();
        const string all =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#$!%^&*";
        var remaining = new string(Enumerable.Repeat(all, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        var chars = (uppercase + lowercase + digit + special + remaining).ToCharArray();
        for (int i = chars.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
        return new string(chars);
    }
}