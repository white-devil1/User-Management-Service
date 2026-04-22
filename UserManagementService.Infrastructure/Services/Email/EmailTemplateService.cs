using UserManagementService.Application.Services;

namespace UserManagementService.Infrastructure.Services.Email;

public class EmailTemplateService : IEmailTemplateService
{
    public string GenerateOtpEmailTemplate(string userName, string otp, string companyName, string companyUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Password Reset Request</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f4f4f4;"">
    <tr>
      <td align=""center"" style=""padding: 40px 0;"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
          <!-- Header -->
          <tr>
            <td style=""background-color: #4F46E5; padding: 40px; text-align: center; border-radius: 8px 8px 0 0;"">
              <h1 style=""color: #ffffff; margin: 0; font-size: 28px;"">Password Reset Request</h1>
            </td>
          </tr>
          
          <!-- Body -->
          <tr>
            <td style=""padding: 40px;"">
              <p style=""color: #333333; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
              
              <p style=""color: #333333; font-size: 16px; line-height: 1.6;"">
                We received a request to reset your password. Use the OTP (One-Time Password) below to reset your password:
              </p>
              
              <!-- OTP Box -->
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin: 30px 0;"">
                <tr>
                  <td align=""center"" style=""background-color: #F3F4F6; padding: 30px; border-radius: 8px;"">
                    <p style=""color: #6B7280; font-size: 14px; margin: 0 0 10px 0;"">Your OTP Code</p>
                    <p style=""color: #4F46E5; font-size: 36px; font-weight: bold; letter-spacing: 8px; margin: 0; font-family: 'Courier New', monospace;"">{otp}</p>
                  </td>
                </tr>
              </table>
              
              <p style=""color: #333333; font-size: 14px; line-height: 1.6;"">
                <strong>This OTP will expire in 10 minutes.</strong>
              </p>
              
              <p style=""color: #333333; font-size: 14px; line-height: 1.6;"">
                If you didn't request a password reset, please ignore this email or contact support if you have concerns.
              </p>
            </td>
          </tr>
          
          <!-- Footer -->
          <tr>
            <td style=""background-color: #F9FAFB; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;"">
              <p style=""color: #6B7280; font-size: 12px; margin: 0;"">
                This is an automated message, please do not reply.
              </p>
              <p style=""color: #6B7280; font-size: 12px; margin: 10px 0 0 0;"">
                © {DateTime.UtcNow.Year} {companyName}. All rights reserved.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public string GenerateAdminResetPasswordTemplate(string userName, string username, string tempPassword, string companyName, string companyUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Password Reset by Administrator</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f4f4f4;"">
    <tr>
      <td align=""center"" style=""padding: 40px 0;"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
          <!-- Header -->
          <tr>
            <td style=""background-color: #F59E0B; padding: 40px; text-align: center; border-radius: 8px 8px 0 0;"">
              <h1 style=""color: #ffffff; margin: 0; font-size: 28px;"">Password Reset by Administrator</h1>
            </td>
          </tr>
          
          <!-- Body -->
          <tr>
            <td style=""padding: 40px;"">
              <p style=""color: #333333; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
              
              <p style=""color: #333333; font-size: 16px; line-height: 1.6;"">
                Your password has been reset by an administrator. Please use the credentials below to log in:
              </p>
              
              <!-- Credentials Box -->
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin: 30px 0;"">
                <tr>
                  <td align=""center"" style=""background-color: #FEF3C7; padding: 30px; border-radius: 8px; border: 2px solid #F59E0B;"">
                    <!-- Username -->
                    <p style=""color: #92400E; font-size: 14px; margin: 0 0 10px 0;"">Username</p>
                    <p style=""color: #F59E0B; font-size: 24px; font-weight: bold; margin: 0 0 20px 0;"">{username}</p>
                    
                    <!-- Temporary Password -->
                    <p style=""color: #92400E; font-size: 14px; margin: 0 0 10px 0; border-top: 1px dashed #F59E0B; padding-top: 20px;"">Temporary Password</p>
                    <p style=""color: #F59E0B; font-size: 28px; font-weight: bold; letter-spacing: 4px; margin: 0; font-family: 'Courier New', monospace;"">{tempPassword}</p>
                  </td>
                </tr>
              </table>
              
              <div style=""background-color: #FEF3C7; border-left: 4px solid #F59E0B; padding: 15px; margin: 20px 0;"">
                <p style=""color: #92400E; font-size: 14px; margin: 0;"">
                  <strong>⚠️ Important:</strong> You will be required to change your password upon first login.
                </p>
              </div>
              
              <p style=""color: #333333; font-size: 14px; line-height: 1.6;"">
                If you have any questions or concerns, please contact your administrator.
              </p>
            </td>
          </tr>
          
          <!-- Footer -->
          <tr>
            <td style=""background-color: #F9FAFB; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;"">
              <p style=""color: #6B7280; font-size: 12px; margin: 0;"">
                This is an automated message, please do not reply.
              </p>
              <p style=""color: #6B7280; font-size: 12px; margin: 10px 0 0 0;"">
                © {DateTime.UtcNow.Year} {companyName}. All rights reserved.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public string GenerateWelcomeEmailTemplate(string userName, string username, string tempPassword, string companyName, string companyUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Welcome to {companyName}</title>
</head>
<body style=""margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f4f4f4;"">
    <tr>
      <td align=""center"" style=""padding: 40px 0;"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1);"">
          <!-- Header -->
          <tr>
            <td style=""background-color: #10B981; padding: 40px; text-align: center; border-radius: 8px 8px 0 0;"">
              <h1 style=""color: #ffffff; margin: 0; font-size: 28px;"">Welcome to {companyName}!</h1>
            </td>
          </tr>
          
          <!-- Body -->
          <tr>
            <td style=""padding: 40px;"">
              <p style=""color: #333333; font-size: 16px; line-height: 1.6;"">Hello {userName},</p>
              
              <p style=""color: #333333; font-size: 16px; line-height: 1.6;"">
                Welcome aboard! Your account has been successfully created. Below are your login credentials to get started:
              </p>
              
              <!-- Credentials Box -->
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin: 30px 0;"">
                <tr>
                  <td align=""center"" style=""background-color: #D1FAE5; padding: 30px; border-radius: 8px; border: 2px solid #10B981;"">
                    <!-- Email -->
                    <p style=""color: #065F46; font-size: 14px; margin: 0 0 10px 0;"">Email</p>
                    <p style=""color: #10B981; font-size: 24px; font-weight: bold; margin: 0 0 20px 0;"">{username}</p>
                    
                    <!-- Temporary Password -->
                    <p style=""color: #065F46; font-size: 14px; margin: 0 0 10px 0; border-top: 1px dashed #10B981; padding-top: 20px;"">Temporary Password</p>
                    <p style=""color: #10B981; font-size: 28px; font-weight: bold; letter-spacing: 4px; margin: 0; font-family: 'Courier New', monospace;"">{tempPassword}</p>
                  </td>
                </tr>
              </table>
              
              <div style=""background-color: #D1FAE5; border-left: 4px solid #10B981; padding: 15px; margin: 20px 0;"">
                <p style=""color: #065F46; font-size: 14px; margin: 0;"">
                  <strong>🔒 Security Notice:</strong> For your security, you will be required to change your password upon first login.
                </p>
              </div>
              
              <p style=""color: #333333; font-size: 14px; line-height: 1.6;"">
                If you have any questions or need assistance, please don't hesitate to contact your administrator.
              </p>
              
              <p style=""color: #333333; font-size: 14px; line-height: 1.6;"">
                We're excited to have you on board!
              </p>
            </td>
          </tr>
          
          <!-- Footer -->
          <tr>
            <td style=""background-color: #F9FAFB; padding: 30px; text-align: center; border-radius: 0 0 8px 8px;"">
              <p style=""color: #6B7280; font-size: 12px; margin: 0;"">
                This is an automated message, please do not reply.
              </p>
              <p style=""color: #6B7280; font-size: 12px; margin: 10px 0 0 0;"">
                © {DateTime.UtcNow.Year} {companyName}. All rights reserved.
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}