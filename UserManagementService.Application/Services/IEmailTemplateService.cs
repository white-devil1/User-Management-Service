namespace UserManagementService.Application.Services;

public interface IEmailTemplateService
{
    string GenerateOtpEmailTemplate(string userName, string otp, string companyName, string companyUrl);

    string GenerateAdminResetPasswordTemplate(string userName, string username, string tempPassword, string companyName, string companyUrl);
}