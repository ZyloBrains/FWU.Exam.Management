using FWU.Exam.Management.Domain.Enums;

namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Built-in fallback notification templates. These are used whenever no active
/// <see cref="FWU.Exam.Management.Domain.Entities.Notifications.NotificationTemplate"/>
/// exists in the database for a (Code, Channel) pair, so notifications always work
/// even before templates are seeded or edited.
///
/// Email bodies are the inner content region; the shared HTML layout is applied by
/// the template renderer. SMS bodies are plain text. Placeholders use {{Token}}.
/// </summary>
public sealed record TemplateDefinition(
    string Code,
    string Name,
    NotificationChannel Channel,
    string? Subject,
    string Body,
    string? PlaceholdersHelp);

public static class NotificationTemplateDefaults
{
    private static readonly TemplateDefinition ResetPasswordEmail = new(
        Code: "reset_password",
        Name: "Password Reset",
        Channel: NotificationChannel.Email,
        Subject: "Reset Your Password",
        PlaceholdersHelp: "UserName, CallbackUrl",
        Body: """
            <h2 style="color:#2c3e50;margin:0 0 8px;font-size:20px;">Forgot Your Password?</h2>
            <p style="color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;">
                Hi {{UserName}}, we received a request to reset the password for your Far-Western University account. Click the button below to create a new password.
            </p>
            <table role="presentation" cellpadding="0" cellspacing="0" style="margin:24px 0;">
                <tr>
                    <td align="center" style="background:#2980b9;border-radius:6px;">
                        <a href="{{CallbackUrl}}" style="display:inline-block;padding:12px 32px;color:#ffffff;text-decoration:none;font-size:15px;font-weight:500;">Reset Password</a>
                    </td>
                </tr>
            </table>
            <p style="color:#888;line-height:1.6;margin:20px 0 0;font-size:12px;">
                If the button above does not work, copy and paste this link:<br>
                <a href="{{CallbackUrl}}" style="color:#2980b9;word-break:break-all;">{{CallbackUrl}}</a>
            </p>
            <p style="color:#888;line-height:1.6;margin:12px 0 0;font-size:12px;">
                This link will expire after a limited time. If you did not request a password reset, please ignore this email.
            </p>
            """);

    private static readonly TemplateDefinition ConfirmEmail = new(
        Code: "confirm_email",
        Name: "Confirm Email",
        Channel: NotificationChannel.Email,
        Subject: "Confirm Your Email",
        PlaceholdersHelp: "UserName, CallbackUrl",
        Body: """
            <h2 style="color:#2c3e50;margin:0 0 8px;font-size:20px;">Welcome, {{UserName}}!</h2>
            <p style="color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;">
                Thank you for creating an account with Far-Western University. Please confirm your email address to activate your account and get started.
            </p>
            <table role="presentation" cellpadding="0" cellspacing="0" style="margin:24px 0;">
                <tr>
                    <td align="center" style="background:#2980b9;border-radius:6px;">
                        <a href="{{CallbackUrl}}" style="display:inline-block;padding:12px 32px;color:#ffffff;text-decoration:none;font-size:15px;font-weight:500;">Confirm Email Address</a>
                    </td>
                </tr>
            </table>
            <p style="color:#888;line-height:1.6;margin:20px 0 0;font-size:12px;">
                If the button above does not work, copy and paste the following link into your browser:<br>
                <a href="{{CallbackUrl}}" style="color:#2980b9;word-break:break-all;">{{CallbackUrl}}</a>
            </p>
            <p style="color:#888;line-height:1.6;margin:12px 0 0;font-size:12px;">
                This link will expire after a limited time. If you did not create this account, please ignore this email.
            </p>
            """);

    private static readonly TemplateDefinition ChangeEmail = new(
        Code: "change_email",
        Name: "Change Email",
        Channel: NotificationChannel.Email,
        Subject: "Confirm Email Change",
        PlaceholdersHelp: "UserName, CallbackUrl",
        Body: """
            <h2 style="color:#2c3e50;margin:0 0 8px;font-size:20px;">Email Change Request</h2>
            <p style="color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;">
                Hi {{UserName}}, we received a request to change the email address associated with your account. Please confirm this change by clicking the button below.
            </p>
            <table role="presentation" cellpadding="0" cellspacing="0" style="margin:24px 0;">
                <tr>
                    <td align="center" style="background:#2980b9;border-radius:6px;">
                        <a href="{{CallbackUrl}}" style="display:inline-block;padding:12px 32px;color:#ffffff;text-decoration:none;font-size:15px;font-weight:500;">Confirm Email Change</a>
                    </td>
                </tr>
            </table>
            <p style="color:#888;line-height:1.6;margin:20px 0 0;font-size:12px;">
                If the button above does not work, copy and paste this link:<br>
                <a href="{{CallbackUrl}}" style="color:#2980b9;word-break:break-all;">{{CallbackUrl}}</a>
            </p>
            <p style="color:#888;line-height:1.6;margin:12px 0 0;font-size:12px;">
                This link will expire after a limited time. If you did not request this change, please ignore this email.
            </p>
            """);

