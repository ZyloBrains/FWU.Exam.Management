namespace FWU.Exam.Management.Domain.Constants;

/// <summary>
/// Fixed vocabulary of business-activity types recorded via IAuditLogWriter.
/// Grouped by feature area. Values are stored verbatim in AuditLogs.ActivityType.
/// </summary>
public static class ActivityTypes
{
    // Security & users
    public const string UserLogin = "user.login";
    public const string UserLoginFailed = "user.login_failed";
    public const string UserLoginLockedOut = "user.login_locked_out";
    public const string UserLogout = "user.logout";
    public const string UserPasswordResetRequested = "user.password_reset_requested";
    public const string UserPasswordReset = "user.password_reset";
    public const string UserPasswordResetByAdmin = "user.password_reset_by_admin";
    public const string UserCreated = "user.created";
    public const string UserUpdated = "user.updated";
    public const string UserStatusChanged = "user.status_changed";
    public const string UserDeleted = "user.deleted";
    public const string UserRolesChanged = "user.roles_changed";
    public const string RoleCreated = "role.created";
    public const string RoleUpdated = "role.updated";
    public const string RoleDeleted = "role.deleted";
    public const string PermissionsUpdated = "permissions.updated";

    // Payments
    public const string PaymentInitiated = "payment.initiated";
    public const string PaymentProcessed = "payment.processed";
    public const string PaymentVerified = "payment.verified";
    public const string PaymentVerificationFailed = "payment.verification_failed";
    public const string PaymentGatewayConfigured = "payment.gateway.configured";
    public const string PaymentReconciled = "payment.reconciled";
    public const string PaymentMarkedFailed = "payment.marked_failed";

    // Approvals
    public const string ExamScheduleCreated = "exam-schedule.created";
    public const string ExamScheduleUpdated = "exam-schedule.updated";
    public const string ExamScheduleDeleted = "exam-schedule.deleted";
    public const string EntranceSubmitted = "entrance.application.submitted";
    public const string EntranceUpdated = "entrance.application.updated";
    public const string EntranceApproved = "entrance.application.approved";
    public const string EntranceRejected = "entrance.application.rejected";
    public const string EntranceUnderReview = "entrance.application.under_review";
    public const string EntranceConvertedToAdmission = "entrance.application.converted_to_admission";

    // Exams, marks & results
    public const string MarksSaved = "marks.saved";
    public const string MarksSubmitted = "marks.submitted";
    public const string MarksImported = "marks.imported";
    public const string ResultSubmitted = "result.submitted";
    public const string ResultExported = "result.exported";
    public const string RetotalRequested = "retotal.requested";
    public const string RetotalUnderReview = "retotal.under_review";
    public const string RetotalApproved = "retotal.approved";
    public const string RetotalRejected = "retotal.rejected";
    public const string AdmitCardGenerated = "admit-card.generated";

    // Bulk & system
    public const string UsersBulkCreationStarted = "users.bulk_creation_started";
    public const string UsersBulkCreationCompleted = "users.bulk_creation_completed";
    public const string UsersBulkCreationFailed = "users.bulk_creation_failed";
    public const string DatabaseBackupCreated = "database.backup_created";
    public const string DatabaseRestored = "database.restored";
    public const string DatabaseBackupDeleted = "database.backup_deleted";
    public const string ReportExported = "report.exported";
    public const string SmtpConfigUpdated = "config.smtp.updated";
    public const string SmsConfigUpdated = "config.sms.updated";
    public const string EmailConfigUpdated = "config.email.updated";
    public const string TestEmailSent = "communication.test_email_sent";
    public const string TestSmsSent = "communication.test_sms_sent";
    public const string SemesterPromotionCompleted = "semester.promotion_completed";
}
