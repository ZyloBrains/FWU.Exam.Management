using System.Linq.Expressions;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
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

    public DbSet<AcademicYear>? AcademicYears { get; set; }
    public DbSet<Address>? Addresses { get; set; }
    public DbSet<Bank>? Banks { get; set; }
    public DbSet<Batch>? Batches { get; set; }
    public DbSet<Board>? Boards { get; set; }
    public DbSet<College>? Colleges { get; set; }
    public DbSet<CollegeProgram>? CollegePrograms { get; set; }
    public DbSet<CollegeType>? CollegeTypes { get; set; }
    public DbSet<District>? Districts { get; set; }
    public DbSet<EntryFormat>? EntryFormats { get; set; }
    public DbSet<Ethnicity>? Ethnicities { get; set; }
    public DbSet<ExamCenter>? ExamCenters { get; set; }
    public DbSet<ExamFee>? ExamFees { get; set; }
    public DbSet<ExamRegistration>? ExamRegistrations { get; set; }
    public DbSet<ExamRollNumberSetup>? ExamRollNumberSetup { get; set; }
    public DbSet<ExamSchedule>? ExamSchedules { get; set; }
    public DbSet<ExamSubjectResult>? ExamSubjectResults { get; set; }
    public DbSet<ExamSlot>? ExamSlots { get; set; }
    public DbSet<ExamType>? ExamTypes { get; set; }
    public DbSet<Department>? Departments { get; set; }
    public DbSet<FiscalYear>? FiscalYears { get; set; }
    public DbSet<Gender>? Genders { get; set; }
    public DbSet<IndexGroup>? IndexGroups { get; set; }
    public DbSet<Level>? Levels { get; set; }
    public DbSet<LocalLevel>? LocalLevels { get; set; }
    public DbSet<NepaliDate>? NepaliDates { get; set; }
    public DbSet<Notice>? Notices { get; set; }
    public DbSet<PeriodType>? PeriodTypes { get; set; }
    public DbSet<PreviousLevel>? PreviousLevels { get; set; }
    public DbSet<Program>? Programs { get; set; }
    public DbSet<QuestionSet>? QuestionSets { get; set; }
    public DbSet<ResultRecord>? ResultRecords { get; set; }
    public DbSet<SchoolType>? SchoolTypes { get; set; }
    public DbSet<Semester>? Semesters { get; set; }
    public DbSet<SemesterEnrollment>? SemesterEnrollments { get; set; }
    public DbSet<StudentAdmission>? StudentAdmissions { get; set; }
    public DbSet<StudentCategory>? StudentCategories { get; set; }
    public DbSet<StudentGuardian>? StudentGuardians { get; set; }
    public DbSet<StudentQualification>? StudentQualifications { get; set; }
    public DbSet<StudentRegistration>? StudentRegistrations { get; set; }
    public DbSet<SubjectCatalog>? SubjectCatalogs { get; set; }
    public DbSet<SubjectOffering>? SubjectOfferings { get; set; }
    public DbSet<SubjectType>? SubjectTypes { get; set; }
    public DbSet<CurriculumVersion>? CurriculumVersions { get; set; }
    public DbSet<Province>? Provinces { get; set; }
    public DbSet<Faculty>? Faculties { get; set; }
    public DbSet<Tenant>? Tenants { get; set; }
    public DbSet<SmtpConfiguration>? SmtpConfigurations { get; set; }
    public DbSet<CollegeProfile>? CollegeProfiles { get; set; }
    public DbSet<UserAttachment>? UserAttachments { get; set; }
    public DbSet<GradingScheme>? GradingSchemes { get; set; }
    public DbSet<GradeDefinition>? GradeDefinitions { get; set; }
    public DbSet<EntranceExamApplication>? EntranceExamApplications { get; set; }
    public DbSet<ESewaConfiguration>? ESewaConfigurations { get; set; }
    public DbSet<KhaltiConfiguration>? KhaltiConfigurations { get; set; }
 

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
            var contextField = Expression.Field(Expression.Constant(this), nameof(_tenantContext));
            var tenantIdValue = Expression.Property(contextField, nameof(ITenantContext.TenantId));
            var body = Expression.Equal(tenantIdProp, tenantIdValue);
            var lambda = Expression.Lambda(body, param);
            entityType.SetQueryFilter(lambda);

            builder.Entity(entityType.ClrType)
                .HasOne("Tenant")
                .WithMany()
                .HasForeignKey("TenantId")
                .OnDelete(DeleteBehavior.Restrict);
        }

        builder.Entity<BillTitle>()
            .HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ProgramSubjectPracticalCharge>()
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

        builder.Entity<College>()
            .HasOne(c => c.Faculty)
            .WithMany()
            .HasForeignKey(c => c.FacultyId)
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
            .HasOne(es => es.AcademicYear)
            .WithMany(ay => ay.ExamSchedules)
            .HasForeignKey(es => es.AcademicYearId)
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
            .HasOne(sr => sr.Department)
            .WithMany(d => d.StudentRegistrations)
            .HasForeignKey(sr => sr.DepartmentId)
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
            .ToView("vResultRecords")
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

        builder.Entity<SchoolType>()
            .HasOne(st => st.PreviousLevel)
            .WithMany(pl => pl.SchoolTypes)
            .HasForeignKey(st => st.PreviousLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Program>().ToTable("Programs");

        builder.Entity<SubjectCatalog>()
            .HasIndex(sc => sc.SubjectCode)
            .IsUnique();

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

        builder.Entity<ProgramSubjectPracticalCharge>()
            .HasOne(pspc => pspc.Program)
            .WithMany()
            .HasForeignKey(pspc => pspc.ProgramsId)
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

        builder.Entity<College>()
            .HasOne(c => c.CollegeProfile)
            .WithOne(cp => cp.College)
            .HasForeignKey<CollegeProfile>(cp => cp.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CollegeProfile>()
            .HasOne(cp => cp.BlankChequeUserAttachment)
            .WithMany()
            .HasForeignKey(cp => cp.BlankChequeUserAttachmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CollegeProfile>()
            .HasOne(cp => cp.AuditReportUserAttachment)
            .WithMany()
            .HasForeignKey(cp => cp.AuditReportUserAttachmentId)
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
        builder.Entity<ExamSubjectResult>(e => e.Property(x => x.ObtainedMarks).HasPrecision(5, 2));
        builder.Entity<ExamSubjectResult>(e => e.Property(x => x.ObtainedMarksTheoryInternal).HasPrecision(5, 2));
        builder.Entity<ExamSubjectResult>(e => e.Property(x => x.ObtainedMarksPracticalInternal).HasPrecision(5, 2));
        builder.Entity<BillTitle>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<BillTitle>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<ESewaConfiguration>(e =>
        {
            e.ToTable("ESewaConfiguration");
            e.Property(x => x.ServiceChargeAmount).HasPrecision(18, 2);
        });
        builder.Entity<KhaltiConfiguration>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<PaymentPracticalSubjects>(e => e.Property(x => x.TotalAmount).HasPrecision(18, 2));
        builder.Entity<PaymentRequestLog>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<PeriodType>(e => e.Property(x => x.NumberOfMonths).HasPrecision(5, 2));
        builder.Entity<ProgramSubjectPracticalCharge>(e => e.Property(x => x.PracticalSubjectCharge).HasPrecision(18, 2));
        builder.Entity<StudentQualification>(e => e.Property(x => x.Percentage).HasPrecision(5, 2));

        builder.Entity<BankVoucher>(e => e.Property(x => x.VoucherAmount).HasPrecision(18, 2));
        builder.Entity<GradeDefinition>(e => e.Property(x => x.GradePoint).HasPrecision(5, 2));
        builder.Entity<GradeDefinition>(e => e.Property(x => x.MaxPercentage).HasPrecision(5, 2));
        builder.Entity<GradeDefinition>(e => e.Property(x => x.MinPercentage).HasPrecision(5, 2));

        builder.Entity<EntranceExamApplication>(e => e.Property(x => x.PreviousGPA).HasPrecision(5, 2));

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

        // Unique indexes - Global entities
        builder.Entity<Tenant>()
            .HasIndex(t => t.OfficeCode)
            .IsUnique()
            .HasFilter("[OfficeCode] IS NOT NULL");

        builder.Entity<Faculty>()
            .HasIndex(f => f.OfficeCode)
            .IsUnique()
            .HasFilter("[OfficeCode] IS NOT NULL");

        builder.Entity<Department>()
            .HasIndex(d => d.DepartmentCode)
            .IsUnique();

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
            .HasIndex(b => b.BankCode)
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

        builder.Entity<Board>()
            .HasIndex(b => b.BoardName)
            .IsUnique()
            .HasFilter("[BoardName] IS NOT NULL");

        builder.Entity<EntryFormat>()
            .HasIndex(ef => ef.EntryFormatName)
            .IsUnique()
            .HasFilter("[EntryFormatName] IS NOT NULL");

        builder.Entity<IndexGroup>()
            .HasIndex(ig => ig.IndexGroupName)
            .IsUnique()
            .HasFilter("[IndexGroupName] IS NOT NULL");

        builder.Entity<PeriodType>()
            .HasIndex(pt => pt.PeriodTypeName)
            .IsUnique()
            .HasFilter("[PeriodTypeName] IS NOT NULL");

        builder.Entity<PaymentType>()
            .HasIndex(pt => pt.PaymentTypeName)
            .IsUnique()
            .HasFilter("[PaymentTypeName] IS NOT NULL");

        builder.Entity<SchoolType>()
            .HasIndex(st => st.SchoolTypeName)
            .IsUnique();

        // Unique indexes - Tenant-scoped composite (TenantId, Code)
        builder.Entity<College>()
            .HasIndex(c => new { c.TenantId, c.Code })
            .IsUnique();

        builder.Entity<AcademicYear>()
            .HasIndex(ay => new { ay.TenantId, ay.AcademicYearCode })
            .IsUnique();

        builder.Entity<FiscalYear>()
            .HasIndex(fy => new { fy.TenantId, fy.FiscalYearCode })
            .IsUnique()
            .HasFilter("[FiscalYearCode] IS NOT NULL");

        builder.Entity<ExamSchedule>()
            .HasIndex(es => new { es.TenantId, es.ExamScheduleCode })
            .IsUnique()
            .HasFilter("[ExamScheduleCode] IS NOT NULL");

        builder.Entity<Semester>()
            .HasIndex(s => new { s.TenantId, s.Code })
            .IsUnique()
            .HasFilter("[Code] IS NOT NULL");

        builder.Entity<StudentRegistration>()
            .HasIndex(sr => new { sr.TenantId, sr.RegistrationNumber })
            .IsUnique()
            .HasFilter("[RegistrationNumber] IS NOT NULL");
    }
}