    private static readonly TemplateDefinition EntranceApplicationSubmittedEmail = new(
        Code: "entrance_application_submitted",
        Name: "Entrance Application Submitted",
        Channel: NotificationChannel.Email,
        Subject: "Application Submitted Successfully",
        PlaceholdersHelp: "FullName, College, Program, ApplicationId, Date",
        Body: """
            <h2 style="color:#2c3e50;margin:0 0 8px;font-size:20px;">Dear {{FullName}},</h2>
            <p style="color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;">
                Your entrance exam application has been submitted successfully. Below are your application details for your reference.
            </p>
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f8f9fa;border-radius:8px;padding:16px 20px;margin:20px 0;">
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">College:</strong> <span style="color:#555;font-size:13px;">{{College}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Program:</strong> <span style="color:#555;font-size:13px;">{{Program}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Application ID:</strong> <span style="color:#555;font-size:13px;">{{ApplicationId}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Submitted Date:</strong> <span style="color:#555;font-size:13px;">{{Date}}</span></td></tr>
            </table>
            <p style="color:#555;line-height:1.7;margin:20px 0 0;font-size:14px;">
                You will be notified once your application has been reviewed. Please keep your Application ID for future reference.
            </p>
            """);

    private static readonly TemplateDefinition EntranceApplicationSubmittedSms = new(
        Code: "entrance_application_submitted",
        Name: "Entrance Application Submitted",
        Channel: NotificationChannel.Sms,
        Subject: null,
        PlaceholdersHelp: "FullName, Program, College, ApplicationId",
        Body: "Dear {{FullName}}, your entrance application for {{Program}} at {{College}} has been submitted successfully. Application ID: {{ApplicationId}}. - FWU");

    private static readonly TemplateDefinition StudentRegistrationCredentialsEmail = new(
        Code: "student_registration_credentials",
        Name: "Student Registration Credentials",
        Channel: NotificationChannel.Email,
        Subject: "Registration Successful - Login Credentials",
        PlaceholdersHelp: "FullName, RegistrationNumber, College, Program, Email, Password, LoginUrl",
        Body: """
            <h2 style="color:#2c3e50;margin:0 0 8px;font-size:20px;">Dear {{FullName}},</h2>
            <p style="color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;">
                Your student registration has been created successfully. Please find your registration details and login credentials below.
            </p>
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f8f9fa;border-radius:8px;padding:16px 20px;margin:20px 0;">
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Registration No:</strong> <span style="color:#555;font-size:13px;">{{RegistrationNumber}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">College:</strong> <span style="color:#555;font-size:13px;">{{College}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Program:</strong> <span style="color:#555;font-size:13px;">{{Program}}</span></td></tr>
            </table>
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#eaf2f8;border-radius:8px;padding:16px 20px;margin:20px 0;border-left:4px solid #2980b9;">
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Username (Email):</strong> <span style="color:#2980b9;font-size:13px;">{{Email}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Password:</strong> <span style="color:#555;font-size:13px;">{{Password}}</span></td></tr>
            </table>
            <table role="presentation" cellpadding="0" cellspacing="0" style="margin:24px 0;">
                <tr>
                    <td align="center" style="background:#2980b9;border-radius:6px;">
                        <a href="{{LoginUrl}}" style="display:inline-block;padding:12px 32px;color:#ffffff;text-decoration:none;font-size:15px;font-weight:500;">Login to Your Account</a>
                    </td>
                </tr>
            </table>
            <p style="color:#888;line-height:1.6;margin:0 0 12px;font-size:12px;">
                If the button above does not work, copy and paste the following link into your browser:<br>
                <a href="{{LoginUrl}}" style="color:#2980b9;word-break:break-all;">{{LoginUrl}}</a>
            </p>
            <p style="color:#555;line-height:1.6;margin:0 0 12px;font-size:13px;">
                You can sign in using your email address <strong>{{Email}}</strong> or your registration number <strong>{{RegistrationNumber}}</strong> along with the password above.
            </p>
            <p style="color:#e74c3c;line-height:1.6;margin:16px 0 0;font-size:13px;font-weight:500;">
                For security reasons, please change your password after your first login.
            </p>
            """);

    private static readonly TemplateDefinition StudentRegistrationCredentialsSms = new(
        Code: "student_registration_credentials",
        Name: "Student Registration Credentials",
        Channel: NotificationChannel.Sms,
        Subject: null,
        PlaceholdersHelp: "FullName, RegistrationNumber, Email, Password",
        Body: "Dear {{FullName}}, your registration is complete. Reg No: {{RegistrationNumber}}, Username: {{Email}}, Password: {{Password}}. Please change password on first login. - FWU");

