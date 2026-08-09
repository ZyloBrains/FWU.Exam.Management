using FWU.Exam.Management.Domain.Constants;

namespace FWU.Exam.Management.Web.Navigation;

public sealed record MenuRoute(string? Area = null, string? Controller = null, string? Action = null, string? Page = null);

public sealed record MenuItem(
    string Label,
    string Icon,
    string[] Permissions,
    MenuRoute Target,
    string[] Aliases,
    MenuRoute[]? ActiveOns = null);

public sealed record MenuSection(
    string Key,
    string Title,
    string Icon,
    string Description,
    string IconColor,
    string BgColor,
    string HoverBgColor,
    MenuItem[] Items,
    MenuRoute? Landing = null,
    bool SingleLink = false,
    bool SuperAdminOnly = false,
    string? RoleGate = null,
    bool ShowOnDashboard = true,
    MenuRoute[]? SectionActiveOns = null);

public static class AppMenu
{
    private static MenuRoute R(string? area, string? controller, string? action, string? page = null) => new(area, controller, action, page);

    private static MenuItem M(string label, string icon, string[] perms, MenuRoute target, string[] aliases, MenuRoute[]? active = null)
        => new(label, icon, perms, target, aliases, active);

    public static MenuSection[] Sections { get; } =
    [
        new("Dashboard", "Dashboard", "fa-home", "", "", "", "",
            [M("Dashboard", "fa-home", [], R(null, "Dashboard", "Index"), ["home", "overview"])],
            SingleLink: true,
            ShowOnDashboard: false),

        new("User Management", "User Management", "fa-users",
            "Manage users, roles and permissions",
            "text-blue-600", "bg-blue-100", "group-hover:bg-blue-200",
            [
                M("All Users", "fa-list", ["users.view"], R(null, "User", "Index"), ["users", "accounts", "user list"]),
                M("Create New User", "fa-user-plus", ["users.create"], R(null, "User", "Create"), ["new user", "add user"]),
                M("Reset Password", "fa-key", ["users.edit"], R(null, "User", "ResetPassword"), ["password", "reset"]),
                M("Roles", "fa-user-shield", ["roles.view"], R(null, "Role", "Index"), ["role"], [R(null, "Role", null)]),
                M("Permissions", "fa-lock", ["permissions.manage"], R("Admin", "RolePermissionManager", "Index"), ["permission", "access"],
                    [R("Admin", "RolePermissionManager", "Index"), R("Admin", "ManagePermissions", "Index")]),
            ],
            Landing: R(null, "Section", "UserManagement"),
            SectionActiveOns: [R(null, "User", null), R(null, "Role", null), R("Admin", "RolePermissionManager", null), R("Admin", "ManagePermissions", null), R(null, null, null, "/Account/ForgotPassword")]),

        new("Academic Setup", "Academic Setup", "fa-graduation-cap",
            "Configure faculties, programs, semesters and academic years",
            "text-green-600", "bg-green-100", "group-hover:bg-green-200",
            [
                M("Faculties", "fa-building", ["faculties.view"], R("Core", "Faculty", "Index"), ["faculty", "department"]),
                M("Programs / Streams", "fa-stream", ["programs.view"], R("Core", "Programs", "Index"), ["program", "stream", "course"]),
                M("Semesters", "fa-layer-group", ["semesters.view"], R("Core", "Semesters", "Index"), ["semester", "term"]),
                M("Academic Years", "fa-calendar-alt", ["academicyears.view"], R("Core", "AcademicYears", "Index"), ["academic year", "year", "session"]),
            ],
            Landing: R(null, "Section", "AcademicSetup"),
            SectionActiveOns: [R("Core", "Faculty", null), R("Core", "Programs", null), R("Core", "Semesters", null), R("Core", "AcademicYears", null)]),

        new("Subjects", "Subjects", "fa-book",
            "Manage subject catalogs, types, offerings and curriculum",
            "text-teal-600", "bg-teal-100", "group-hover:bg-teal-200",
            [
                M("Subject Catalogs", "fa-book", ["subjects.view"], R("Subjects", "SubjectCatalogs", "Index"), ["subject", "catalog", "syllabus", "curriculum"]),
                M("Subject Types", "fa-tags", ["subjecttypes.view"], R("Subjects", "SubjectTypes", "Index"), ["subject type", "type"]),
                M("Subject Offerings", "fa-layer-group", ["subjectofferings.view"], R("Subjects", "SubjectOfferings", "Index"), ["subject offering", "offering"]),
                M("Curriculum Versions", "fa-code-branch", ["curriculumversions.view"], R("Subjects", "CurriculumVersions", "Index"), ["curriculum", "version"]),
            ],
            Landing: R(null, "Section", "Subjects"),
            SectionActiveOns: [R("Subjects", "SubjectCatalogs", null), R("Subjects", "SubjectTypes", null), R("Subjects", "SubjectOfferings", null), R("Subjects", "CurriculumVersions", null)]),

        new("Colleges", "Colleges", "fa-school",
            "Manage colleges and their programs",
            "text-purple-600", "bg-purple-100", "group-hover:bg-purple-200",
            [
                M("All Colleges", "fa-school", ["colleges.view"], R("Colleges", "Colleges", "Index"), ["college", "institution"]),
                M("College Types", "fa-tag", ["collegetypes.view"], R("Colleges", "CollegeTypes", "Index"), ["college type"]),
                M("College Programs", "fa-diagram-project", ["collegeprograms.view"], R("Colleges", "CollegePrograms", "Index"), ["college program"]),
            ],
            Landing: R(null, "Section", "Colleges"),
            SectionActiveOns: [R("Colleges", "Colleges", null), R("Colleges", "CollegeTypes", null), R("Colleges", "CollegePrograms", null)]),

        new("Registration", "Registration", "fa-address-card",
            "Manage student registrations and admissions",
            "text-teal-600", "bg-teal-100", "group-hover:bg-teal-200",
            [
                M("Student Registrations", "fa-user-plus", ["students.view"], R("Students", "StudentRegistrations", "Index"), ["student", "registration", "enroll"]),
                M("Student Categories", "fa-layer-group", ["studentcategories.view"], R(null, "StudentCategories", "Index"), ["category"]),
                M("Student Admissions", "fa-user-graduate", ["studentadmissions.view"], R("Students", "StudentAdmissions", "Index"), ["admission", "admit"]),
                M("Semester Enrollments", "fa-user-check", ["studentadmissions.view"], R("Students", "SemesterEnrollments", "Index"), ["enrollment", "semester"]),
            ],
            Landing: R(null, "Section", "Registration"),
            SectionActiveOns: [R("Students", "StudentRegistrations", null), R(null, "StudentCategories", null), R("Students", "StudentAdmissions", null), R("Students", "SemesterEnrollments", null)]),

        new("Examination", "Examination", "fa-file-alt",
            "Manage exam schedules, types, registrations and more",
            "text-red-600", "bg-red-100", "group-hover:bg-red-200",
            [
                M("Exam Schedules", "fa-calendar-alt", ["examschedules.view"], R("Exams", "ExamSchedules", "Index"), ["schedule", "timetable", "routine"]),
                M("Schedule Approvals", "fa-check-double", ["examapproval.view"], R("Exams", "CollegeAdminApprovals", "Index"), ["approval", "approve", "schedule"]),
                M("Exam Types", "fa-list-alt", ["examtypes.view"], R("Exams", "ExamTypes", "Index"), ["exam type"]),
                M("Entrance Schedule", "fa-calendar-plus", ["examschedules.view"], R("Exams", "Entrance", "ManageSchedule"), ["entrance", "schedule"],
                    [R("Exams", "Entrance", "ManageSchedule"), R("Exams", "Entrance", "CreateSchedule"), R("Exams", "Entrance", "EditSchedule"), R("Exams", "Entrance", "ScheduleDetails")]),
                M("Exam Registrations", "fa-clipboard-list", ["examregistrations.view"], R("Exams", "ExamRegistrations", "Index"), ["exam registration", "form"]),
                M("Student Exam Forms", "fa-user-check", ["examregistrations.view"], R("Exams", "ExamRegistrations", "StudentForms"), ["student form", "exam form"]),
                M("Admit Cards", "fa-ticket-alt", ["admitcards.view"], R("Exams", "AdmitCards", "Index"), ["admit", "card", "hall ticket"]),
            ],
            Landing: R(null, "Section", "Examination"),
            SectionActiveOns: [R("Exams", "Entrance", null), R("Exams", "ExamSchedules", null), R("Exams", "ExamTypes", null), R("Exams", "ExamRegistrations", null), R("Exams", "AdmitCards", null), R("Exams", "CollegeAdminApprovals", null)]),

        new("Exam Centers", "Exam Centers", "fa-map-marker-alt",
            "Manage examination centers and distribution",
            "text-cyan-600", "bg-cyan-100", "group-hover:bg-cyan-200",
            [
                M("Exam Centers", "fa-map-marker-alt", ["examcenters.view"], R("Exams", "ExamCenters", "Index"), ["center", "centre"]),
                M("Center Distribution", "fa-people-arrows", ["examcenters.view"], R("Exams", "ExamCenterDistribution", "Index"), ["distribution", "center"]),
            ],
            Landing: R(null, "Section", "ExamCenters"),
            SectionActiveOns: [R("Exams", "ExamCenters", null), R("Exams", "ExamCenterDistribution", null)]),

        new("Grading & Marks", "Grading & Marks", "fa-percentage",
            "Manage grading schemes and marks entry",
            "text-yellow-600", "bg-yellow-100", "group-hover:bg-yellow-200",
            [
                M("Grading Schemes", "fa-percentage", ["gradingschemes.view"], R("Exams", "GradingSchemes", "Index"), ["grade", "grading", "scheme"]),
                M("Marks Entry", "fa-table", ["marksentry.view"], R("Exams", "CollegeAdminMarks", "Dashboard"), ["marks", "entry", "scores"]),
            ],
            Landing: R(null, "Section", "GradingAndMarks"),
            SectionActiveOns: [R("Exams", "GradingSchemes", null), R("Exams", "CollegeAdminMarks", null)]),

        new("Results", "Results", "fa-poll",
            "View and manage examination results",
            "text-yellow-600", "bg-yellow-100", "group-hover:bg-yellow-200",
            [
                M("Subject Results", "fa-check-double", ["examsubjectresults.view"], R("Exams", "ExamSubjectResults", "Index"), ["subject result", "result"]),
                M("Result Records", "fa-clipboard-check", ["resultrecords.view"], R("Exams", "ResultRecords", "Index"), ["result", "record"]),
                M("Retotaling", "fa-redo-alt", ["retotaling.view"], R("Exams", "RetotalRequests", "Index"), ["retotal", "re-evaluation", "recheck"]),
            ],
            Landing: R(null, "Section", "Results"),
            SectionActiveOns: [R("Exams", "ExamSubjectResults", null), R("Exams", "ResultRecords", null), R("Exams", "RetotalRequests", null)]),

        new("Payments", "Payments", "fa-money-bill-wave",
            "Manage banks, payment types and bill titles",
            "text-orange-600", "bg-orange-100", "group-hover:bg-orange-200",
            [
                M("Banks", "fa-university", ["banks.view"], R("Payments", "Banks", "Index"), ["bank"]),
                M("Payment Types", "fa-credit-card", ["paymenttypes.view"], R("Payments", "PaymentTypes", "Index"), ["payment type", "fee type"]),
                M("Bill Titles", "fa-file-invoice-dollar", ["billtitles.view"], R("Payments", "BillTitles", "Index"), ["bill", "title", "fee"]),
            ],
            Landing: R(null, "Section", "Payments"),
            SectionActiveOns: [R("Payments", "Banks", null), R("Payments", "PaymentTypes", null), R("Payments", "BillTitles", null)]),

        new("Location", "Location", "fa-map-marker-alt",
            "Manage provinces, districts and local levels",
            "text-cyan-600", "bg-cyan-100", "group-hover:bg-cyan-200",
            [
                M("Provinces", "fa-map-marked-alt", ["provinces.view"], R("Location", "Provinces", "Index"), ["province"]),
                M("Districts", "fa-map-pin", ["districts.view"], R("Location", "Districts", "Index"), ["district"]),
                M("Local Levels", "fa-city", ["locallevels.view"], R("Location", "LocalLevels", "Index"), ["local level", "municipality", "rural municipality", "ward"]),
            ],
            Landing: R(null, "Section", "Location"),
            SectionActiveOns: [R("Location", "Provinces", null), R("Location", "Districts", null), R("Location", "LocalLevels", null)]),

        new("Student Portal", "Student Portal", "fa-user-graduate",
            "Student portal - profile, exams, marksheet and payments",
            "text-pink-600", "bg-pink-100", "group-hover:bg-pink-200",
            [
                M("Exam Forms", "fa-file-invoice", ["student.portal.examforms"], R("Students", "StudentDashboard", "ExamForms"), ["exam form", "form"]),
                M("Marksheet", "fa-file-alt", ["student.portal.marksheet"], R("Students", "StudentDashboard", "Marksheet"), ["marksheet", "mark sheet", "result"]),
                M("Admit Cards", "fa-ticket-alt", ["admitcards.view"], R("Students", "StudentDashboard", "AdmitCards"), ["admit card", "hall ticket"]),
                M("Payment History", "fa-credit-card", ["student.portal.payment"], R("Students", "StudentDashboard", "PaymentHistory"), ["payment", "history", "receipt"]),
                M("Re-evaluation", "fa-redo-alt", ["retotaling.view"], R("Students", "StudentDashboard", "RetotalRequests"), ["retotal", "re-evaluation", "recheck"]),
            ],
            Landing: R(null, "Section", "StudentPortal"),
            RoleGate: Role.Student,
            SectionActiveOns: [R("Students", "StudentDashboard", null)]),

        new("System Config", "System Config", "fa-cog",
            "Configure tenants, notices, audit and backup",
            "text-indigo-600", "bg-indigo-100", "group-hover:bg-indigo-200",
            [
                M("Tenants", "fa-globe", ["tenants.view"], R(null, "Tenants", "Index"), ["tenant", "organization", "office"]),
                M("Notices", "fa-bullhorn", ["notices.view"], R("Core", "Notices", "Index"), ["notice", "announcement"]),
                M("Audit Log", "fa-history", ["auditlog.view"], R("Core", "AuditLog", "Index"), ["audit", "log", "activity"],
                    [R("Core", "AuditLog", "Index"), R("Core", "AuditLog", "Details")]),
                M("Backup & Restore", "fa-database", ["backuprestore.manage"], R("Core", "BackupRestore", "Index"), ["backup", "restore"]),
            ],
            Landing: R(null, "Section", "SystemConfig"),
            SectionActiveOns: [R(null, "Tenants", null), R("Core", "Notices", null), R("Core", "AuditLog", null), R("Core", "BackupRestore", null)]),

        new("Email & SMS", "Email & SMS", "fa-envelope",
            "Configure email and SMS delivery services",
            "text-violet-600", "bg-violet-100", "group-hover:bg-violet-200",
            [
                M("SMTP Configuration", "fa-envelope", ["smtp.view"], R("Core", "SmtpConfigurations", "Index"), ["smtp", "email", "mail", "server"]),
                M("Send Test Email", "fa-paper-plane", ["smtp.view"], R("Core", "TestEmail", "Index"), ["test email", "send email"]),
                M("SMS Configuration", "fa-sms", ["sms.view"], R("Core", "SmsConfigurations", "Index"), ["sms", "message", "text"]),
                M("Test GumpNow SMS", "fa-comment-dots", ["sms.view"], R("Core", "TestGumpNowSms", "Index"), ["gumpnow", "sms", "test"]),
                M("GumpNow Email Config", "fa-envelope-open-text", ["gumpnowemail.view"], R("Core", "GumpNowEmailConfigurations", "Index"), ["gumpnow", "email", "config"]),
                M("Test GumpNow Email", "fa-paper-plane", ["gumpnowemail.view"], R("Core", "TestGumpNowEmail", "Index"), ["gumpnow", "email", "test"]),
            ],
            Landing: R(null, "Section", "EmailAndSms"),
            SectionActiveOns: [R("Core", "SmtpConfigurations", null), R("Core", "TestEmail", null), R("Core", "SmsConfigurations", null), R("Core", "TestGumpNowSms", null), R("Core", "GumpNowEmailConfigurations", null), R("Core", "TestGumpNowEmail", null)]),

        new("Payment Gateways", "Payment Gateways", "fa-credit-card",
            "Configure payment gateway integrations",
            "text-rose-600", "bg-rose-100", "group-hover:bg-rose-200",
            [
                M("eSewa Configuration", "fa-credit-card", ["esewa.view"], R("Core", "ESewaConfigurations", "Index"), ["esewa", "payment gateway"]),
                M("Khalti Configuration", "fa-credit-card", ["khalti.view"], R("Core", "KhaltiConfigurations", "Index"), ["khalti", "payment gateway"]),
                M("ConnectIPS Configuration", "fa-credit-card", ["connectips.view"], R("Core", "ConnectIPSConfigurations", "Index"), ["connectips", "payment gateway"]),
            ],
            Landing: R(null, "Section", "PaymentGateways"),
            SectionActiveOns: [R("Core", "ESewaConfigurations", null), R("Core", "KhaltiConfigurations", null), R("Core", "ConnectIPSConfigurations", null)]),

        new("Core Area", "Core Area", "fa-cubes",
            "",
            "", "", "",
            [
                M("Level", "fa-layer-group", ["levels.view"], R("Core", "Levels", "Index"), ["level"]),
                M("Gender", "fa-venus-mars", ["genders.view"], R("Core", "Genders", "Index"), ["gender"]),
                M("Ethnicity", "fa-users", ["ethnicities.view"], R("Core", "Ethnicities", "Index"), ["ethnicity", "ethnic"]),
                M("Board", "fa-landmark", ["boards.view"], R("Core", "Boards", "Index"), ["board"]),
                M("Country", "fa-globe", ["countries.view"], R("Core", "Countries", "Index"), ["country", "nation"]),
            ],
            SuperAdminOnly: true,
            ShowOnDashboard: false,
            SectionActiveOns: [R("Core", "Levels", null), R("Core", "Genders", null), R("Core", "Ethnicities", null), R("Core", "Boards", null), R("Core", "Countries", null)]),

        new("Reports", "Reports", "fa-chart-line",
            "Generate and view various reports",
            "text-rose-600", "bg-rose-100", "group-hover:bg-rose-200",
            [
                M("College Payments", "fa-money-check-alt", ["reports.collegepayments"], R("Reports", "Reports", "CollegePayments"), ["college payment", "payment report"]),
                M("Subject Count Report", "fa-chart-bar", ["reports.subjectcount"], R("Reports", "Reports", "SubjectCount"), ["subject count"]),
                M("Exam Triplicate", "fa-copy", ["reports.examtriplicate"], R("Reports", "Reports", "ExamTriplicate"), ["exam triplicate", "triplicate"]),
                M("Summary Report", "fa-chart-pie", ["reports.summary"], R("Reports", "Reports", "Summary"), ["summary"]),
                M("Tabulation Triplicate", "fa-table", ["reports.tabulationtriplicate"], R("Reports", "Reports", "TabulationTriplicate"), ["tabulation"]),
                M("Program Wise Student", "fa-user-friends", ["reports.programwisestudent"], R("Reports", "Reports", "ProgramWiseStudent"), ["program wise", "student report"]),
                M("Attendance Sheet", "fa-clipboard-check", ["reports.attendanceheet"], R("Reports", "Reports", "AttendanceSheet"), ["attendance", "sheet"]),
                M("Marks Foil", "fa-poll", ["reports.marksfoil"], R("Reports", "Reports", "MarksFoil"), ["marks foil", "foil"]),
                M("Bank Voucher List", "fa-file-invoice-dollar", ["reports.bankvoucherlist"], R("Reports", "Reports", "BankVoucherList"), ["bank voucher", "voucher"]),
            ],
            Landing: R("Reports", "Reports", "Index"),
            SectionActiveOns: [R("Reports", "Reports", null)]),
    ];
}
