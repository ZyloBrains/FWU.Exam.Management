namespace FWU.Exam.Management.Domain.Entities.Permissions;

public static class Permissions
{
    // Groups
    public const string GroupDashboard = "dashboard";
    public const string GroupFaculties = "faculties";
    public const string GroupColleges = "colleges";
    public const string GroupPrograms = "programs";
    public const string GroupSemesters = "semesters";
    public const string GroupAcademicYears = "academicyears";
    public const string GroupSubjects = "subjects";
    public const string GroupSubjectTypes = "subjecttypes";
    public const string GroupSubjectOfferings = "subjectofferings";
    public const string GroupCurriculumVersions = "curriculumversions";
    public const string GroupExamSchedules = "examschedules";
    public const string GroupExamTypes = "examtypes";
    public const string GroupEntrance = "entrance";
    public const string GroupStudents = "students";
    public const string GroupStudentAdmissions = "studentadmissions";
    public const string GroupUsers = "users";
    public const string GroupRoles = "roles";
    public const string GroupPermissions = "permissions";
    public const string GroupBanks = "banks";
    public const string GroupPaymentTypes = "paymenttypes";
    public const string GroupBillTitles = "billtitles";
    public const string GroupProvinces = "provinces";
    public const string GroupDistricts = "districts";
    public const string GroupLocalLevels = "locallevels";
    public const string GroupCollegeTypes = "collegetypes";
    public const string GroupCollegePrograms = "collegeprograms";
    public const string GroupNotices = "notices";
    public const string GroupBoards = "boards";
    public const string GroupLevels = "levels";
    public const string GroupGenders = "genders";
    public const string GroupEthnicities = "ethnicities";
    public const string GroupCountries = "countries";
    public const string GroupSmtp = "smtp";
    public const string GroupSms = "sms";
    public const string GroupGumpNowEmail = "gumpnowemail";
    public const string GroupESewa = "esewa";
    public const string GroupKhalti = "khalti";
    public const string GroupConnectIPS = "connectips";
    public const string GroupTenants = "tenants";
    public const string GroupStudentCategories = "studentcategories";
    public const string GroupGradingSchemes = "gradingschemes";
    public const string GroupExamRegistration = "examregistration";
    public const string GroupExamSubjectResults = "examsubjectresults";
    public const string GroupResultRecords = "resultrecords";
    public const string GroupExamCenters = "examcenters";
    public const string GroupAdmitCards = "admitcards";
    public const string GroupRetotaling = "retotaling";
    public const string GroupMarksEntry = "marksentry";
    public const string GroupReports = "reports";
    public const string GroupAuditLog = "auditlog";
    public const string GroupBackupRestore = "backuprestore";

    // Helper to build permission name
    public static string N(string group, string action) => $"{group}.{action}";

    // Actions
    public const string View = "view";
    public const string Create = "create";
    public const string Edit = "edit";
    public const string Delete = "delete";
    public const string Manage = "manage";
    public const string Approve = "approve";
    public const string Reject = "reject";
    public const string Export = "export";
    public const string AssignRoles = "assign.roles";

    // Dashboard
    public const string DashboardView = "dashboard.view";

    // Faculties
    public const string FacultiesView = "faculties.view";
    public const string FacultiesCreate = "faculties.create";
    public const string FacultiesEdit = "faculties.edit";
    public const string FacultiesDelete = "faculties.delete";

    // Colleges
    public const string CollegesView = "colleges.view";
    public const string CollegesCreate = "colleges.create";
    public const string CollegesEdit = "colleges.edit";
    public const string CollegesDelete = "colleges.delete";

    // Programs
    public const string ProgramsView = "programs.view";
    public const string ProgramsCreate = "programs.create";
    public const string ProgramsEdit = "programs.edit";
    public const string ProgramsDelete = "programs.delete";

    // Semesters
    public const string SemestersView = "semesters.view";
    public const string SemestersCreate = "semesters.create";
    public const string SemestersEdit = "semesters.edit";
    public const string SemestersDelete = "semesters.delete";

    // Academic Years
    public const string AcademicYearsView = "academicyears.view";
    public const string AcademicYearsCreate = "academicyears.create";
    public const string AcademicYearsEdit = "academicyears.edit";
    public const string AcademicYearsDelete = "academicyears.delete";

    // Subjects
    public const string SubjectsView = "subjects.view";
    public const string SubjectsCreate = "subjects.create";
    public const string SubjectsEdit = "subjects.edit";
    public const string SubjectsDelete = "subjects.delete";

    // Subject Types
    public const string SubjectTypesView = "subjecttypes.view";
    public const string SubjectTypesCreate = "subjecttypes.create";
    public const string SubjectTypesEdit = "subjecttypes.edit";
    public const string SubjectTypesDelete = "subjecttypes.delete";

    // Subject Offerings
    public const string SubjectOfferingsView = "subjectofferings.view";
    public const string SubjectOfferingsCreate = "subjectofferings.create";
    public const string SubjectOfferingsEdit = "subjectofferings.edit";
    public const string SubjectOfferingsDelete = "subjectofferings.delete";