    private static readonly TemplateDefinition TenantAccountCreated = new(
        Code: "tenant_account_created",
        Name: "Tenant Account Created",
        Channel: NotificationChannel.Email,
        Subject: "Tenant Account Created",
        PlaceholdersHelp: "FullName, TenantName, OfficeCode, AdminEmail",
        Body: """
            <h2 style="color:#2c3e50;margin:0 0 8px;font-size:20px;">Dear {{FullName}},</h2>
            <p style="color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;">
                Your tenant account has been created successfully. Below are the details for your reference.
            </p>
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f8f9fa;border-radius:8px;padding:16px 20px;margin:20px 0;">
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Tenant:</strong> <span style="color:#555;font-size:13px;">{{TenantName}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Office Code:</strong> <span style="color:#555;font-size:13px;">{{OfficeCode}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Admin Email:</strong> <span style="color:#555;font-size:13px;">{{AdminEmail}}</span></td></tr>
            </table>
            <p style="color:#555;line-height:1.7;margin:20px 0 0;font-size:14px;">
                Please use your email address <strong>{{AdminEmail}}</strong> to log in. If you have not set your password yet, use the <strong>"Forgot Password"</strong> option on the login page.
            </p>
            """);

    private static readonly TemplateDefinition ExamScheduleCreatedEmail = new(
        Code: "exam_schedule_created",
        Name: "Exam Schedule Created",
        Channel: NotificationChannel.Email,
        Subject: "New Exam Schedule: {{ExamScheduleName}}",
        PlaceholdersHelp: "ExamScheduleName, ExamScheduleCode, ProgramName, SemesterName, ExamTypeName, AcademicYearName, StartDate, EndDate, StartTime, EndTime, StartDateBs, EndDateBs, CollegeName, Remarks",
        Body: """
            <h2 style="color:#2c3e50;margin:0 0 8px;font-size:20px;">Dear {{CollegeName}},</h2>
            <p style="color:#555;line-height:1.7;margin:0 0 20px;font-size:14px;">
                A new exam schedule has been created and requires your college's approval. Please review the details below.
            </p>
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f8f9fa;border-radius:8px;padding:16px 20px;margin:20px 0;">
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Schedule:</strong> <span style="color:#555;font-size:13px;">{{ExamScheduleName}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Code:</strong> <span style="color:#555;font-size:13px;">{{ExamScheduleCode}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Program:</strong> <span style="color:#555;font-size:13px;">{{ProgramName}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Semester:</strong> <span style="color:#555;font-size:13px;">{{SemesterName}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Exam Type:</strong> <span style="color:#555;font-size:13px;">{{ExamTypeName}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Academic Year:</strong> <span style="color:#555;font-size:13px;">{{AcademicYearName}}</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Start Date:</strong> <span style="color:#555;font-size:13px;">{{StartDate}} ({{StartDateBs}})</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">End Date:</strong> <span style="color:#555;font-size:13px;">{{EndDate}} ({{EndDateBs}})</span></td></tr>
                <tr><td style="padding:4px 0;"><strong style="color:#2c3e50;font-size:13px;">Time:</strong> <span style="color:#555;font-size:13px;">{{StartTime}} - {{EndTime}}</span></td></tr>
            </table>
            <p style="color:#555;line-height:1.7;margin:20px 0 0;font-size:14px;">
                Please log in to the exam management system to approve or reject this schedule.
            </p>
            """);

    private static readonly TemplateDefinition ExamScheduleCreatedSms = new(
        Code: "exam_schedule_created",
        Name: "Exam Schedule Created",
        Channel: NotificationChannel.Sms,
        Subject: null,
        PlaceholdersHelp: "ExamScheduleCode, ExamScheduleName, StartDate",
        Body: "Dear {{CollegeName}}, a new exam schedule ({{ExamScheduleCode}} - {{ExamScheduleName}}) has been created starting {{StartDate}}. Please login to approve. - FWU");

    public static readonly IReadOnlyList<TemplateDefinition> All =
    [
        ResetPasswordEmail,
        ConfirmEmail,
        ChangeEmail,
        EntranceApplicationSubmittedEmail,
        EntranceApplicationSubmittedSms,
        StudentRegistrationCredentialsEmail,
        StudentRegistrationCredentialsSms,
        TenantAccountCreated,
        ExamScheduleCreatedEmail,
        ExamScheduleCreatedSms,
    ];

    public static TemplateDefinition? Get(string code, NotificationChannel channel)
        => All.FirstOrDefault(t => t.Code == code && t.Channel == channel);
}
