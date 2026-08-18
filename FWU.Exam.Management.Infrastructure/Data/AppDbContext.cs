using System.Linq.Expressions;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Permissions;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Entities.Notifications;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FWU.Exam.Management.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext>? logger = null, ITenantContext? tenantContext = null) 
    : IdentityDbContext<AppUser>(options)
{
    private readonly ILogger<AppDbContext> _logger = logger ?? NullLogger<AppDbContext>.Instance;
    private readonly ITenantContext? _tenantContext = tenantContext;

    internal static readonly AsyncLocal<ITenantContext?> AmbientTenantContext = new();
    private readonly bool _ambientContextInitialized = SetAmbientTenantContext(tenantContext);

    private static bool SetAmbientTenantContext(ITenantContext? ctx)
    {
        AmbientTenantContext.Value = ctx;
        return true;
    }

    internal static int GetCurrentTenantId() => AmbientTenantContext.Value?.TenantId ?? 0;
    internal static bool IsCurrentTenantCentral() => AmbientTenantContext.Value?.IsCentralTenant ?? false;
    internal static bool IsCurrentUserCollegeAdmin() => AmbientTenantContext.Value?.IsCollegeAdmin ?? false;
    internal static int? GetCurrentCollegeId() => AmbientTenantContext.Value?.CollegeId;
    internal static IReadOnlyList<int> GetCurrentCollegeTenantIds() => AmbientTenantContext.Value?.CollegeTenantIds ?? [];

    // Rooted at the DbContext instance so EF Core query filters parameterize (and re-evaluate
    // per query execution) instead of inlining the value once into the compiled query cache.
    private int FilterTenantId => GetCurrentTenantId();
    private bool FilterIsCentral => IsCurrentTenantCentral();
    private bool FilterIsCollegeAdmin => IsCurrentUserCollegeAdmin();
    private int? FilterCollegeId => GetCurrentCollegeId();
    private IReadOnlyList<int> FilterCollegeTenantIds => GetCurrentCollegeTenantIds();

    public DbSet<AcademicYear> AcademicYears { get; set; } = null!;
    public DbSet<Address> Addresses { get; set; } = null!;
    public DbSet<Bank> Banks { get; set; } = null!;
    public DbSet<Batch> Batches { get; set; } = null!;
    public DbSet<Board> Boards { get; set; } = null!;
    public DbSet<College> Colleges { get; set; } = null!;
    public DbSet<CollegeFaculty> CollegeFaculties { get; set; } = null!;
    public DbSet<CollegeProgram> CollegePrograms { get; set; } = null!;
    public DbSet<CollegeType> CollegeTypes { get; set; } = null!;
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<District> Districts { get; set; } = null!;
    public DbSet<Ethnicity> Ethnicities { get; set; } = null!;
    public DbSet<ExamCenter> ExamCenters { get; set; } = null!;
    public DbSet<ExamFee> ExamFees { get; set; } = null!;
    public DbSet<ExamRegistration> ExamRegistrations { get; set; } = null!;
    public DbSet<ExamRollNumberSetup> ExamRollNumberSetup { get; set; } = null!;
    public DbSet<ExamSchedule> ExamSchedules { get; set; } = null!;
    public DbSet<ExamSubjectResult> ExamSubjectResults { get; set; } = null!;
    public DbSet<ExamSlot> ExamSlots { get; set; } = null!;
    public DbSet<ExamType> ExamTypes { get; set; } = null!;
    public DbSet<AdmitCard> AdmitCards { get; set; } = null!;
    public DbSet<RetotalRequest> RetotalRequests { get; set; } = null!;
    public DbSet<Gender> Genders { get; set; } = null!;
    public DbSet<Level> Levels { get; set; } = null!;
    public DbSet<LocalLevel> LocalLevels { get; set; } = null!;
    public DbSet<NepaliDate> NepaliDates { get; set; } = null!;
    public DbSet<Notice> Notices { get; set; } = null!;
    public DbSet<PreviousLevel> PreviousLevels { get; set; } = null!;
    public DbSet<Program> Programs { get; set; } = null!;
    public DbSet<ProgramSemester> ProgramSemesters { get; set; } = null!;
    public DbSet<ResultRecord> ResultRecords { get; set; } = null!;
    public DbSet<Semester> Semesters { get; set; } = null!;
    public DbSet<SemesterEnrollment> SemesterEnrollments { get; set; } = null!;
    public DbSet<SemesterInstance> SemesterInstances { get; set; } = null!;
    public DbSet<StudentAdmission> StudentAdmissions { get; set; } = null!;
    public DbSet<StudentCategory> StudentCategories { get; set; } = null!;
    public DbSet<StudentGuardian> StudentGuardians { get; set; } = null!;
    public DbSet<StudentQualification> StudentQualifications { get; set; } = null!;
    public DbSet<StudentRegistration> StudentRegistrations { get; set; } = null!;
    public DbSet<SubjectCatalog> SubjectCatalogs { get; set; } = null!;
    public DbSet<SubjectOffering> SubjectOfferings { get; set; } = null!;
    public DbSet<SubjectType> SubjectTypes { get; set; } = null!;
    public DbSet<CurriculumVersion> CurriculumVersions { get; set; } = null!;
    public DbSet<Province> Provinces { get; set; } = null!;
    public DbSet<Faculty> Faculties { get; set; } = null!;
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<SmtpConfiguration> SmtpConfigurations { get; set; } = null!;
    public DbSet<SmsConfiguration> SmsConfigurations { get; set; } = null!;
    public DbSet<GumpNowEmailConfiguration> GumpNowEmailConfigurations { get; set; } = null!;
    public DbSet<GumpNowEmailLog> GumpNowEmailLogs { get; set; } = null!;
    public DbSet<SmsLog> SmsLogs { get; set; } = null!;
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; } = null!;
    public DbSet<UserAttachment> UserAttachments { get; set; } = null!;
    public DbSet<GradingScheme> GradingSchemes { get; set; } = null!;
    public DbSet<GradeDefinition> GradeDefinitions { get; set; } = null!;
    public DbSet<GradeGroup> GradeGroups { get; set; } = null!;
    public DbSet<GradePoint> GradePoints { get; set; } = null!;
    public DbSet<EntranceExamApplication> EntranceExamApplications { get; set; } = null!;
    public DbSet<ApplicationVoucher> ApplicationVouchers { get; set; } = null!;
    public DbSet<PaymentRequestLog> PaymentRequestLogs { get; set; } = null!;
    public DbSet<PaymentResponseLog> PaymentResponseLogs { get; set; } = null!;
    public DbSet<PaymentPracticalSubjects> PaymentPracticalSubjects { get; set; } = null!;
    public DbSet<ESewaConfiguration> ESewaConfigurations { get; set; } = null!;
    public DbSet<KhaltiConfiguration> KhaltiConfigurations { get; set; } = null!;
    public DbSet<ConnectIpsPaymentConfiguration> ConnectIpsPaymentConfigurations { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<ExamCenterCollege> ExamCenterColleges { get; set; } = null!;
    public DbSet<ExamCenterVenue> ExamCenterVenues { get; set; } = null!;
    public DbSet<ExamCenterSymbolRange> ExamCenterSymbolRanges { get; set; } = null!;
    public DbSet<CollegeAdminSubjectAssignment> CollegeAdminSubjectAssignments { get; set; } = null!;

    public DbSet<AuditLog> AuditLogs { get; set; } = null!;
    public DbSet<BulkUserCreationJob> BulkUserCreationJobs { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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

        foreach (var entityType in builder.Model.GetEntityTypes().Where(e => typeof(ITenantScoped).IsAssignableFrom(e.ClrType) && e.ClrType != typeof(Tenant)))
        {
            var param = Expression.Parameter(entityType.ClrType, "e");
            var tenantIdProp = Expression.Call(typeof(EF), nameof(EF.Property), [typeof(int)], param, Expression.Constant("TenantId"));
            var context = Expression.Constant(this, typeof(AppDbContext));
            var tenantIdValue = Expression.Property(context, nameof(FilterTenantId));
            var isCentral = Expression.Property(context, nameof(FilterIsCentral));
            var isCollegeAdmin = Expression.Property(context, nameof(FilterIsCollegeAdmin));
            var collegeTenantIds = Expression.Property(context, nameof(FilterCollegeTenantIds));
            var collegeAdminTenants = Expression.Call(
                typeof(Enumerable), nameof(Enumerable.Contains), [typeof(int)], collegeTenantIds, tenantIdProp);
            var collegeAdminBranch = Expression.AndAlso(isCollegeAdmin, collegeAdminTenants);
            var tenantBranch = Expression.Equal(tenantIdProp, tenantIdValue);
            var body = Expression.OrElse(isCentral, Expression.OrElse(collegeAdminBranch, tenantBranch));
            var lambda = Expression.Lambda(body, param);
            entityType.SetQueryFilter(lambda);

            builder.Entity(entityType.ClrType)
                .HasOne("Tenant")
                .WithMany()
                .HasForeignKey("TenantId")
                .OnDelete(DeleteBehavior.Restrict);
        }

        builder.Entity<NotificationTemplate>()
            .HasIndex(t => new { t.Code, t.Channel })
            .IsUnique();

        builder.Entity<College>()
            .HasQueryFilter(c => FilterIsCentral ||
                (FilterIsCollegeAdmin && c.Id == FilterCollegeId) ||
                c.CollegeFaculties!.Any(cf => cf.TenantId == FilterTenantId));

        builder.Entity<CollegeFaculty>()
            .ToTable("CollegeFaculties");

        builder.Entity<CollegeFaculty>()
            .HasKey(cf => new { cf.CollegeId, cf.FacultyId });

        builder.Entity<CollegeFaculty>()
            .HasOne(cf => cf.College)
            .WithMany(c => c.CollegeFaculties)
            .HasForeignKey(cf => cf.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CollegeFaculty>()
            .HasOne(cf => cf.Faculty)
            .WithMany(f => f.CollegeFaculties)
            .HasForeignKey(cf => cf.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BillTitle>()
            .HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BankVoucher>()
            .HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<AppUser>().ToTable("Users");

        builder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.Entity<College>()
            .HasOne(c => c.CollegeType)
            .WithMany(ct => ct.Colleges)
            .HasForeignKey(c => c.CollegeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<District>()
            .HasOne(d => d.Province)
            .WithMany(p => p.Districts)
            .HasForeignKey(d => d.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<LocalLevel>()
            .HasOne(ll => ll.District)
            .WithMany(d => d.LocalLevels)
            .HasForeignKey(ll => ll.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Address>()
            .HasOne(a => a.LocalLevel)
            .WithMany(ll => ll.Addresses)
            .HasForeignKey(a => a.LocalLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<College>()
            .HasOne(c => c.Address)
            .WithMany()
            .HasForeignKey(c => c.AddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Program>()
            .HasOne(p => p.Faculty)
            .WithMany()
            .HasForeignKey(p => p.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.PermanentAddress)
            .WithMany()
            .HasForeignKey(sr => sr.PermanentAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.CurrentAddress)
            .WithMany()
            .HasForeignKey(sr => sr.CurrentAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Batch>()
            .HasOne(b => b.AcademicYear)
            .WithMany(ay => ay.Batches)
            .HasForeignKey(b => b.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSchedule>()
            .HasOne(es => es.SemesterInstance)
            .WithMany()
            .HasForeignKey(es => es.SemesterInstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSchedule>()
            .HasOne(es => es.ExamType)
            .WithMany()
            .HasForeignKey(es => es.ExamTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamCenter>()
            .HasOne(ec => ec.ExamSchedule)
            .WithMany(es => es.ExamCenters)
            .HasForeignKey(ec => ec.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.Entity<StudentAdmission>()
            .HasOne(sa => sa.Program)
            .WithMany(p => p.StudentAdmissions)
            .HasForeignKey(sa => sa.ProgramsId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentAdmission>()
            .HasOne(sa => sa.College)
            .WithMany(c => c.StudentAdmissions)
            .HasForeignKey(sa => sa.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentAdmission>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(nameof(StudentAdmission.AppUserId))
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentAdmission>()
            .HasOne(sa => sa.AcademicYear)
            .WithMany()
            .HasForeignKey(sa => sa.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.StudentAdmission)
            .WithOne(sa => sa.StudentRegistration)
            .HasForeignKey<StudentRegistration>(sr => sr.StudentAdmissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRegistration>()
            .HasOne(er => er.ExamCenter)
            .WithMany(ec => ec.ExamRegistrations)
            .HasForeignKey(er => er.ExamCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRegistration>()
            .HasOne(er => er.Program)
            .WithMany(p => p.ExamRegistrations)
            .HasForeignKey(er => er.ProgramsId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRegistration>()
            .HasOne(er => er.ExamSchedule)
            .WithMany(es => es.ExamRegistrations)
            .HasForeignKey(er => er.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRegistration>()
            .HasOne(er => er.AcademicYear)
            .WithMany(ay => ay.ExamRegistrations)
            .HasForeignKey(er => er.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRegistration>()
            .HasOne(er => er.College)
            .WithMany(c => c.ExamRegistrations)
            .HasForeignKey(er => er.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);



        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.AcademicYear)
            .WithMany(ay => ay.StudentRegistrations)
            .HasForeignKey(sr => sr.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.Level)
            .WithMany(l => l.StudentRegistrations)
            .HasForeignKey(sr => sr.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.College)
            .WithMany(c => c.StudentRegistrations)
            .HasForeignKey(sr => sr.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.Gender)
            .WithMany(g => g.StudentRegistrations)
            .HasForeignKey(sr => sr.GenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.StudentCategory)
            .WithMany(sc => sc.StudentRegistrations)
            .HasForeignKey(sr => sr.StudentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.Ethnicity)
            .WithMany(e => e.StudentRegistrations)
            .HasForeignKey(sr => sr.EthnicityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.Faculty)
            .WithMany()
            .HasForeignKey(sr => sr.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.Program)
            .WithMany()
            .HasForeignKey(sr => sr.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasIndex(sr => sr.Email)
            .IsUnique()
            .HasFilter("[Email] IS NOT NULL");

        builder.Entity<StudentGuardian>()
            .HasOne(sg => sg.StudentRegistration)
            .WithMany(sr => sr.StudentGuardians)
            .HasForeignKey(sg => sg.StudentRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentQualification>()
            .HasOne(sq => sq.StudentRegistration)
            .WithMany(sr => sr.StudentQualifications)
            .HasForeignKey(sq => sq.StudentRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentQualification>()
            .HasOne(sq => sq.Board)
            .WithMany(b => b.StudentQualifications)
            .HasForeignKey(sq => sq.BoardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentQualification>()
            .HasOne(sq => sq.PreviousLevel)
            .WithMany(pl => pl.StudentQualifications)
            .HasForeignKey(sq => sq.PreviousLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CollegeProgram>()
            .HasOne(cp => cp.College)
            .WithMany(c => c.CollegePrograms)
            .HasForeignKey(cp => cp.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CollegeProgram>()
            .HasOne(cp => cp.Program)
            .WithMany(p => p.CollegePrograms)
            .HasForeignKey(cp => cp.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ResultRecord>()
            .HasOne(rr => rr.AcademicYear)
            .WithMany()
            .HasForeignKey(rr => rr.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ResultRecord>()
            .HasOne(rr => rr.Program)
            .WithMany()
            .HasForeignKey(rr => rr.ProgramsId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ResultRecord>()
            .HasOne(rr => rr.ExamType)
            .WithMany()
            .HasForeignKey(rr => rr.ExamTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ResultRecord>()
            .HasOne(rr => rr.College)
            .WithMany()
            .HasForeignKey(rr => rr.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ResultRecord>()
            .HasOne(rr => rr.ExamSchedule)
            .WithMany()
            .HasForeignKey(rr => rr.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ResultRecord>()
            .HasOne(rr => rr.Level)
            .WithMany()
            .HasForeignKey(rr => rr.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ResultRecord>()
            .ToTable("ResultRecords")
            .HasKey(rr => rr.Id);

        builder.Entity<ExamFee>()
            .HasOne(ef => ef.ExamSchedule)
            .WithMany()
            .HasForeignKey(ef => ef.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamFee>()
            .HasOne(ef => ef.CollegeType)
            .WithMany(ct => ct.ExamFees)
            .HasForeignKey(ef => ef.CollegeTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamFee>()
            .HasOne(ef => ef.ExamType)
            .WithMany()
            .HasForeignKey(ef => ef.ExamTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PreviousLevel>()
            .HasOne(pl => pl.Level)
            .WithMany()
            .HasForeignKey(pl => pl.LevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Program>().ToTable("Programs");

        builder.Entity<SubjectCatalog>()
            .HasIndex(sc => sc.SubjectCode);

        builder.Entity<SubjectCatalog>()
            .HasOne(sc => sc.SubjectType)
            .WithMany(st => st.SubjectCatalogs)
            .HasForeignKey(sc => sc.SubjectTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SubjectOffering>()
            .HasOne(so => so.SubjectCatalog)
            .WithMany(sc => sc.SubjectOfferings)
            .HasForeignKey(so => so.SubjectCatalogId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SubjectOffering>()
            .HasOne(so => so.Program)
            .WithMany()
            .HasForeignKey(so => so.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SubjectOffering>()
            .HasOne(so => so.Semester)
            .WithMany(s => s.SubjectOfferings)
            .HasForeignKey(so => so.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SubjectOffering>()
            .HasOne(so => so.CurriculumVersion)
            .WithMany(cv => cv.SubjectOfferings)
            .HasForeignKey(so => so.CurriculumVersionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SubjectOffering>()
            .HasIndex(so => new { so.CurriculumVersionId, so.SubjectCatalogId, so.ProgramId, so.SemesterId })
            .IsUnique()
            .HasFilter("[IsActive] = 1");

        builder.Entity<CurriculumVersion>()
            .HasOne(cv => cv.Program)
            .WithMany()
            .HasForeignKey(cv => cv.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CurriculumVersion>()
            .HasOne(cv => cv.EffectiveAcademicYear)
            .WithMany()
            .HasForeignKey(cv => cv.EffectiveAcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationVoucher>()
            .HasOne(av => av.ExamSchedule)
            .WithMany()
            .HasForeignKey(av => av.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationVoucher>()
            .HasOne(av => av.StudentRegistration)
            .WithMany(sr => sr.ApplicationVouchers)
            .HasForeignKey(av => av.StudentRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BillTitle>()
            .HasOne(bt => bt.ExamSchedule)
            .WithMany()
            .HasForeignKey(bt => bt.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BillTitle>()
            .HasOne(bt => bt.Program)
            .WithMany()
            .HasForeignKey(bt => bt.ProgramsId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRollNumberSetup>()
            .HasOne(ers => ers.ExamSchedule)
            .WithMany()
            .HasForeignKey(ers => ers.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSubjectResult>()
            .HasOne(esr => esr.ExamRegistration)
            .WithMany(er => er.ExamSubjectResults)
            .HasForeignKey(esr => esr.ExamRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSubjectResult>()
            .HasOne(esr => esr.ExamType)
            .WithMany()
            .HasForeignKey(esr => esr.ExamTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSubjectResult>()
            .HasOne(esr => esr.SubjectOffering)
            .WithMany()
            .HasForeignKey(esr => esr.SubjectOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSubjectResult>()
            .HasOne(esr => esr.ExamSchedule)
            .WithMany(es => es.ExamSubjectResults)
            .HasForeignKey(esr => esr.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSlot>()
            .HasOne(ess => ess.ExamSchedule)
            .WithMany(es => es.ExamSlots)
            .HasForeignKey(ess => ess.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSlot>()
            .HasOne(ess => ess.SubjectOffering)
            .WithMany(so => so.ExamSlots)
            .HasForeignKey(ess => ess.SubjectOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSlot>()
            .HasOne(ess => ess.Batch)
            .WithMany(b => b.ExamSlots)
            .HasForeignKey(ess => ess.BatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSlot>()
            .HasOne(ess => ess.ExamCenter)
            .WithMany(ec => ec.ExamSlots)
            .HasForeignKey(ess => ess.ExamCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CollegeAdminSubjectAssignment>()
            .HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(tsa => tsa.CollegeAdminUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CollegeAdminSubjectAssignment>()
            .HasOne(tsa => tsa.SubjectOffering)
            .WithMany()
            .HasForeignKey(tsa => tsa.SubjectOfferingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CollegeAdminSubjectAssignment>()
            .HasOne(tsa => tsa.ExamSchedule)
            .WithMany()
            .HasForeignKey(tsa => tsa.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamCenterCollege>()
            .HasOne(ecc => ecc.ExamCenter)
            .WithMany(ec => ec.ExamCenterColleges)
            .HasForeignKey(ecc => ecc.ExamCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamCenterCollege>()
            .HasOne(ecc => ecc.College)
            .WithMany()
            .HasForeignKey(ecc => ecc.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamCenterVenue>()
            .HasOne(ecv => ecv.ExamCenter)
            .WithMany(ec => ec.ExamCenterVenues)
            .HasForeignKey(ecv => ecv.ExamCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamCenterVenue>()
            .HasOne(ecv => ecv.College)
            .WithMany()
            .HasForeignKey(ecv => ecv.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamCenterSymbolRange>()
            .HasOne(ecsr => ecsr.ExamSchedule)
            .WithMany()
            .HasForeignKey(ecsr => ecsr.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamCenterSymbolRange>()
            .HasOne(ecsr => ecsr.ExamCenter)
            .WithMany(ec => ec.ExamCenterSymbolRanges)
            .HasForeignKey(ecsr => ecsr.ExamCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AdmitCard>(entity =>
        {
            entity.ToTable("HallTickets");

            entity.Property(e => e.AdmitCardNumber)
                .HasColumnName("HallTicketNumber");

            entity.HasOne(ht => ht.ExamRegistration)
                .WithMany()
                .HasForeignKey(ht => ht.ExamRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ht => ht.ExamSchedule)
                .WithMany()
                .HasForeignKey(ht => ht.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ht => ht.StudentRegistration)
                .WithMany()
                .HasForeignKey(ht => ht.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RetotalRequest>()
            .HasOne(rr => rr.ExamSubjectResult)
            .WithMany()
            .HasForeignKey(rr => rr.ExamSubjectResultId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RetotalRequest>()
            .HasOne(rr => rr.StudentRegistration)
            .WithMany()
            .HasForeignKey(rr => rr.StudentRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<RetotalRequest>()
            .HasOne(rr => rr.ExamRegistration)
            .WithMany()
            .HasForeignKey(rr => rr.ExamRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PaymentRequestLog>()
            .HasOne(prl => prl.PaymentType)
            .WithMany(pt => pt.PaymentRequestLogs)
            .HasForeignKey(prl => prl.PaymentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PaymentRequestLog>()
            .HasOne(prl => prl.StudentRegistration)
            .WithMany(sr => sr.PaymentRequestLogs)
            .HasForeignKey(prl => prl.StudentRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PaymentRequestLog>()
            .HasOne(prl => prl.College)
            .WithMany()
            .HasForeignKey(prl => prl.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PaymentResponseLog>()
            .HasOne(prl => prl.PaymentRequestLog)
            .WithMany(pr => pr.PaymentResponseLog)
            .HasForeignKey(prl => prl.PaymentRequestLogId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<PaymentPracticalSubjects>()
            .HasOne(pps => pps.PaymentRequestLog)
            .WithMany(pr => pr.PaymentPracticalSubjects)
            .HasForeignKey(pps => pps.PaymentRequestLogId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AppUser>()
            .HasOne(u => u.Faculty)
            .WithMany()
            .HasForeignKey(u => u.FacultyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AppUser>()
            .HasOne(u => u.College)
            .WithMany()
            .HasForeignKey(u => u.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BankVoucher>()
            .HasOne(bv => bv.BankVoucherAttachment)
            .WithMany()
            .HasForeignKey(bv => bv.BankVoucherUserAttachmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GradingScheme>()
            .HasOne(gs => gs.Program)
            .WithMany()
            .HasForeignKey(gs => gs.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GradingScheme>()
            .HasOne(gs => gs.AcademicYear)
            .WithMany()
            .HasForeignKey(gs => gs.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GradingScheme>()
            .HasOne(gs => gs.GradeGroup)
            .WithMany()
            .HasForeignKey(gs => gs.GradeGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GradeGroup>()
            .HasIndex(gg => gg.GradeGroupName);

        builder.Entity<GradeGroup>(e => e.Property(x => x.Id).ValueGeneratedNever());
        builder.Entity<GradePoint>(e => e.Property(x => x.Id).ValueGeneratedNever());

        builder.Entity<GradePoint>()
            .HasOne(gp => gp.GradeGroup)
            .WithMany(gg => gg.GradePoints)
            .HasForeignKey(gp => gp.GradeGroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GradePoint>()
            .HasIndex(gp => new { gp.GradeGroupId, gp.ObtainedMark })
            .IsUnique();

        builder.Entity<GradeDefinition>()
            .HasOne(gd => gd.GradingScheme)
            .WithMany(gs => gs.GradeDefinitions)
            .HasForeignKey(gd => gd.GradingSchemeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationVoucher>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<College>(e => e.Property(x => x.AllocatedAmount).HasPrecision(18, 2));
        builder.Entity<ExamFee>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<ExamRegistration>(e => e.Property(x => x.AttendancePercentage).HasPrecision(5, 2));
        builder.Entity<ExamRegistration>(e => e.Property(x => x.FeeEnclosed).HasPrecision(18, 2));
        builder.Entity<ExamSchedule>(e => e.Property(x => x.ExtendedDateCharge).HasPrecision(18, 2));
        builder.Entity<ExamSchedule>(e => e.Property(x => x.ExamFee).HasPrecision(18, 2));
        builder.Entity<ExamSchedule>(e => e.Property(x => x.PracticalSubjectFee).HasPrecision(18, 2));
        builder.Entity<BillTitle>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<BillTitle>(e => e.Property(x => x.PracticalFee).HasPrecision(18, 2));
        builder.Entity<ESewaConfiguration>(e =>
        {
            e.ToTable("ESewaConfiguration");
            e.Property(x => x.ServiceChargeAmount).HasPrecision(18, 2);
        });
        builder.Entity<KhaltiConfiguration>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<ConnectIpsPaymentConfiguration>(e =>
        {
            e.ToTable("ConnectIpsPaymentConfiguration");
        });
        builder.Entity<PaymentPracticalSubjects>(e => e.Property(x => x.TotalAmount).HasPrecision(18, 2));
        builder.Entity<PaymentRequestLog>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<StudentQualification>(e => e.Property(x => x.Percentage).HasPrecision(5, 2));

        builder.Entity<BankVoucher>(e => e.Property(x => x.VoucherAmount).HasPrecision(18, 2));
        builder.Entity<GradeDefinition>(e => e.Property(x => x.GradePoint).HasPrecision(5, 2));
        builder.Entity<GradeDefinition>(e => e.Property(x => x.MaxPercentage).HasPrecision(5, 2));
        builder.Entity<GradeDefinition>(e => e.Property(x => x.MinPercentage).HasPrecision(5, 2));
        builder.Entity<GradePoint>(e => e.Property(x => x.GradePointValue).HasPrecision(5, 2));

        builder.Entity<EntranceExamApplication>(e => e.Property(x => x.PreviousGPA).HasPrecision(5, 2));
        builder.Entity<EntranceExamApplication>(e => e.Property(x => x.PreviousGPA2).HasPrecision(5, 2));
        builder.Entity<EntranceExamApplication>(e => e.Property(x => x.PreviousGPA3).HasPrecision(5, 2));

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.AcademicYear)
            .WithMany()
            .HasForeignKey(a => a.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.College)
            .WithMany()
            .HasForeignKey(a => a.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.Program)
            .WithMany()
            .HasForeignKey(a => a.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.Gender)
            .WithMany()
            .HasForeignKey(a => a.GenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.PermanentAddress)
            .WithMany()
            .HasForeignKey(a => a.PermanentAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.PreviousLevel)
            .WithMany()
            .HasForeignKey(a => a.PreviousLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.PreviousLevel2)
            .WithMany()
            .HasForeignKey(a => a.PreviousLevel2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.PreviousLevel3)
            .WithMany()
            .HasForeignKey(a => a.PreviousLevel3Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.CitizenshipDistrict)
            .WithMany()
            .HasForeignKey(a => a.CitizenshipDistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<EntranceExamApplication>()
            .HasOne(a => a.ApplicationVoucher)
            .WithMany()
            .HasForeignKey(a => a.ApplicationVoucherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique indexes - Global entities
        builder.Entity<Tenant>()
            .HasIndex(t => t.OfficeCode)
            .IsUnique()
            .HasFilter("[OfficeCode] IS NOT NULL");

        builder.Entity<Faculty>()
            .HasIndex(f => f.OfficeCode)
            .IsUnique()
            .HasFilter("[OfficeCode] IS NOT NULL");

        builder.Entity<Program>()
            .HasIndex(p => p.ProgramCode)
            .IsUnique()
            .HasFilter("[ProgramCode] IS NOT NULL");

        builder.Entity<Level>()
            .HasIndex(l => l.LevelCode)
            .IsUnique()
            .HasFilter("[LevelCode] IS NOT NULL");

        builder.Entity<SubjectType>()
            .HasIndex(st => st.Code)
            .IsUnique();

        builder.Entity<CollegeType>()
            .HasIndex(ct => ct.Code)
            .IsUnique();

        builder.Entity<ExamType>()
            .HasIndex(et => et.Name)
            .IsUnique()
            .HasFilter("[Name] IS NOT NULL");

        builder.Entity<District>()
            .HasIndex(d => d.DistrictCode)
            .IsUnique()
            .HasFilter("[DistrictCode] IS NOT NULL");

        builder.Entity<Province>()
            .HasIndex(p => p.ProvinceCode)
            .IsUnique()
            .HasFilter("[ProvinceCode] IS NOT NULL");

        builder.Entity<Bank>()
            .HasIndex(b => new { b.TenantId, b.BankCode })
            .IsUnique()
            .HasFilter("[BankCode] IS NOT NULL");

        builder.Entity<Gender>()
            .HasIndex(g => g.GenderName)
            .IsUnique()
            .HasFilter("[GenderName] IS NOT NULL");

        builder.Entity<Ethnicity>()
            .HasIndex(e => e.EthnicityName)
            .IsUnique()
            .HasFilter("[EthnicityName] IS NOT NULL");

        builder.Entity<Country>()
            .HasIndex(c => c.CountryName)
            .IsUnique()
            .HasFilter("[CountryName] IS NOT NULL");

        builder.Entity<Board>()
            .HasIndex(b => b.BoardName)
            .IsUnique()
            .HasFilter("[BoardName] IS NOT NULL");

        builder.Entity<PaymentType>()
            .HasIndex(pt => new { pt.TenantId, pt.PaymentTypeName })
            .IsUnique()
            .HasFilter("[PaymentTypeName] IS NOT NULL");

        // Unique indexes - Tenant-scoped composite (TenantId, Code)
        builder.Entity<College>()
            .HasIndex(c => c.Code)
            .IsUnique();

        builder.Entity<AcademicYear>()
            .HasIndex(ay => new { ay.TenantId, ay.AcademicYearCode })
            .IsUnique();

        builder.Entity<ExamSchedule>()
            .HasIndex(es => new { es.TenantId, es.ExamScheduleCode })
            .IsUnique()
            .HasFilter("[ExamScheduleCode] IS NOT NULL");

        builder.Entity<Semester>()
            .HasIndex(s => s.Code)
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        builder.Entity<ProgramSemester>()
            .HasOne(ps => ps.Program)
            .WithMany(p => p.ProgramSemesters)
            .HasForeignKey(ps => ps.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProgramSemester>()
            .HasOne(ps => ps.Semester)
            .WithMany(s => s.ProgramSemesters)
            .HasForeignKey(ps => ps.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProgramSemester>()
            .HasIndex(ps => new { ps.ProgramId, ps.SemesterId })
            .IsUnique();

        builder.Entity<SemesterInstance>()
            .HasOne(si => si.Semester)
            .WithMany()
            .HasForeignKey(si => si.SemesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SemesterInstance>()
            .HasOne(si => si.AcademicYear)
            .WithMany()
            .HasForeignKey(si => si.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SemesterInstance>()
            .HasOne(si => si.Program)
            .WithMany()
            .HasForeignKey(si => si.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<SemesterInstance>()
            .HasIndex(si => new { si.SemesterId, si.AcademicYearId, si.ProgramId })
            .IsUnique();

        builder.Entity<SemesterEnrollment>()
            .HasOne(se => se.SemesterInstance)
            .WithMany(si => si.SemesterEnrollments)
            .HasForeignKey(se => se.SemesterInstanceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasIndex(sr => new { sr.TenantId, sr.RegistrationNumber })
            .IsUnique()
            .HasFilter("[RegistrationNumber] IS NOT NULL");

        builder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(256).IsRequired();
            entity.Property(p => p.DisplayName).HasMaxLength(256);
            entity.Property(p => p.Group).HasMaxLength(128);
            entity.HasIndex(p => p.Name).IsUnique();
        });

        builder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
            entity.HasOne<IdentityRole>()
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Kind).HasMaxLength(32);
            entity.Property(a => a.EntityName).HasMaxLength(128);
            entity.Property(a => a.EntityId).HasMaxLength(128);
            entity.Property(a => a.Action).HasMaxLength(32);
            entity.Property(a => a.UserName).HasMaxLength(256);
            entity.Property(a => a.UserId).HasMaxLength(128);
            entity.Property(a => a.ChangesJson);
            entity.Property(a => a.ActivityType).HasMaxLength(128);
            entity.Property(a => a.Description).HasMaxLength(500);
            entity.Property(a => a.Severity).HasMaxLength(32);
            entity.Property(a => a.DetailsJson);
            entity.HasIndex(a => new { a.EntityName, a.EntityId });
            entity.HasIndex(a => new { a.Kind, a.ActivityType, a.Timestamp });
            entity.HasIndex(a => a.Timestamp);
        });

        builder.Entity<BulkUserCreationJob>(entity =>
        {
            entity.ToTable("BulkUserCreationJobs");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.UserId).HasMaxLength(128);
            entity.Property(b => b.Status).HasMaxLength(32);
            entity.Property(b => b.ErrorMessage).HasMaxLength(2000);
            entity.HasIndex(b => b.Status);
            entity.HasIndex(b => b.CreatedAt);
        });
    }
}