    // Curriculum Versions
    public const string CurriculumVersionsView = "curriculumversions.view";
    public const string CurriculumVersionsCreate = "curriculumversions.create";
    public const string CurriculumVersionsEdit = "curriculumversions.edit";
    public const string CurriculumVersionsDelete = "curriculumversions.delete";

    // Exam Schedules
    public const string ExamSchedulesView = "examschedules.view";
    public const string ExamSchedulesCreate = "examschedules.create";
    public const string ExamSchedulesEdit = "examschedules.edit";
    public const string ExamSchedulesDelete = "examschedules.delete";

    // Exam Types
    public const string ExamTypesView = "examtypes.view";
    public const string ExamTypesCreate = "examtypes.create";
    public const string ExamTypesEdit = "examtypes.edit";
    public const string ExamTypesDelete = "examtypes.delete";

    // Entrance
    public const string EntranceView = "entrance.view";
    public const string EntranceApprove = "entrance.approve";
    public const string EntranceReject = "entrance.reject";
    public const string EntranceExport = "entrance.export";

    // Students / Registrations
    public const string StudentsView = "students.view";
    public const string StudentsCreate = "students.create";
    public const string StudentsEdit = "students.edit";
    public const string StudentsDelete = "students.delete";

    // Student Admissions
    public const string StudentAdmissionsView = "studentadmissions.view";
    public const string StudentAdmissionsCreate = "studentadmissions.create";
    public const string StudentAdmissionsEdit = "studentadmissions.edit";
    public const string StudentAdmissionsDelete = "studentadmissions.delete";

    // Users
    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersEdit = "users.edit";
    public const string UsersDelete = "users.delete";
    public const string UsersAssignRoles = "users.assign.roles";

    // Roles
    public const string RolesView = "roles.view";
    public const string RolesCreate = "roles.create";
    public const string RolesEdit = "roles.edit";
    public const string RolesDelete = "roles.delete";

    // Permissions management
    public const string PermissionsManage = "permissions.manage";

    // Banks
    public const string BanksView = "banks.view";
    public const string BanksCreate = "banks.create";
    public const string BanksEdit = "banks.edit";
    public const string BanksDelete = "banks.delete";

    // Payment Types
    public const string PaymentTypesView = "paymenttypes.view";
    public const string PaymentTypesCreate = "paymenttypes.create";
    public const string PaymentTypesEdit = "paymenttypes.edit";
    public const string PaymentTypesDelete = "paymenttypes.delete";

    // Bill Titles
    public const string BillTitlesView = "billtitles.view";
    public const string BillTitlesCreate = "billtitles.create";
    public const string BillTitlesEdit = "billtitles.edit";
    public const string BillTitlesDelete = "billtitles.delete";

    // Provinces
    public const string ProvincesView = "provinces.view";
    public const string ProvincesCreate = "provinces.create";
    public const string ProvincesEdit = "provinces.edit";
    public const string ProvincesDelete = "provinces.delete";

    // Districts
    public const string DistrictsView = "districts.view";
    public const string DistrictsCreate = "districts.create";
    public const string DistrictsEdit = "districts.edit";
    public const string DistrictsDelete = "districts.delete";

    // Local Levels
    public const string LocalLevelsView = "locallevels.view";
    public const string LocalLevelsCreate = "locallevels.create";
    public const string LocalLevelsEdit = "locallevels.edit";
    public const string LocalLevelsDelete = "locallevels.delete";

    // College Types
    public const string CollegeTypesView = "collegetypes.view";
    public const string CollegeTypesCreate = "collegetypes.create";
    public const string CollegeTypesEdit = "collegetypes.edit";
    public const string CollegeTypesDelete = "collegetypes.delete";

    // College Programs
    public const string CollegeProgramsView = "collegeprograms.view";
    public const string CollegeProgramsCreate = "collegeprograms.create";
    public const string CollegeProgramsEdit = "collegeprograms.edit";
    public const string CollegeProgramsDelete = "collegeprograms.delete";

    // Notices
    public const string NoticesView = "notices.view";
    public const string NoticesCreate = "notices.create";
    public const string NoticesEdit = "notices.edit";
    public const string NoticesDelete = "notices.delete";

    // Boards
    public const string BoardsView = "boards.view";
    public const string BoardsCreate = "boards.create";
    public const string BoardsEdit = "boards.edit";
    public const string BoardsDelete = "boards.delete";

    // Levels
    public const string LevelsView = "levels.view";
    public const string LevelsCreate = "levels.create";
    public const string LevelsEdit = "levels.edit";
    public const string LevelsDelete = "levels.delete";

    // Genders
    public const string GendersView = "genders.view";
    public const string GendersCreate = "genders.create";
    public const string GendersEdit = "genders.edit";
    public const string GendersDelete = "genders.delete";

    // Ethnicities
    public const string EthnicitiesView = "ethnicities.view";
    public const string EthnicitiesCreate = "ethnicities.create";
    public const string EthnicitiesEdit = "ethnicities.edit";
    public const string EthnicitiesDelete = "ethnicities.delete";

    public const string CountriesView = "countries.view";
    public const string CountriesCreate = "countries.create";
    public const string CountriesEdit = "countries.edit";
    public const string CountriesDelete = "countries.delete";

