using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserManagementService.Application.Services;
using UserManagementService.Domain.Configuration;
using UserManagementService.Domain.Entities.Auth;
using UserManagementService.Infrastructure.Persistence;

namespace UserManagementService.Infrastructure.Services.Email;

public class OtpService : IOtpService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly EmailSettings _emailSettings;

    public OtpService(
        ApplicationDbContext context,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings)
    {
        _context = context;
        _emailService = emailService;
        _emailSettings = emailSettings.Value;
    }

    public async Task<string> GenerateAndSendOtpAsync(
        string email,
        string? userId,
        string purpose,
        string? ipAddress,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        // ✅ Check rate limiting
        var attempts = await GetOtpAttemptsLastHourAsync(email, cancellationToken);
        if (attempts >= _emailSettings.OtpSettings.MaxAttemptsPerHour)
        {
            throw new InvalidOperationException($"Maximum OTP attempts ({_emailSettings.OtpSettings.MaxAttemptsPerHour}) exceeded. Please try again later.");
        }

        // ✅ Generate 6-digit OTP
        var otp = GenerateOtpCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(_emailSettings.OtpSettings.ExpirationMinutes);

        // ✅ Invalidate any existing unused OTPs for this email/purpose
        await InvalidateExistingOtpsAsync(email, purpose, cancellationToken);

        // ✅ Save OTP to database
        var otpVerification = new OtpVerification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            OTP = otp,
            Purpose = purpose,
            ExpiresAt = expiresAt,
            IPAddress = ipAddress,
            UserAgent = userAgent,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.OtpVerifications.Add(otpVerification);
        await _context.SaveChangesAsync(cancellationToken);

        // ✅ Send OTP email
        var userName = await GetUserNameByEmailAsync(email, cancellationToken);
        await _emailService.SendOtpEmailAsync(email, userName ?? "User", otp, purpose, cancellationToken);

        return otp;
    }

    public async Task<bool> ValidateOtpAsync(
        string email,
        string otp,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        var otpRecord = await _context.OtpVerifications
            .FirstOrDefaultAsync(o =>
                o.Email == email &&
                o.OTP == otp &&
                o.Purpose == purpose &&
                !o.IsUsed &&
                o.ExpiresAt > DateTime.UtcNow,
            cancellationToken);

        return otpRecord != null;
    }

    public async Task<bool> InvalidateOtpAsync(
        string email,
        string otp,
        string purpose,
        CancellationToken cancellationToken = default)
    {
        var otpRecord = await _context.OtpVerifications
            .FirstOrDefaultAsync(o =>
                o.Email == email &&
                o.OTP == otp &&
                o.Purpose == purpose &&
                !o.IsUsed,
            cancellationToken);

        if (otpRecord == null)
        {
            return false;
        }

        // ✅ Check if expired
        if (otpRecord.ExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        // ✅ Mark as used
        otpRecord.IsUsed = true;
        otpRecord.UsedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> GetRemainingOtpAttemptsAsync(string email, CancellationToken cancellationToken = default)
    {
        var attempts = await GetOtpAttemptsLastHourAsync(email, cancellationToken);
        var maxAttempts = _emailSettings.OtpSettings.MaxAttemptsPerHour;
        return Math.Max(0, maxAttempts - attempts);
    }

    // ✅ Helper: Generate 6-digit OTP
    private string GenerateOtpCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    // ✅ Helper: Get OTP attempts in last hour
    private async Task<int> GetOtpAttemptsLastHourAsync(string email, CancellationToken cancellationToken)
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        return await _context.OtpVerifications
            .CountAsync(o => o.Email == email && o.CreatedAt >= oneHourAgo, cancellationToken);
    }

    // ✅ Helper: Invalidate existing unused OTPs
    private async Task InvalidateExistingOtpsAsync(string email, string purpose, CancellationToken cancellationToken)
    {
        var existingOtps = await _context.OtpVerifications
            .Where(o => o.Email == email && o.Purpose == purpose && !o.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var otp in existingOtps)
        {
            otp.IsUsed = true;
            otp.UsedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    // ✅ Helper: Get user name by email
    private async Task<string?> GetUserNameByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return user?.FirstName ?? user?.UserName;
    }
}