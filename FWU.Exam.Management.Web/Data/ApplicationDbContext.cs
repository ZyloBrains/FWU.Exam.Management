using fwu_examination_management_system.Data.Auditing;
using fwu_examination_management_system.Data.Models;
using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Payments;
using fwu_examination_management_system.Data.Models.Students;
using fwu_examination_management_system.Data.Models.Subjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace fwu_examination_management_system.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger) 
    : IdentityDbContext<AppUser>(options)
{
    private readonly ILogger<ApplicationDbContext> _logger = logger;

    public DbSet<Organization> Organizations { get; set; }
    public DbSet<AcademicYear> AcademicYears { get; set; }
    public DbSet<ActiveExamSchedule> ActiveExamSchedules { get; set; }
    public DbSet<ApplicationVoucher> ApplicationVouchers { get; set; }
    public DbSet<Area> Areas { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<BankVoucher> BankVouchers { get; set; }
    public DbSet<Batch> Batches { get; set; }
    public DbSet<BillTitle> BillTitles { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<College> Colleges { get; set; }
    public DbSet<CollegeProfile> CollegeProfiles { get; set; }
    public DbSet<CollegeProgram> CollegePrograms { get; set; }
    public DbSet<CollegeType> CollegeTypes { get; set; }
    public DbSet<ConnectIpsPaymentConfiguration> ConnectIpsPaymentConfigurations { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<EntryFormat> EntryFormats { get; set; }
    public DbSet<ESewaConfiguration> ESewaConfigurations { get; set; }
    public DbSet<Ethnicity> Ethnicities { get; set; }
    public DbSet<ExamAttendanceStatus> ExamAttendanceStatuses { get; set; }
    public DbSet<ExamCenter> ExamCenters { get; set; }
    public DbSet<ExamCenterDetail> ExamCenterDetails { get; set; }
    public DbSet<ExamFormFeeName> ExamFormFeeNames { get; set; }
    public DbSet<ExamFormFeeRate> ExamFormFeeRates { get; set; }
    public DbSet<ExamRegistration> ExamRegistrations { get; set; }
    public DbSet<ExamRegistrationActionLog> ExamRegistrationActionLogs { get; set; }
    public DbSet<ExamRegistrationCenterChange> ExamRegistrationCenterChanges { get; set; }
    public DbSet<ExamRollNumberSetup> ExamRollNumberSetups { get; set; }
    public DbSet<ExamRollNumberSetupDetail> ExamRollNumberSetupDetails { get; set; }
    public DbSet<ExamSchedule> ExamSchedules { get; set; }
    public DbSet<ExamScheduleBatch> ExamScheduleBatches { get; set; }
    public DbSet<ExamScheduleDetail> ExamScheduleDetails { get; set; }
    public DbSet<ExamScheduleParent> ExamScheduleParents { get; set; }
    public DbSet<ExamSubjectRegistration> ExamSubjectRegistrations { get; set; }
    public DbSet<ExamSubjectRegistrationExamSession> ExamSubjectRegistrationExamSessions { get; set; }
    public DbSet<ExamSubjectRegistrationInternal> ExamSubjectRegistrationInternals { get; set; }
    public DbSet<ExamType> ExamTypes { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<FiscalYear> FiscalYears { get; set; }
    public DbSet<Gender> Genders { get; set; }
    public DbSet<IndexGroup> IndexGroups { get; set; }
    public DbSet<KhaltiConfiguration> KhaltiConfigurations { get; set; }
    public DbSet<Level> Levels { get; set; }
    public DbSet<LocalLevel> LocalLevels { get; set; }
    public DbSet<NepaliDate> NepaliDates { get; set; }
    public DbSet<Notice> Notices { get; set; }
    public DbSet<PasswordResetLog> PasswordResetLogs { get; set; }
    public DbSet<PaymentPracticalSubjects> PaymentPracticalSubjects { get; set; }
    public DbSet<PaymentRequestLog> PaymentRequestLogs { get; set; }
    public DbSet<PaymentResponseLog> PaymentResponseLogs { get; set; }
    public DbSet<PaymentType> PaymentTypes { get; set; }
    public DbSet<PeriodType> PeriodTypes { get; set; }
    public DbSet<PreferredExamCenter> PreferredExamCenters { get; set; }
    public DbSet<PreviousLevel> PreviousLevels { get; set; }
    public DbSet<Programs> Programs { get; set; }
    public DbSet<ProgramPeriodType> ProgramPeriodTypes { get; set; }
    public DbSet<ProgramSubjectPracticalCharge> ProgramSubjectPracticalCharges { get; set; }
    public DbSet<ProgramYearPart> ProgramYearParts { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<QuestionSet> QuestionSets { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<ResultRecord> ResultRecords { get; set; }
    public DbSet<SchoolType> SchoolTypes { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<SmtpConfiguration> SmtpConfigurations { get; set; }
    public DbSet<StudentAdmission> StudentAdmissions { get; set; }
    public DbSet<StudentCategory> StudentCategories { get; set; }
    public DbSet<StudentGuardian> StudentGuardians { get; set; }
    public DbSet<StudentProgramYearPart> StudentProgramYearParts { get; set; }
    public DbSet<StudentQualification> StudentQualifications { get; set; }
    public DbSet<StudentRegistration> StudentRegistrations { get; set; }
    public DbSet<StudentRegistrationSearch> StudentRegistrationSearches { get; set; }
    public DbSet<SubjectBatch> SubjectBatches { get; set; }
    public DbSet<SubjectDetail> SubjectDetails { get; set; }
    public DbSet<SubjectGroup> SubjectGroups { get; set; }
    public DbSet<SubjectGroupDetailMap> SubjectGroupDetailMaps { get; set; }
    public DbSet<SubjectTriplicate> SubjectTriplicates { get; set; }
    public DbSet<SubjectType> SubjectTypes { get; set; }
    public DbSet<UserAttachment> UserAttachments { get; set; }
    public DbSet<UserProgramMap> UserProgramMaps { get; set; }
    public DbSet<YearPart> YearParts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure shadow properties for IAuditable
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(IAuditable).IsAssignableFrom(entityType.ClrType))
            {
                entityType.AddProperty("CreatedBy", typeof(string));
                entityType.AddProperty("CreatedDate", typeof(DateTime?));
                entityType.AddProperty("UpdatedBy", typeof(string));
                entityType.AddProperty("UpdatedDate", typeof(DateTime?));
            }
        }

        // Remove "AspNet" prefix from Identity tables
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<AppUser>().ToTable("Users");
    }
}