    // SMTP
    public const string SmtpView = "smtp.view";
    public const string SmtpCreate = "smtp.create";
    public const string SmtpEdit = "smtp.edit";
    public const string SmtpDelete = "smtp.delete";

    // SMS
    public const string SmsView = "sms.view";
    public const string SmsCreate = "sms.create";
    public const string SmsEdit = "sms.edit";
    public const string SmsDelete = "sms.delete";

    // GumpNow Email
    public const string GumpNowEmailView = "gumpnowemail.view";
    public const string GumpNowEmailCreate = "gumpnowemail.create";
    public const string GumpNowEmailEdit = "gumpnowemail.edit";
    public const string GumpNowEmailDelete = "gumpnowemail.delete";

    // eSewa Config
    public const string ESewaView = "esewa.view";
    public const string ESewaCreate = "esewa.create";
    public const string ESewaEdit = "esewa.edit";
    public const string ESewaDelete = "esewa.delete";

    // Khalti Config
    public const string KhaltiView = "khalti.view";
    public const string KhaltiCreate = "khalti.create";
    public const string KhaltiEdit = "khalti.edit";
    public const string KhaltiDelete = "khalti.delete";

    // ConnectIPS Config
    public const string ConnectIPSView = "connectips.view";
    public const string ConnectIPSCreate = "connectips.create";
    public const string ConnectIPSEdit = "connectips.edit";
    public const string ConnectIPSDelete = "connectips.delete";

    // Tenants
    public const string TenantsView = "tenants.view";
    public const string TenantsCreate = "tenants.create";
    public const string TenantsEdit = "tenants.edit";
    public const string TenantsDelete = "tenants.delete";

    // Student Categories
    public const string StudentCategoriesView = "studentcategories.view";
    public const string StudentCategoriesCreate = "studentcategories.create";
    public const string StudentCategoriesEdit = "studentcategories.edit";
    public const string StudentCategoriesDelete = "studentcategories.delete";

    // Grading Schemes
    public const string GradingSchemesView = "gradingschemes.view";
    public const string GradingSchemesCreate = "gradingschemes.create";
    public const string GradingSchemesEdit = "gradingschemes.edit";
    public const string GradingSchemesDelete = "gradingschemes.delete";

    // Exam Registration
    public const string ExamRegistrationView = "examregistration.view";
    public const string ExamRegistrationCreate = "examregistration.create";
    public const string ExamRegistrationEdit = "examregistration.edit";
    public const string ExamRegistrationDelete = "examregistration.delete";
    public const string ExamRegistrationVerify = "examregistration.verify";
    public const string ExamRegistrationApprove = "examregistration.approve";

    // Exam Subject Results
    public const string ExamSubjectResultsView = "examsubjectresults.view";
    public const string ExamSubjectResultsCreate = "examsubjectresults.create";
    public const string ExamSubjectResultsEdit = "examsubjectresults.edit";
    public const string ExamSubjectResultsDelete = "examsubjectresults.delete";

    // Result Records (read-only)
    public const string ResultRecordsView = "resultrecords.view";

    // Exam Centers
    public const string ExamCentersView = "examcenters.view";
    public const string ExamCentersCreate = "examcenters.create";
    public const string ExamCentersEdit = "examcenters.edit";
    public const string ExamCentersDelete = "examcenters.delete";
    public const string ExamCentersGenerateRollNumbers = "examcenters.generaterollnumbers";

    // Admit Cards
    public const string AdmitCardsView = "admitcards.view";
    public const string AdmitCardsCreate = "admitcards.create";
    public const string AdmitCardsEdit = "admitcards.edit";
    public const string AdmitCardsDelete = "admitcards.delete";
    public const string AdmitCardsGenerate = "admitcards.generate";
    public const string AdmitCardsDownload = "admitcards.download";

    // Retotaling
    public const string RetotalingView = "retotaling.view";
    public const string RetotalingRequest = "retotaling.request";
    public const string RetotalingApprove = "retotaling.approve";
    public const string RetotalingReject = "retotaling.reject";
    public const string RetotalingReview = "retotaling.review";

    // Audit Log
    public const string AuditLogView = "auditlog.view";

    // Backup & Restore
    public const string BackupRestoreManage = "backuprestore.manage";

    // Marks Entry (College Admin)
    public const string MarksEntryView = "marksentry.view";
    public const string MarksEntrySubmit = "marksentry.submit";
    public const string MarksEntryImport = "marksentry.import";
    public const string MarksEntryExport = "marksentry.export";

    // Reports
    public const string ReportsCollegePayments = "reports.collegepayments";
    public const string ReportsSubjectCount = "reports.subjectcount";
    public const string ReportsExamTriplicate = "reports.examtriplicate";
    public const string ReportsSummary = "reports.summary";
    public const string ReportsTabulationTriplicate = "reports.tabulationtriplicate";
    public const string ReportsProgramWiseStudent = "reports.programwisestudent";
    public const string ReportsAttendanceSheet = "reports.attendanceheet";
    public const string ReportsMarksFoil = "reports.marksfoil";
    public const string ReportsBankVoucherList = "reports.bankvoucherlist";

    // Student Portal (for student-facing pages)
    public const string StudentPortalProfile = "student.portal.profile";
    public const string StudentPortalExamForms = "student.portal.examforms";
    public const string StudentPortalMarksheet = "student.portal.marksheet";
    public const string StudentPortalPayment = "student.portal.payment";

