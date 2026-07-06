namespace FWU.Exam.Management.Infrastructure.Services;

public static class EmailTemplateHelper
{
    public static string? LogoUrl { get; set; }
    public static string? LogoBase64 { get; set; }

    private static string LogoImg()
    {
        var src = LogoBase64 ?? LogoUrl;
        return string.IsNullOrEmpty(src)
            ? ""
            : $@"<img src=""{src}"" alt=""Far-Western University"" style=""max-width:140px;height:auto;display:block;margin:0 auto 10px;border-radius:8px;"" class=""logo-img"" onerror=""this.style.display='none'"" />";
    }

    private static string Layout(string title, string content) => $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no"">
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
    <title>{title}</title>
    <style type=""text/css"">
        @@media only screen and (max-width:640px) {{
            .email-container {{ width:100% !important; max-width:100% !important; }}
            .email-padding {{ padding:24px 20px !important; }}
            .header-padding {{ padding:24px 20px !important; }}
            .footer-padding {{ padding:16px 20px !important; }}
            .content-heading {{ font-size:18px !important; }}
            .content-text {{ font-size:14px !important; }}
            .btn-table {{ width:100% !important; }}
            .btn-td {{ display:block !important; width:100% !important; }}
            .btn-link {{ display:block !important; width:100% !important; padding:14px 20px !important; font-size:15px !important; text-align:center !important; box-sizing:border-box !important; }}
            .details-table {{ padding:12px 14px !important; }}
            .details-table td {{ font-size:13px !important; }}
            .creds-table {{ padding:12px 14px !important; }}
            .fallback-link {{ word-break:break-all !important; font-size:12px !important; }}
            .footer-text {{ font-size:11px !important; }}
        }}
        @@media only screen and (max-width:480px) {{
            .email-padding {{ padding:20px 16px !important; }}
            .header-padding {{ padding:20px 16px !important; }}
            .content-heading {{ font-size:17px !important; }}
            .content-text {{ font-size:13px !important; }}
            .logo-img {{ max-width:110px !important; }}
        }}
    </style>
</head>
<body style=""margin:0;padding:0;background-color:#f4f6f9;font-family:'Segoe UI',Tahoma,Geneva,Verdana,sans-serif; -webkit-text-size-adjust:100%; -ms-text-size-adjust:100%;"">
    <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f6f9;padding:20px 0;"" class=""body-table"">
        <tr>
            <td align=""center"" valign=""top"">
                <!--[if mso]><table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" align=""center""><tr><td><![endif]-->
                <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%;background-color:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.08);"" class=""email-container"">
                    <tr>
                        <td style=""background:linear-gradient(135deg,#1a5276,#2980b9);padding:28px 32px;text-align:center;"" class=""header-padding"">
                            {LogoImg()}
                            <h1 style=""color:#ffffff;margin:0;font-size:22px;font-weight:600;letter-spacing:0.5px;"" class=""content-heading"">Far-Western University</h1>
                            <p style=""color:rgba(255,255,255,0.85);margin:6px 0 0;font-size:13px;"">Exam Management Information System</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding:32px 36px 20px;"" class=""email-padding"">
                            {content}
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color:#f8f9fa;padding:18px 36px;text-align:center;border-top:1px solid #e9ecef;"" class=""footer-padding"">
                            <p style=""margin:0;font-size:12px;color:#888;line-height:1.6;"" class=""footer-text"">
                                Far-Western University &bull; Mahendranagar, Nepal<br>
                                This is an automated message. Please do not reply directly.
                            </p>
                        </td>
                    </tr>
                </table>
                <!--[if mso]></td></tr></table><![endif]-->
            </td>
        </tr>
    </table>
</body>
</html>";

    public static string ConfirmEmail(string userName, string callbackUrl) => Layout("Confirm Your Email", $@"
        <h2 style=""color:#2c3e50;margin:0 0 8px;font-size:20px;"">Welcome, {userName}!</h2>
        <p style=""color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;"">
            Thank you for creating an account with Far-Western University. Please confirm your email address to activate your account and get started.
        </p>
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:24px 0;"" class=""btn-table"">
            <tr>
                <td align=""center"" style=""background:#2980b9;border-radius:6px;"" class=""btn-td"">
                    <a href=""{callbackUrl}"" style=""display:inline-block;padding:12px 32px;color:#ffffff;text-decoration:none;font-size:15px;font-weight:500;"" class=""btn-link"">Confirm Email Address</a>
                </td>
            </tr>
        </table>
        <p style=""color:#888;line-height:1.6;margin:20px 0 0;font-size:12px;"">
            If the button above does not work, copy and paste the following link into your browser:<br>
            <a href=""{callbackUrl}"" style=""color:#2980b9;word-break:break-all;"" class=""fallback-link"">{callbackUrl}</a>
        </p>
        <p style=""color:#888;line-height:1.6;margin:12px 0 0;font-size:12px;"">
            This link will expire after a limited time. If you did not create this account, please ignore this email.
        </p>
    ");

    public static string ChangeEmail(string userName, string callbackUrl) => Layout("Confirm Email Change", $@"
        <h2 style=""color:#2c3e50;margin:0 0 8px;font-size:20px;"" class=""content-heading"">Email Change Request</h2>
        <p style=""color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;"" class=""content-text"">
            Hi {userName}, we received a request to change the email address associated with your account. Please confirm this change by clicking the button below.
        </p>
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:24px 0;"" class=""btn-table"">
            <tr>
                <td align=""center"" style=""background:#2980b9;border-radius:6px;"" class=""btn-td"">
                    <a href=""{callbackUrl}"" style=""display:inline-block;padding:12px 32px;color:#ffffff;text-decoration:none;font-size:15px;font-weight:500;"" class=""btn-link"">Confirm Email Change</a>
                </td>
            </tr>
        </table>
        <p style=""color:#888;line-height:1.6;margin:20px 0 0;font-size:12px;"">
            If the button above does not work, copy and paste this link:<br>
            <a href=""{callbackUrl}"" style=""color:#2980b9;word-break:break-all;"" class=""fallback-link"">{callbackUrl}</a>
        </p>
        <p style=""color:#888;line-height:1.6;margin:12px 0 0;font-size:12px;"">
            This link will expire after a limited time. If you did not request this change, please ignore this email.
        </p>
    ");

    public static string ResetPassword(string userName, string callbackUrl) => Layout("Reset Your Password", $@"
        <h2 style=""color:#2c3e50;margin:0 0 8px;font-size:20px;"" class=""content-heading"">Forgot Your Password?</h2>
        <p style=""color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;"" class=""content-text"">
            Hi {userName}, we received a request to reset the password for your Far-Western University account. Click the button below to create a new password.
        </p>
        <table role=""presentation"" cellpadding=""0"" cellspacing=""0"" style=""margin:24px 0;"" class=""btn-table"">
            <tr>
                <td align=""center"" style=""background:#2980b9;border-radius:6px;"" class=""btn-td"">
                    <a href=""{callbackUrl}"" style=""display:inline-block;padding:12px 32px;color:#ffffff;text-decoration:none;font-size:15px;font-weight:500;"" class=""btn-link"">Reset Password</a>
                </td>
            </tr>
        </table>
        <p style=""color:#888;line-height:1.6;margin:20px 0 0;font-size:12px;"">
            If the button above does not work, copy and paste this link:<br>
            <a href=""{callbackUrl}"" style=""color:#2980b9;word-break:break-all;"" class=""fallback-link"">{callbackUrl}</a>
        </p>
        <p style=""color:#888;line-height:1.6;margin:12px 0 0;font-size:12px;"">
            This link will expire after a limited time. If you did not request a password reset, please ignore this email.
        </p>
    ");

    public static string EntranceApplicationSubmitted(string fullName, string college, string program, int applicationId, string date) => Layout("Application Submitted Successfully", $@"
        <h2 style=""color:#2c3e50;margin:0 0 8px;font-size:20px;"" class=""content-heading"">Dear {fullName},</h2>
        <p style=""color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;"" class=""content-text"">
            Your entrance exam application has been submitted successfully. Below are your application details for your reference.
        </p>
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8f9fa;border-radius:8px;padding:16px 20px;margin:20px 0;"" class=""details-table"">
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">College:</strong> <span style=""color:#555;font-size:13px;"">{college}</span></td></tr>
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Program:</strong> <span style=""color:#555;font-size:13px;"">{program}</span></td></tr>
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Application ID:</strong> <span style=""color:#555;font-size:13px;"">{applicationId}</span></td></tr>
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Submitted Date:</strong> <span style=""color:#555;font-size:13px;"">{date}</span></td></tr>
        </table>
        <p style=""color:#555;line-height:1.7;margin:20px 0 0;font-size:14px;"">
            You will be notified once your application has been reviewed. Please keep your Application ID for future reference.
        </p>
    ");

    public static string StudentRegistrationCredentials(string fullName, string registrationNumber, string college, string program, string email, string password) => Layout("Registration Successful - Login Credentials", $@"
        <h2 style=""color:#2c3e50;margin:0 0 8px;font-size:20px;"" class=""content-heading"">Dear {fullName},</h2>
        <p style=""color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;"" class=""content-text"">
            Your student registration has been created successfully. Please find your registration details and login credentials below.
        </p>
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8f9fa;border-radius:8px;padding:16px 20px;margin:20px 0;"" class=""details-table"">
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Registration No:</strong> <span style=""color:#555;font-size:13px;"">{registrationNumber}</span></td></tr>
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">College:</strong> <span style=""color:#555;font-size:13px;"">{college}</span></td></tr>
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Program:</strong> <span style=""color:#555;font-size:13px;"">{program}</span></td></tr>
        </table>
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#eaf2f8;border-radius:8px;padding:16px 20px;margin:20px 0;border-left:4px solid #2980b9;"" class=""creds-table"">
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Username (Email):</strong> <span style=""color:#2980b9;font-size:13px;"">{email}</span></td></tr>
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Password:</strong> <span style=""color:#555;font-size:13px;"">{password}</span></td></tr>
        </table>
        <p style=""color:#e74c3c;line-height:1.6;margin:16px 0 0;font-size:13px;font-weight:500;"">
            For security reasons, please change your password after your first login.
        </p>
    ");

    public static string TenantAccountCreated(string fullName, string tenantName, string officeCode, string adminEmail) => Layout("Tenant Account Created", $@"
        <h2 style=""color:#2c3e50;margin:0 0 8px;font-size:20px;"" class=""content-heading"">Dear {fullName},</h2>
        <p style=""color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;"" class=""content-text"">
            Your tenant account has been created successfully. Below are the details for your reference.
        </p>
        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f8f9fa;border-radius:8px;padding:16px 20px;margin:20px 0;"" class=""details-table"">
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Tenant:</strong> <span style=""color:#555;font-size:13px;"">{tenantName}</span></td></tr>
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Office Code:</strong> <span style=""color:#555;font-size:13px;"">{officeCode}</span></td></tr>
            <tr><td style=""padding:4px 0;""><strong style=""color:#2c3e50;font-size:13px;"">Admin Email:</strong> <span style=""color:#555;font-size:13px;"">{adminEmail}</span></td></tr>
        </table>
        <p style=""color:#555;line-height:1.7;margin:20px 0 0;font-size:14px;"">
            Please use your email address <strong>{adminEmail}</strong> to log in. If you have not set your password yet, use the <strong>""Forgot Password""</strong> option on the login page.
        </p>
    ");
}
