using fwu_examination_management_system.Data.Auditing;
using fwu_examination_management_system.Data.Models;
using fwu_examination_management_system.Data.Models.Colleges;
using fwu_examination_management_system.Data.Models.Exams;
using fwu_examination_management_system.Data.Models.Location;
using fwu_examination_management_system.Data.Models.Payments;
using fwu_examination_management_system.Data.Models.Semesters;
using fwu_examination_management_system.Data.Models.Students;
using fwu_examination_management_system.Data.Models.Subjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace fwu_examination_management_system.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) 
    : IdentityDbContext<AppUser>(options)
{
    private readonly ILogger<AppDbContext> _logger = logger;

    public DbSet<Organization>? Organizations { get; set; }
    public DbSet<AcademicYear>? AcademicYears { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<Batch> Batches { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<College>? Colleges { get; set; }
    public DbSet<CollegeProfile>? CollegeProfiles { get; set; }
    public DbSet<CollegeProgram>? CollegePrograms { get; set; }
    public DbSet<CollegeType>? CollegeTypes { get; set; }
    public DbSet<ConnectIpsPaymentConfiguration>? ConnectIpsPaymentConfigurations { get; set; }
    public DbSet<District>? Districts { get; set; }
    public DbSet<EntryFormat>? EntryFormats { get; set; }
    public DbSet<ESewaConfiguration>? ESewaConfigurations { get; set; }
    public DbSet<Ethnicity>? Ethnicities { get; set; }
    public DbSet<ExamAttendanceStatus>? ExamAttendanceStatuses { get; set; }
    public DbSet<ExamCenter>? ExamCenters { get; set; }
    public DbSet<ExamCenterDetail>? ExamCenterDetails { get; set; }
    public DbSet<ExamFormFeeName>? ExamFormFeeNames { get; set; }
    public DbSet<ExamFormFeeRate>? ExamFormFeeRates { get; set; }
    public DbSet<ExamRegistration>? ExamRegistrations { get; set; }
    public DbSet<ExamRegistrationActionLog>? ExamRegistrationActionLogs { get; set; }
    public DbSet<ExamRegistrationCenterChange>? ExamRegistrationCenterChanges { get; set; }
    public DbSet<ExamRollNumberSetup>? ExamRollNumberSetups { get; set; }
    public DbSet<ExamRollNumberSetupDetail>? ExamRollNumberSetupDetails { get; set; }
    public DbSet<ExamSchedule>? ExamSchedules { get; set; }
    public DbSet<ExamSubjectRegistration>? ExamSubjectRegistrations { get; set; }
    public DbSet<ExamSubjectRegistrationExamSession>? ExamSubjectRegistrationExamSessions { get; set; }
    public DbSet<ExamSubjectRegistrationInternal>? ExamSubjectRegistrationInternals { get; set; }
    public DbSet<ExamType>? ExamTypes { get; set; }
    public DbSet<Faculty>? Faculties { get; set; }
    public DbSet<FiscalYear>? FiscalYears { get; set; }
    public DbSet<Gender>? Genders { get; set; }
    public DbSet<IndexGroup>? IndexGroups { get; set; }
    public DbSet<KhaltiConfiguration>? KhaltiConfigurations { get; set; }
    public DbSet<Level>? Levels { get; set; }
    public DbSet<LocalLevel>? LocalLevels { get; set; }
    public DbSet<NepaliDate>? NepaliDates { get; set; }
    public DbSet<Notice>? Notices { get; set; }
    public DbSet<PasswordResetLog>? PasswordResetLogs { get; set; }
    public DbSet<PaymentPracticalSubjects>? PaymentPracticalSubjects { get; set; }
    public DbSet<PaymentRequestLog>? PaymentRequestLogs { get; set; }
    public DbSet<PaymentResponseLog>? PaymentResponseLogs { get; set; }
    public DbSet<PaymentType>? PaymentTypes { get; set; }
    public DbSet<PeriodType>? PeriodTypes { get; set; }
    public DbSet<PreferredExamCenter>? PreferredExamCenters { get; set; }
    public DbSet<PreviousLevel>? PreviousLevels { get; set; }
    public DbSet<Program>? Programs { get; set; }
    public DbSet<ProgramSubjectPracticalCharge>? ProgramSubjectPracticalCharges { get; set; }
    public DbSet<QuestionSet>? QuestionSets { get; set; }
    public DbSet<ResultRecord>? ResultRecords { get; set; }
    public DbSet<SchoolType>? SchoolTypes { get; set; }
    public DbSet<Section>? Sections { get; set; }
    public DbSet<Semester>? Semesters { get; set; }
    public DbSet<SemesterEnrollment>? SemesterEnrollments { get; set; }
    public DbSet<SmtpConfiguration>? SmtpConfigurations { get; set; }
    public DbSet<StudentAdmission>? StudentAdmissions { get; set; }
    public DbSet<StudentCategory>? StudentCategories { get; set; }
    public DbSet<StudentGuardian>? StudentGuardians { get; set; }
    public DbSet<SemesterEnrollment>? StudentProgramSemesters { get; set; }
    public DbSet<StudentQualification>? StudentQualifications { get; set; }
    public DbSet<StudentRegistration>? StudentRegistrations { get; set; }
    public DbSet<StudentRegistrationSearch>? StudentRegistrationSearches { get; set; }
    public DbSet<SubjectCatalog>? SubjectCatalogs { get; set; }
    public DbSet<SubjectOffering>? SubjectOfferings { get; set; }
    public DbSet<SubjectTriplicate>? SubjectTriplicates { get; set; }
    public DbSet<SubjectType>? SubjectTypes { get; set; }
    public DbSet<CurriculumVersion>? CurriculumVersions { get; set; }
    public DbSet<UserAttachment>? UserAttachments { get; set; }
    public DbSet<UserProgramMap>? UserProgramMaps { get; set; }
    public DbSet<Province>? Provinces { get; set; }

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

        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<AppUser>().ToTable("Users");

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
            .HasOne(es => es.Level)
            .WithMany(l => l.ExamSchedules)
            .HasForeignKey(es => es.LevelId)
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
            .HasOne(sa => sa.Batch)
            .WithMany(b => b.StudentAdmissions)
            .HasForeignKey(sa => sa.BatchId)
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

        builder.Entity<ExamSubjectRegistration>()
            .HasOne(esr => esr.ExamRegistration)
            .WithMany(er => er.ExamSubjectRegistrations)
            .HasForeignKey(esr => esr.ExamRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSubjectRegistration>()
            .HasOne(esr => esr.ExamType)
            .WithMany()
            .HasForeignKey(esr => esr.ExamTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSubjectRegistration>()
            .HasOne(esr => esr.ExamSubjectRegistrationExamSession)
            .WithOne(esres => esres.ExamSubjectRegistration)
            .HasForeignKey<ExamSubjectRegistrationExamSession>(esres => esres.ExamSubjectRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamSubjectRegistrationInternal>()
            .HasOne(esri => esri.AcademicYear)
            .WithMany()
            .HasForeignKey(esri => esri.EntryAcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);
                
        builder.Entity<ExamCenterDetail>()
            .HasOne(ecd => ecd.ExamCenter)
            .WithMany(ec => ec.ExamCenterDetails)
            .HasForeignKey(ecd => ecd.ExamCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamCenterDetail>()
            .HasOne(ecd => ecd.College)
            .WithMany(c => c.ExamCenterDetails)
            .HasForeignKey(ecd => ecd.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamCenterDetail>()
            .HasOne(ecd => ecd.Program)
            .WithMany()
            .HasForeignKey(ecd => ecd.ProgramsId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRegistrationCenterChange>()
            .HasOne(ercc => ercc.ExamRegistration)
            .WithMany()
            .HasForeignKey(ercc => ercc.ExamRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRegistrationCenterChange>()
            .HasOne(ercc => ercc.PreferredExamCenter)
            .WithMany(pec => pec.ExamRegistrationCenterChanges)
            .HasForeignKey(ercc => ercc.PreferredExamCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRegistrationActionLog>()
            .HasOne(eral => eral.ExamRegistration)
            .WithMany(er => er.ExamRegistrationActionLogs)
            .HasForeignKey(eral => eral.ExamRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRollNumberSetupDetail>()
            .HasOne(ersd => ersd.ExamRollNumberSetup)
            .WithMany(ers => ers.ExamRollNumberSetupDetails)
            .HasForeignKey(ersd => ersd.ExamRollNumberSetupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRollNumberSetupDetail>()
            .HasOne(ersd => ersd.ExamSchedule)
            .WithMany()
            .HasForeignKey(ersd => ersd.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRollNumberSetupDetail>()
            .HasOne(ersd => ersd.ExamType)
            .WithMany()
            .HasForeignKey(ersd => ersd.ExamTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRollNumberSetupDetail>()
            .HasOne(ersd => ersd.College)
            .WithMany()
            .HasForeignKey(ersd => ersd.CollegeId)
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
            .HasOne(sr => sr.Faculty)
            .WithMany(f => f.StudentRegistrations)
            .HasForeignKey(sr => sr.FacultyId)
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
            .HasOne(sr => sr.IndexGroup)
            .WithMany(ig => ig.StudentRegistrations)
            .HasForeignKey(sr => sr.IndexGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.EntryFormat)
            .WithMany(ef => ef.StudentRegistrations)
            .HasForeignKey(sr => sr.EntryFormatId)
            .OnDelete(DeleteBehavior.Restrict);

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

        builder.Entity<ProgramSubjectPracticalCharge>()
            .HasOne(pspc => pspc.Program)
            .WithMany()
            .HasForeignKey(pspc => pspc.ProgramsId)
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

        builder.Entity<UserProgramMap>()
            .HasOne(upm => upm.User)
            .WithMany()
            .HasForeignKey(upm => upm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserProgramMap>()
            .HasOne(upm => upm.Program)
            .WithMany()
            .HasForeignKey(upm => upm.ProgramId)
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

        builder.Entity<BankVoucher>()
            .HasOne(bv => bv.AcademicYear)
            .WithMany()
            .HasForeignKey(bv => bv.AcademicYearId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BankVoucher>()
            .HasOne(bv => bv.College)
            .WithMany(c => c.BankVouchers)
            .HasForeignKey(bv => bv.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BankVoucher>()
            .HasOne(bv => bv.BillTitle)
            .WithMany(bt => bt.BankVouchers)
            .HasForeignKey(bv => bv.BillTitleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BankVoucher>()
            .HasOne(bv => bv.Bank)
            .WithMany(b => b.BankVouchers)
            .HasForeignKey(bv => bv.BankId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BankVoucher>()
            .HasOne(bv => bv.ExamScheduleParent)
            .WithMany(esp => esp.BankVouchers)
            .HasForeignKey(bv => bv.ExamScheduleParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BankVoucher>()
            .HasOne(bv => bv.UserAttachment)
            .WithMany()
            .HasForeignKey(bv => bv.BankVoucherUserAttachmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.PhotoAttachment)
            .WithMany()
            .HasForeignKey(sr => sr.PhotoAttachmentId)
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

        builder.Entity<ExamFormFeeRate>()
            .HasOne(effr => effr.ExamSchedule)
            .WithMany()
            .HasForeignKey(effr => effr.ExamScheduleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamFormFeeRate>()
            .HasOne(effr => effr.ExamFormFeeName)
            .WithMany(effn => effn.ExamFormFeeRates)
            .HasForeignKey(effr => effr.ExamFormFeeNameId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamFormFeeRate>()
            .HasOne(effr => effr.ExamType)
            .WithMany()
            .HasForeignKey(effr => effr.ExamTypeId)
            .OnDelete(DeleteBehavior.Restrict);
                
        builder.Entity<PasswordResetLog>()
            .HasOne(prl => prl.User)
            .WithMany()
            .HasForeignKey(prl => prl.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<UserAttachment>()
            .HasOne(ua => ua.UploadedByUser)
            .WithMany()
            .HasForeignKey(ua => ua.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AppUser>()
            .HasOne(u => u.Organization)
            .WithMany()
            .HasForeignKey(u => u.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AppUser>()
            .HasOne(u => u.StudentRegistration)
            .WithMany(sr => sr.Users)
            .HasForeignKey(u => u.StudentRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<AppUser>()
            .HasOne(u => u.College)
            .WithMany(c => c.Users)
            .HasForeignKey(u => u.CollegeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ExamRegistration>()
            .HasOne(er => er.ApplicationVoucher)
            .WithMany()
            .HasForeignKey(er => er.ApplicationVoucherId)
            .OnDelete(DeleteBehavior.Restrict);

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

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.PermanentAddress)
            .WithMany()
            .HasForeignKey(sr => sr.PermanentAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentRegistration>()
            .HasOne(sr => sr.TemporaryAddress)
            .WithMany()
            .HasForeignKey(sr => sr.TemporaryAddressId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<College>()
            .HasOne(c => c.Address)
            .WithMany()
            .HasForeignKey(c => c.AddressId)
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

        builder.Entity<StudentAdmission>()
            .HasOne(sa => sa.StudentRegistration)
            .WithMany(sr => sr.StudentAdmissions)
            .HasForeignKey(sa => sa.StudentRegistrationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<StudentAdmission>()
            .HasOne(sa => sa.Section)
            .WithMany(s => s.StudentAdmissions)
            .HasForeignKey(sa => sa.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ApplicationVoucher>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<College>(e => e.Property(x => x.AllocatedAmount).HasPrecision(18, 2));
        builder.Entity<ExamFormFeeRate>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<ExamRegistration>(e => e.Property(x => x.AttendancePercentage).HasPrecision(5, 2));
        builder.Entity<ExamRegistration>(e => e.Property(x => x.FeeEnclosed).HasPrecision(18, 2));
        builder.Entity<ExamSchedule>(e => e.Property(x => x.ExtendedDateCharge).HasPrecision(18, 2));
        builder.Entity<ExamSubjectRegistrationExamSession>(e => e.Property(x => x.ObtainedMarks).HasPrecision(5, 2));
        builder.Entity<ExamSubjectRegistrationInternal>(e => e.Property(x => x.ObtainedMarksTheoryInternal).HasPrecision(5, 2));
        builder.Entity<ExamSubjectRegistrationInternal>(e => e.Property(x => x.ObtainedMarksPracticalInternal).HasPrecision(5, 2));
        builder.Entity<BankVoucher>(e => e.Property(x => x.VoucherAmount).HasPrecision(18, 2));
        builder.Entity<BillTitle>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<ESewaConfiguration>(e => e.Property(x => x.ServiceChargeAmount).HasPrecision(18, 2));
        builder.Entity<KhaltiConfiguration>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<PaymentPracticalSubjects>(e => e.Property(x => x.TotalAmount).HasPrecision(18, 2));
        builder.Entity<PaymentRequestLog>(e => e.Property(x => x.Amount).HasPrecision(18, 2));
        builder.Entity<PeriodType>(e => e.Property(x => x.NumberOfMonths).HasPrecision(5, 2));
        builder.Entity<ProgramSubjectPracticalCharge>(e => e.Property(x => x.PracticalSubjectCharge).HasPrecision(18, 2));
        builder.Entity<StudentQualification>(e => e.Property(x => x.Percentage).HasPrecision(5, 2));
    }
}