    // All permissions list for seeding
    public static readonly IReadOnlyList<(string Name, string DisplayName, string Group, string Description)> All =
    [
        (DashboardView, "Dashboard View", GroupDashboard, "Access the dashboard"),

        (FacultiesView, "View Faculties", GroupFaculties, "View faculty list"),
        (FacultiesCreate, "Create Faculties", GroupFaculties, "Create new faculties"),
        (FacultiesEdit, "Edit Faculties", GroupFaculties, "Edit existing faculties"),
        (FacultiesDelete, "Delete Faculties", GroupFaculties, "Delete faculties"),

        (CollegesView, "View Colleges", GroupColleges, "View college list"),
        (CollegesCreate, "Create Colleges", GroupColleges, "Create new colleges"),
        (CollegesEdit, "Edit Colleges", GroupColleges, "Edit existing colleges"),
        (CollegesDelete, "Delete Colleges", GroupColleges, "Delete colleges"),

        (ProgramsView, "View Programs", GroupPrograms, "View program list"),
        (ProgramsCreate, "Create Programs", GroupPrograms, "Create new programs"),
        (ProgramsEdit, "Edit Programs", GroupPrograms, "Edit existing programs"),
        (ProgramsDelete, "Delete Programs", GroupPrograms, "Delete programs"),

        (SemestersView, "View Semesters", GroupSemesters, "View semester list"),
        (SemestersCreate, "Create Semesters", GroupSemesters, "Create new semesters"),
        (SemestersEdit, "Edit Semesters", GroupSemesters, "Edit existing semesters"),
        (SemestersDelete, "Delete Semesters", GroupSemesters, "Delete semesters"),

        (AcademicYearsView, "View Academic Years", GroupAcademicYears, "View academic year list"),
        (AcademicYearsCreate, "Create Academic Years", GroupAcademicYears, "Create new academic years"),
        (AcademicYearsEdit, "Edit Academic Years", GroupAcademicYears, "Edit existing academic years"),
        (AcademicYearsDelete, "Delete Academic Years", GroupAcademicYears, "Delete academic years"),

        (SubjectsView, "View Subjects", GroupSubjects, "View subject catalog"),
        (SubjectsCreate, "Create Subjects", GroupSubjects, "Create new subjects"),
        (SubjectsEdit, "Edit Subjects", GroupSubjects, "Edit existing subjects"),
        (SubjectsDelete, "Delete Subjects", GroupSubjects, "Delete subjects"),

        (SubjectTypesView, "View Subject Types", GroupSubjectTypes, "View subject type list"),
        (SubjectTypesCreate, "Create Subject Types", GroupSubjectTypes, "Create new subject types"),
        (SubjectTypesEdit, "Edit Subject Types", GroupSubjectTypes, "Edit existing subject types"),
        (SubjectTypesDelete, "Delete Subject Types", GroupSubjectTypes, "Delete subject types"),

        (SubjectOfferingsView, "View Subject Offerings", GroupSubjectOfferings, "View subject offering list"),
        (SubjectOfferingsCreate, "Create Subject Offerings", GroupSubjectOfferings, "Create new subject offerings"),
        (SubjectOfferingsEdit, "Edit Subject Offerings", GroupSubjectOfferings, "Edit existing subject offerings"),
        (SubjectOfferingsDelete, "Delete Subject Offerings", GroupSubjectOfferings, "Delete subject offerings"),

        (CurriculumVersionsView, "View Curriculum Versions", GroupCurriculumVersions, "View curriculum version list"),
        (CurriculumVersionsCreate, "Create Curriculum Versions", GroupCurriculumVersions, "Create new curriculum versions"),
        (CurriculumVersionsEdit, "Edit Curriculum Versions", GroupCurriculumVersions, "Edit existing curriculum versions"),
        (CurriculumVersionsDelete, "Delete Curriculum Versions", GroupCurriculumVersions, "Delete curriculum versions"),

        (ExamSchedulesView, "View Exam Schedules", GroupExamSchedules, "View exam schedule list"),
        (ExamSchedulesCreate, "Create Exam Schedules", GroupExamSchedules, "Create new exam schedules"),
        (ExamSchedulesEdit, "Edit Exam Schedules", GroupExamSchedules, "Edit existing exam schedules"),
        (ExamSchedulesDelete, "Delete Exam Schedules", GroupExamSchedules, "Delete exam schedules"),

        (ExamTypesView, "View Exam Types", GroupExamTypes, "View exam type list"),
        (ExamTypesCreate, "Create Exam Types", GroupExamTypes, "Create new exam types"),
        (ExamTypesEdit, "Edit Exam Types", GroupExamTypes, "Edit existing exam types"),
        (ExamTypesDelete, "Delete Exam Types", GroupExamTypes, "Delete exam types"),

        (EntranceView, "View Entrance Applications", GroupEntrance, "View entrance applications"),
        (EntranceApprove, "Approve Entrance", GroupEntrance, "Approve entrance applications"),
        (EntranceReject, "Reject Entrance", GroupEntrance, "Reject entrance applications"),
        (EntranceExport, "Export Entrance", GroupEntrance, "Export entrance applications to Excel"),

        (StudentsView, "View Students", GroupStudents, "View student registrations"),
        (StudentsCreate, "Create Students", GroupStudents, "Create new student registrations"),
        (StudentsEdit, "Edit Students", GroupStudents, "Edit existing student registrations"),
        (StudentsDelete, "Delete Students", GroupStudents, "Delete student registrations"),

        (StudentAdmissionsView, "View Student Admissions", GroupStudentAdmissions, "View student admissions"),
        (StudentAdmissionsCreate, "Create Student Admissions", GroupStudentAdmissions, "Create new student admissions"),
        (StudentAdmissionsEdit, "Edit Student Admissions", GroupStudentAdmissions, "Edit existing student admissions"),
        (StudentAdmissionsDelete, "Delete Student Admissions", GroupStudentAdmissions, "Delete student admissions"),

        (UsersView, "View Users", GroupUsers, "View user list"),
        (UsersCreate, "Create Users", GroupUsers, "Create new users"),
        (UsersEdit, "Edit Users", GroupUsers, "Edit existing users"),
        (UsersDelete, "Delete Users", GroupUsers, "Delete users"),
        (UsersAssignRoles, "Assign User Roles", GroupUsers, "Assign roles to users"),

        (RolesView, "View Roles", GroupRoles, "View role list"),
        (RolesCreate, "Create Roles", GroupRoles, "Create new roles"),
        (RolesEdit, "Edit Roles", GroupRoles, "Edit existing roles"),
        (RolesDelete, "Delete Roles", GroupRoles, "Delete roles"),

        (PermissionsManage, "Manage Permissions", GroupPermissions, "Assign permissions to roles"),

        (BanksView, "View Banks", GroupBanks, "View bank list"),
        (BanksCreate, "Create Banks", GroupBanks, "Create new banks"),
        (BanksEdit, "Edit Banks", GroupBanks, "Edit existing banks"),
        (BanksDelete, "Delete Banks", GroupBanks, "Delete banks"),

        (PaymentTypesView, "View Payment Types", GroupPaymentTypes, "View payment type list"),
        (PaymentTypesCreate, "Create Payment Types", GroupPaymentTypes, "Create new payment types"),
        (PaymentTypesEdit, "Edit Payment Types", GroupPaymentTypes, "Edit existing payment types"),
        (PaymentTypesDelete, "Delete Payment Types", GroupPaymentTypes, "Delete payment types"),

        (BillTitlesView, "View Bill Titles", GroupBillTitles, "View bill title list"),
        (BillTitlesCreate, "Create Bill Titles", GroupBillTitles, "Create new bill titles"),
        (BillTitlesEdit, "Edit Bill Titles", GroupBillTitles, "Edit existing bill titles"),
        (BillTitlesDelete, "Delete Bill Titles", GroupBillTitles, "Delete bill titles"),

        (ProvincesView, "View Provinces", GroupProvinces, "View province list"),
        (ProvincesCreate, "Create Provinces", GroupProvinces, "Create new provinces"),
        (ProvincesEdit, "Edit Provinces", GroupProvinces, "Edit existing provinces"),
        (ProvincesDelete, "Delete Provinces", GroupProvinces, "Delete provinces"),

        (DistrictsView, "View Districts", GroupDistricts, "View district list"),
        (DistrictsCreate, "Create Districts", GroupDistricts, "Create new districts"),
        (DistrictsEdit, "Edit Districts", GroupDistricts, "Edit existing districts"),
        (DistrictsDelete, "Delete Districts", GroupDistricts, "Delete districts"),

        (LocalLevelsView, "View Local Levels", GroupLocalLevels, "View local level list"),
        (LocalLevelsCreate, "Create Local Levels", GroupLocalLevels, "Create new local levels"),
        (LocalLevelsEdit, "Edit Local Levels", GroupLocalLevels, "Edit existing local levels"),
        (LocalLevelsDelete, "Delete Local Levels", GroupLocalLevels, "Delete local levels"),

        (CollegeTypesView, "View College Types", GroupCollegeTypes, "View college type list"),
        (CollegeTypesCreate, "Create College Types", GroupCollegeTypes, "Create new college types"),
        (CollegeTypesEdit, "Edit College Types", GroupCollegeTypes, "Edit existing college types"),
        (CollegeTypesDelete, "Delete College Types", GroupCollegeTypes, "Delete college types"),

        (CollegeProgramsView, "View College Programs", GroupCollegePrograms, "View college program list"),
        (CollegeProgramsCreate, "Create College Programs", GroupCollegePrograms, "Create new college programs"),
        (CollegeProgramsEdit, "Edit College Programs", GroupCollegePrograms, "Edit existing college programs"),
        (CollegeProgramsDelete, "Delete College Programs", GroupCollegePrograms, "Delete college programs"),

        (NoticesView, "View Notices", GroupNotices, "View notice list"),
        (NoticesCreate, "Create Notices", GroupNotices, "Create new notices"),
        (NoticesEdit, "Edit Notices", GroupNotices, "Edit existing notices"),
        (NoticesDelete, "Delete Notices", GroupNotices, "Delete notices"),

        (BoardsView, "View Boards", GroupBoards, "View board list"),
        (BoardsCreate, "Create Boards", GroupBoards, "Create new boards"),
        (BoardsEdit, "Edit Boards", GroupBoards, "Edit existing boards"),
        (BoardsDelete, "Delete Boards", GroupBoards, "Delete boards"),

        (LevelsView, "View Levels", GroupLevels, "View level list"),
        (LevelsCreate, "Create Levels", GroupLevels, "Create new levels"),
        (LevelsEdit, "Edit Levels", GroupLevels, "Edit existing levels"),
        (LevelsDelete, "Delete Levels", GroupLevels, "Delete levels"),

        (GendersView, "View Genders", GroupGenders, "View gender list"),
        (GendersCreate, "Create Genders", GroupGenders, "Create new genders"),
        (GendersEdit, "Edit Genders", GroupGenders, "Edit existing genders"),
        (GendersDelete, "Delete Genders", GroupGenders, "Delete genders"),

        (EthnicitiesView, "View Ethnicities", GroupEthnicities, "View ethnicity list"),
        (EthnicitiesCreate, "Create Ethnicities", GroupEthnicities, "Create new ethnicities"),
        (EthnicitiesEdit, "Edit Ethnicities", GroupEthnicities, "Edit existing ethnicities"),
        (EthnicitiesDelete, "Delete Ethnicities", GroupEthnicities, "Delete ethnicities"),

        (CountriesView, "View Countries", GroupCountries, "View country list"),
        (CountriesCreate, "Create Countries", GroupCountries, "Create new countries"),
        (CountriesEdit, "Edit Countries", GroupCountries, "Edit existing countries"),
        (CountriesDelete, "Delete Countries", GroupCountries, "Delete countries"),

        (SmtpView, "View SMTP Config", GroupSmtp, "View SMTP configuration"),
        (SmtpCreate, "Create SMTP Config", GroupSmtp, "Create new SMTP configuration"),
        (SmtpEdit, "Edit SMTP Config", GroupSmtp, "Edit SMTP configuration"),
        (SmtpDelete, "Delete SMTP Config", GroupSmtp, "Delete SMTP configuration"),

        (SmsView, "View SMS Config", GroupSms, "View SMS configuration"),
        (SmsCreate, "Create SMS Config", GroupSms, "Create new SMS configuration"),
        (SmsEdit, "Edit SMS Config", GroupSms, "Edit SMS configuration"),
        (SmsDelete, "Delete SMS Config", GroupSms, "Delete SMS configuration"),

        (GumpNowEmailView, "View GumpNow Email Config", GroupGumpNowEmail, "View GumpNow email configuration"),
        (GumpNowEmailCreate, "Create GumpNow Email Config", GroupGumpNowEmail, "Create new GumpNow email configuration"),
        (GumpNowEmailEdit, "Edit GumpNow Email Config", GroupGumpNowEmail, "Edit GumpNow email configuration"),
        (GumpNowEmailDelete, "Delete GumpNow Email Config", GroupGumpNowEmail, "Delete GumpNow email configuration"),

        (ESewaView, "View eSewa Config", GroupESewa, "View eSewa configuration"),
        (ESewaCreate, "Create eSewa Config", GroupESewa, "Create eSewa configuration"),
        (ESewaEdit, "Edit eSewa Config", GroupESewa, "Edit eSewa configuration"),
        (ESewaDelete, "Delete eSewa Config", GroupESewa, "Delete eSewa configuration"),

        (KhaltiView, "View Khalti Config", GroupKhalti, "View Khalti configuration"),
        (KhaltiCreate, "Create Khalti Config", GroupKhalti, "Create Khalti configuration"),
        (KhaltiEdit, "Edit Khalti Config", GroupKhalti, "Edit Khalti configuration"),
        (KhaltiDelete, "Delete Khalti Config", GroupKhalti, "Delete Khalti configuration"),

        (ConnectIPSView, "View ConnectIPS Config", GroupConnectIPS, "View ConnectIPS configuration"),
        (ConnectIPSCreate, "Create ConnectIPS Config", GroupConnectIPS, "Create ConnectIPS configuration"),
        (ConnectIPSEdit, "Edit ConnectIPS Config", GroupConnectIPS, "Edit ConnectIPS configuration"),
        (ConnectIPSDelete, "Delete ConnectIPS Config", GroupConnectIPS, "Delete ConnectIPS configuration"),

        (TenantsView, "View Tenants", GroupTenants, "View tenant list"),
        (TenantsCreate, "Create Tenants", GroupTenants, "Create new tenants"),
        (TenantsEdit, "Edit Tenants", GroupTenants, "Edit existing tenants"),
        (TenantsDelete, "Delete Tenants", GroupTenants, "Delete tenants"),

        (StudentCategoriesView, "View Student Categories", GroupStudentCategories, "View student category list"),
        (StudentCategoriesCreate, "Create Student Categories", GroupStudentCategories, "Create new student categories"),
        (StudentCategoriesEdit, "Edit Student Categories", GroupStudentCategories, "Edit existing student categories"),
        (StudentCategoriesDelete, "Delete Student Categories", GroupStudentCategories, "Delete student categories"),

        (GradingSchemesView, "View Grading Schemes", GroupGradingSchemes, "View grading scheme list"),
        (GradingSchemesCreate, "Create Grading Schemes", GroupGradingSchemes, "Create new grading schemes"),
        (GradingSchemesEdit, "Edit Grading Schemes", GroupGradingSchemes, "Edit existing grading schemes"),
        (GradingSchemesDelete, "Delete Grading Schemes", GroupGradingSchemes, "Delete grading schemes"),

        (ExamRegistrationView, "View Exam Registrations", GroupExamRegistration, "View exam registration list"),
        (ExamRegistrationCreate, "Create Exam Registrations", GroupExamRegistration, "Create new exam registrations"),
        (ExamRegistrationEdit, "Edit Exam Registrations", GroupExamRegistration, "Edit existing exam registrations"),
        (ExamRegistrationDelete, "Delete Exam Registrations", GroupExamRegistration, "Delete exam registrations"),
        (ExamRegistrationVerify, "Verify Exam Registrations", GroupExamRegistration, "Verify exam registrations at college level"),
        (ExamRegistrationApprove, "Approve Exam Registrations", GroupExamRegistration, "Approve exam registrations at admin level"),

        (ExamSubjectResultsView, "View Subject Results", GroupExamSubjectResults, "View subject-wise results"),
        (ExamSubjectResultsCreate, "Create Subject Results", GroupExamSubjectResults, "Enter subject-wise marks"),
        (ExamSubjectResultsEdit, "Edit Subject Results", GroupExamSubjectResults, "Edit subject-wise marks"),
        (ExamSubjectResultsDelete, "Delete Subject Results", GroupExamSubjectResults, "Delete subject results"),

        (ResultRecordsView, "View Result Records", GroupResultRecords, "View published result records"),

        (ExamCentersView, "View Exam Centers", GroupExamCenters, "View exam center list"),
        (ExamCentersCreate, "Create Exam Centers", GroupExamCenters, "Create new exam centers"),
        (ExamCentersEdit, "Edit Exam Centers", GroupExamCenters, "Edit existing exam centers"),
        (ExamCentersDelete, "Delete Exam Centers", GroupExamCenters, "Delete exam centers"),
        (ExamCentersGenerateRollNumbers, "Generate Roll Numbers", GroupExamCenters, "Generate exam roll numbers for registrations"),

        (AdmitCardsView, "View Admit Cards", GroupAdmitCards, "View admit card list"),
        (AdmitCardsCreate, "Create Admit Cards", GroupAdmitCards, "Create new admit cards"),
        (AdmitCardsEdit, "Edit Admit Cards", GroupAdmitCards, "Edit existing admit cards"),
        (AdmitCardsDelete, "Delete Admit Cards", GroupAdmitCards, "Delete admit cards"),
        (AdmitCardsGenerate, "Generate Admit Cards", GroupAdmitCards, "Generate admit cards for exam registrations"),
        (AdmitCardsDownload, "Download Admit Cards", GroupAdmitCards, "Download admit cards"),

        (RetotalingView, "View Retotaling Requests", GroupRetotaling, "View retotaling/ re-evaluation requests"),
        (RetotalingRequest, "Request Retotaling", GroupRetotaling, "Request re-evaluation of exam results"),
        (RetotalingApprove, "Approve Retotaling", GroupRetotaling, "Approve retotaling requests"),
        (RetotalingReject, "Reject Retotaling", GroupRetotaling, "Reject retotaling requests"),
        (RetotalingReview, "Review Retotaling", GroupRetotaling, "Review retotaling requests and update marks"),

        (ReportsCollegePayments, "College Payments Report", GroupReports, "View college payments report"),
        (ReportsSubjectCount, "Subject Count Report", GroupReports, "View subject count report"),
        (ReportsExamTriplicate, "Exam Triplicate Report", GroupReports, "View exam triplicate report"),
        (ReportsSummary, "Summary Report", GroupReports, "View summary report"),
        (ReportsTabulationTriplicate, "Tabulation Triplicate Report", GroupReports, "View tabulation triplicate report"),
        (ReportsProgramWiseStudent, "Program Wise Student Report", GroupReports, "View program wise student report"),
        (ReportsAttendanceSheet, "Attendance Sheet Report", GroupReports, "View attendance sheet report"),
        (ReportsMarksFoil, "Marks Foil Report", GroupReports, "View marks foil report"),
        (ReportsBankVoucherList, "Bank Voucher List Report", GroupReports, "View bank voucher list report"),

        (StudentPortalProfile, "Student Profile", "student.portal", "View own profile"),
        (StudentPortalExamForms, "Exam Forms", "student.portal", "View and submit exam forms"),
        (StudentPortalMarksheet, "Marksheet", "student.portal", "View own marksheet"),
        (StudentPortalPayment, "Make Payment", "student.portal", "Make exam fee payments"),

        (MarksEntryView, "View Marks Entry", GroupMarksEntry, "View marks entry dashboard for assigned subjects"),
        (MarksEntrySubmit, "Submit Marks", GroupMarksEntry, "Submit marks for assigned subjects"),
        (MarksEntryImport, "Import Marks", GroupMarksEntry, "Import marks from Excel for assigned subjects"),
        (MarksEntryExport, "Export Marks", GroupMarksEntry, "Export marks to Excel for assigned subjects"),

        (AuditLogView, "View Audit Log", GroupAuditLog, "View audit trail and activity logs"),

        (BackupRestoreManage, "Manage Backup & Restore", GroupBackupRestore, "Backup and restore database"),
    ];

