using UserManagementService.Application.Services;

namespace UserManagementService.Infrastructure.Services.Email;

public class EmailTemplateService : IEmailTemplateService
{
    public string GenerateOtpEmailTemplate(string displayName, string otp, string companyName, string companyUrl)
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
              <p style=""color: #333333; font-size: 16px; line-height: 1.6;"">Hello {displayName},</p>
              
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

    public string GenerateAdminResetPasswordTemplate(string displayName, string loginEmail, string tempPassword, string companyName, string companyUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Your Login Credentials — {companyName}</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Arial, sans-serif; background-color: #f0f2f5;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f0f2f5;"">
    <tr>
      <td align=""center"" style=""padding: 48px 16px;"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 24px rgba(0,0,0,0.08); overflow: hidden;"">

          <!-- Header -->
          <tr>
            <td style=""background: linear-gradient(135deg, #2563eb 0%, #7c3aed 100%); padding: 48px 40px; text-align: center;"">
              <p style=""color: rgba(255,255,255,0.85); font-size: 13px; font-weight: 600; letter-spacing: 0.1em; text-transform: uppercase; margin: 0 0 12px 0;"">{companyName}</p>
              <h1 style=""color: #ffffff; margin: 0; font-size: 26px; font-weight: 700; line-height: 1.3;"">Your Account Password Has Been Reset</h1>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style=""padding: 40px 40px 32px;"">
              <p style=""color: #374151; font-size: 16px; line-height: 1.7; margin: 0 0 16px 0;"">Hello {displayName},</p>

              <p style=""color: #6b7280; font-size: 15px; line-height: 1.7; margin: 0 0 32px 0;"">
                A new temporary password has been issued for your account. Use the credentials below to sign in. You will be prompted to set a new password immediately after logging in.
              </p>

              <!-- Credentials Card -->
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin: 0 0 28px 0;"">
                <tr>
                  <td style=""background-color: #f8fafc; border: 1.5px solid #e2e8f0; border-radius: 10px; padding: 28px 32px;"">

                    <!-- Email row -->
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 20px;"">
                      <tr>
                        <td style=""padding-bottom: 6px;"">
                          <p style=""color: #9ca3af; font-size: 11px; font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase; margin: 0;"">Login Email</p>
                        </td>
                      </tr>
                      <tr>
                        <td style=""background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 6px; padding: 12px 16px;"">
                          <p style=""color: #1e293b; font-size: 16px; font-weight: 600; margin: 0; word-break: break-all;"">{loginEmail}</p>
                        </td>
                      </tr>
                    </table>

                    <!-- Divider -->
                    <hr style=""border: none; border-top: 1px dashed #e2e8f0; margin: 0 0 20px 0;"" />

                    <!-- Password row -->
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
                      <tr>
                        <td style=""padding-bottom: 6px;"">
                          <p style=""color: #9ca3af; font-size: 11px; font-weight: 600; letter-spacing: 0.08em; text-transform: uppercase; margin: 0;"">Temporary Password</p>
                        </td>
                      </tr>
                      <tr>
                        <td style=""background-color: #ffffff; border: 1px solid #e2e8f0; border-radius: 6px; padding: 12px 16px;"">
                          <p style=""color: #2563eb; font-size: 20px; font-weight: 700; letter-spacing: 3px; margin: 0; font-family: 'Courier New', monospace;"">{tempPassword}</p>
                        </td>
                      </tr>
                    </table>

                  </td>
                </tr>
              </table>

              <!-- Warning notice -->
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 28px;"">
                <tr>
                  <td style=""background-color: #fffbeb; border: 1px solid #fde68a; border-radius: 8px; padding: 16px 20px;"">
                    <p style=""color: #92400e; font-size: 13px; line-height: 1.6; margin: 0;"">
                      <strong>Important:</strong> This temporary password expires in <strong>24 hours</strong>.
                      You will be asked to set a new password the first time you log in.
                      Do not share your credentials with anyone.
                    </p>
                  </td>
                </tr>
              </table>

              <p style=""color: #9ca3af; font-size: 13px; line-height: 1.6; margin: 0;"">
                If you did not request this change or believe this was sent in error, please contact support immediately.
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""background-color: #f8fafc; padding: 24px 40px; text-align: center; border-top: 1px solid #e2e8f0;"">
              <p style=""color: #9ca3af; font-size: 12px; margin: 0 0 4px 0;"">This is an automated notification — please do not reply to this email.</p>
              <p style=""color: #9ca3af; font-size: 12px; margin: 0;"">© {DateTime.UtcNow.Year} {companyName}. All rights reserved.</p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    public string GenerateWelcomeEmailTemplate(string displayName, string loginEmail, string tempPassword, string companyName, string companyUrl)
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
              <p style=""color: #333333; font-size: 16px; line-height: 1.6;"">Hello {displayName},</p>
              
              <p style=""color: #333333; font-size: 16px; line-height: 1.6;"">
                Welcome aboard! Your account has been successfully created. Below are your login credentials to get started:
              </p>
              
              <!-- Credentials Box -->
              <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin: 30px 0;"">
                <tr>
                  <td align=""center"" style=""background-color: #D1FAE5; padding: 30px; border-radius: 8px; border: 2px solid #10B981;"">
                    <!-- Email -->
                    <p style=""color: #065F46; font-size: 14px; margin: 0 0 10px 0;"">Email</p>
                    <p style=""color: #10B981; font-size: 24px; font-weight: bold; margin: 0 0 20px 0;"">{loginEmail}</p>
                    
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