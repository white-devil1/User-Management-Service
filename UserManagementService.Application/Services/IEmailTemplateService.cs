namespace UserManagementService.Application.Services;

public interface IEmailTemplateService
{
    string GenerateOtpEmailTemplate(string displayName, string otp, string companyName, string companyUrl);

    string GenerateAdminResetPasswordTemplate(string displayName, string loginEmail, string tempPassword, string companyName, string companyUrl);

    string GenerateWelcomeEmailTemplate(string displayName, string loginEmail, string tempPassword, string companyName, string companyUrl);
}