    // Permission set per role
    public static readonly Dictionary<string, string[]> RolePermissions = new()
    {
        ["SuperAdmin"] = All.Select(p => p.Name).ToArray(),

        ["FacultyAdmin"] =
        [
            DashboardView,

            CollegesView, CollegesCreate, CollegesEdit, CollegesDelete,
            CollegeTypesView, CollegeTypesCreate, CollegeTypesEdit, CollegeTypesDelete,
            CollegeProgramsView, CollegeProgramsCreate, CollegeProgramsEdit, CollegeProgramsDelete,

            ProgramsView, ProgramsCreate, ProgramsEdit, ProgramsDelete,
            SemestersView, SemestersCreate, SemestersEdit, SemestersDelete,
            AcademicYearsView, AcademicYearsCreate, AcademicYearsEdit,

            SubjectsView, SubjectsCreate, SubjectsEdit, SubjectsDelete,
            SubjectTypesView, SubjectTypesCreate, SubjectTypesEdit,
            SubjectOfferingsView, SubjectOfferingsCreate, SubjectOfferingsEdit, SubjectOfferingsDelete,
            CurriculumVersionsView,

            ExamSchedulesView, ExamSchedulesCreate, ExamSchedulesEdit,
            ExamTypesView, ExamTypesCreate, ExamTypesEdit,
            EntranceView, EntranceApprove, EntranceReject, EntranceExport,

            GradingSchemesView, GradingSchemesCreate, GradingSchemesEdit,
            ExamRegistrationView, ExamRegistrationCreate, ExamRegistrationEdit, ExamRegistrationVerify, ExamRegistrationApprove,
            ExamSubjectResultsView, ExamSubjectResultsCreate, ExamSubjectResultsEdit,
            ResultRecordsView,

            MarksEntryView, MarksEntrySubmit, MarksEntryImport, MarksEntryExport,

            ExamCentersView, ExamCentersCreate, ExamCentersEdit, ExamCentersDelete,
            ExamCentersGenerateRollNumbers,

            AdmitCardsView, AdmitCardsCreate, AdmitCardsEdit, AdmitCardsGenerate, AdmitCardsDownload,
            RetotalingView, RetotalingReview, RetotalingApprove, RetotalingReject,

            StudentsView, StudentsCreate, StudentsEdit, StudentsDelete,
            StudentAdmissionsView, StudentAdmissionsCreate, StudentAdmissionsEdit,

            UsersView, UsersCreate, UsersEdit, UsersAssignRoles,

            BanksView, BanksCreate, BanksEdit, BanksDelete,
            PaymentTypesView, PaymentTypesCreate, PaymentTypesEdit, PaymentTypesDelete,
            BillTitlesView, BillTitlesCreate, BillTitlesEdit, BillTitlesDelete,

            NoticesView, NoticesCreate, NoticesEdit, NoticesDelete,
            BoardsView, BoardsCreate, BoardsEdit, BoardsDelete,
            GendersView, GendersCreate, GendersEdit, GendersDelete,
            EthnicitiesView, EthnicitiesCreate, EthnicitiesEdit, EthnicitiesDelete,
            CountriesView, CountriesCreate, CountriesEdit, CountriesDelete,

            SmsView, SmsCreate, SmsEdit, SmsDelete,
            GumpNowEmailView, GumpNowEmailCreate, GumpNowEmailEdit, GumpNowEmailDelete,

            AuditLogView,
            BackupRestoreManage,
        ],

        ["CollegeAdmin"] =
        [
            DashboardView,

            CollegesView,
            ProgramsView,
            SubjectsView,
            SemestersView,
            AcademicYearsView,

            ExamSchedulesView,
            ExamTypesView,
            EntranceView, EntranceApprove, EntranceReject, EntranceExport,

            GradingSchemesView,
            ExamRegistrationView, ExamRegistrationCreate, ExamRegistrationEdit,
            ExamSubjectResultsView,
            ResultRecordsView,

            MarksEntryView, MarksEntrySubmit, MarksEntryImport, MarksEntryExport,

            ExamCentersView,
            AdmitCardsView, AdmitCardsDownload,
            RetotalingView,

            StudentsView, StudentsCreate, StudentsEdit,
            StudentAdmissionsView, StudentAdmissionsCreate, StudentAdmissionsEdit,

            UsersView, UsersCreate, UsersEdit,
            PermissionsManage,

            BanksView, BanksCreate, BanksEdit,
            PaymentTypesView, PaymentTypesCreate, PaymentTypesEdit, PaymentTypesDelete,
            BillTitlesView, BillTitlesCreate, BillTitlesEdit, BillTitlesDelete,
        ],

        ["Student"] =
        [
            DashboardView,
            StudentPortalProfile,
            StudentPortalExamForms,
            StudentPortalMarksheet,
            StudentPortalPayment,
            AdmitCardsView, AdmitCardsDownload,
            RetotalingView, RetotalingRequest,
        ],
    };
}
