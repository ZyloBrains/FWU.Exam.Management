using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using fwu_examination_management_system.Models;

namespace fwu_examination_management_system.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Organization> Organizations { get; set; }

        // DbSets for all entities
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

        public override int SaveChanges()
        {
            NormalizeDateTimesToUtc();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            NormalizeDateTimesToUtc();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            NormalizeDateTimesToUtc();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            NormalizeDateTimesToUtc();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void NormalizeDateTimesToUtc()
        {
            foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified))
            {
                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(DateTime) && property.CurrentValue is DateTime dateTime)
                    {
                        property.CurrentValue = EnsureUtc(dateTime);
                    }
                    else if (property.Metadata.ClrType == typeof(DateTime?) && property.CurrentValue is DateTime nullableDateTime)
                    {
                        property.CurrentValue = EnsureUtc(nullableDateTime);
                    }
                }
            }
        }

        private static DateTime EnsureUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
                _ => value
            };
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships using Fluent API

            // AcademicYear
            builder.Entity<AcademicYear>()
                .HasMany(a => a.Batches)
                .WithOne(b => b.AcademicYear)
                .HasForeignKey(b => b.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AcademicYear>()
                .HasMany(a => a.ExamRegistrations)
                .WithOne(e => e.AcademicYear)
                .HasForeignKey(e => e.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AcademicYear>()
                .HasMany(a => a.ExamSchedules)
                .WithOne(e => e.AcademicYear)
                .HasForeignKey(e => e.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AcademicYear>()
                .HasMany(a => a.StudentRegistrations)
                .WithOne(s => s.AcademicYear)
                .HasForeignKey(s => s.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            // ActiveExamSchedule
            builder.Entity<ActiveExamSchedule>()
                .HasOne(a => a.ExamSchedule)
                .WithMany(e => e.ActiveExamSchedules)
                .HasForeignKey(a => a.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ApplicationVoucher
            builder.Entity<ApplicationVoucher>()
                .HasOne(a => a.ExamSchedule)
                .WithMany(e => e.ApplicationVouchers)
                .HasForeignKey(a => a.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationVoucher>()
                .HasOne(a => a.StudentRegistration)
                .WithMany(s => s.ApplicationVouchers)
                .HasForeignKey(a => a.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Area
            builder.Entity<Area>()
                .HasMany(a => a.Colleges)
                .WithOne(c => c.Area)
                .HasForeignKey(c => c.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Bank
            builder.Entity<Bank>()
                .HasMany(b => b.BankVouchers)
                .WithOne(bv => bv.Bank)
                .HasForeignKey(bv => bv.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            // BankVoucher
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

            // Batch
            builder.Entity<Batch>()
                .HasOne(b => b.AcademicYear)
                .WithMany(a => a.Batches)
                .HasForeignKey(b => b.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Batch>()
                .HasMany(b => b.StudentAdmissions)
                .WithOne(sa => sa.Batch)
                .HasForeignKey(sa => sa.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Batch>()
                .HasMany(b => b.ExamScheduleBatches)
                .WithOne(esb => esb.Batch)
                .HasForeignKey(esb => esb.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            // BillTitle
            builder.Entity<BillTitle>()
                .HasOne(bt => bt.ExamSchedule)
                .WithMany(es => es.BillTitles)
                .HasForeignKey(bt => bt.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BillTitle>()
                .HasMany(bt => bt.BankVouchers)
                .WithOne(bv => bv.BillTitle)
                .HasForeignKey(bv => bv.BillTitleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Board
            builder.Entity<Board>()
                .HasMany(b => b.Programs)
                .WithOne(p => p.Board)
                .HasForeignKey(p => p.BoardId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Board>()
                .HasMany(b => b.StudentQualifications)
                .WithOne(sq => sq.Board)
                .HasForeignKey(sq => sq.BoardId)
                .OnDelete(DeleteBehavior.Restrict);

            // College
            builder.Entity<College>()
                .HasOne(c => c.District)
                .WithMany(d => d.Colleges)
                .HasForeignKey(c => c.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasOne(c => c.CollegeType)
                .WithMany(ct => ct.Colleges)
                .HasForeignKey(c => c.CollegeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasOne(c => c.Area)
                .WithMany(a => a.Colleges)
                .HasForeignKey(c => c.AreaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasOne(c => c.QuestionSet)
                .WithMany(qs => qs.Colleges)
                .HasForeignKey(c => c.QuestionSetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasOne(c => c.CollegeProfile)
                .WithOne(cp => cp.College)
                .HasForeignKey<CollegeProfile>(cp => cp.CollegeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<College>()
                .HasMany(c => c.BankVouchers)
                .WithOne(bv => bv.College)
                .HasForeignKey(bv => bv.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.CollegePrograms)
                .WithOne(cp => cp.College)
                .HasForeignKey(cp => cp.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.ExamCenters)
                .WithOne(ec => ec.College)
                .HasForeignKey(ec => ec.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.ExamCenterDetails)
                .WithOne(ecd => ecd.College)
                .HasForeignKey(ecd => ecd.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.ExamRegistrations)
                .WithOne(er => er.College)
                .HasForeignKey(er => er.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.StudentAdmissions)
                .WithOne(sa => sa.College)
                .HasForeignKey(sa => sa.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.StudentRegistrations)
                .WithOne(sr => sr.College)
                .HasForeignKey(sr => sr.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<College>()
                .HasMany(c => c.Users)
                .WithOne(u => u.College)
                .HasForeignKey(u => u.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            // CollegeProfile
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

            // CollegeProgram
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

            // CollegeType
            builder.Entity<CollegeType>()
                .HasMany(ct => ct.Colleges)
                .WithOne(c => c.CollegeType)
                .HasForeignKey(c => c.CollegeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CollegeType>()
                .HasMany(ct => ct.ExamFormFeeRates)
                .WithOne(efr => efr.CollegeType)
                .HasForeignKey(efr => efr.CollegeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // District
            builder.Entity<District>()
                .HasOne(d => d.Province)
                .WithMany(p => p.Districts)
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<District>()
                .HasMany(d => d.Colleges)
                .WithOne(c => c.District)
                .HasForeignKey(c => c.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<District>()
                .HasMany(d => d.LocalLevels)
                .WithOne(ll => ll.District)
                .HasForeignKey(ll => ll.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<District>()
                .HasMany(d => d.StudentRegistrations)
                .WithOne(sr => sr.District)
                .HasForeignKey(sr => sr.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            // EntryFormat
            builder.Entity<EntryFormat>()
                .HasMany(ef => ef.StudentRegistrations)
                .WithOne(sr => sr.EntryFormat)
                .HasForeignKey(sr => sr.EntryFormatId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ethnicity
            builder.Entity<Ethnicity>()
                .HasMany(e => e.StudentRegistrations)
                .WithOne(sr => sr.Ethnicity)
                .HasForeignKey(sr => sr.EthnicityId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamCenter
            builder.Entity<ExamCenter>()
                .HasOne(ec => ec.ExamSchedule)
                .WithMany(es => es.ExamCenters)
                .HasForeignKey(ec => ec.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamCenter>()
                .HasOne(ec => ec.College)
                .WithMany(c => c.ExamCenters)
                .HasForeignKey(ec => ec.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamCenter>()
                .HasMany(ec => ec.ExamCenterDetails)
                .WithOne(ecd => ecd.ExamCenter)
                .HasForeignKey(ecd => ecd.ExamCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamCenter>()
                .HasMany(ec => ec.ExamRegistrations)
                .WithOne(er => er.ExamCenter)
                .HasForeignKey(er => er.ExamCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamCenterDetail
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
                .HasForeignKey(ecd => ecd.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamFormFeeName
            builder.Entity<ExamFormFeeName>()
                .HasMany(efn => efn.ExamFormFeeRates)
                .WithOne(efr => efr.ExamFormFeeName)
                .HasForeignKey(efr => efr.ExamFormFeeNameId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamFormFeeRate
            builder.Entity<ExamFormFeeRate>()
                .HasOne(efr => efr.ExamSchedule)
                .WithMany(es => es.ExamFormFeeRates)
                .HasForeignKey(efr => efr.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamFormFeeRate>()
                .HasOne(efr => efr.ExamFormFeeName)
                .WithMany(efn => efn.ExamFormFeeRates)
                .HasForeignKey(efr => efr.ExamFormFeeNameId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamFormFeeRate>()
                .HasOne(efr => efr.CollegeType)
                .WithMany(ct => ct.ExamFormFeeRates)
                .HasForeignKey(efr => efr.CollegeTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamFormFeeRate>()
                .HasOne(efr => efr.ExamType)
                .WithMany(et => et.ExamFormFeeRates)
                .HasForeignKey(efr => efr.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamRegistration
            builder.Entity<ExamRegistration>()
                .HasOne(er => er.StudentProgramYearPart)
                .WithMany(spyp => spyp.ExamRegistrations)
                .HasForeignKey(er => er.StudentProgramYearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRegistration>()
                .HasOne(er => er.AcademicYear)
                .WithMany(ay => ay.ExamRegistrations)
                .HasForeignKey(er => er.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRegistration>()
                .HasOne(er => er.ExamCenter)
                .WithMany(ec => ec.ExamRegistrations)
                .HasForeignKey(er => er.ExamCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRegistration>()
                .HasOne(er => er.College)
                .WithMany(c => c.ExamRegistrations)
                .HasForeignKey(er => er.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRegistration>()
                .HasOne(er => er.ExamSchedule)
                .WithMany(es => es.ExamRegistrations)
                .HasForeignKey(er => er.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRegistration>()
                .HasOne(er => er.Program)
                .WithMany(p => p.ExamRegistrations)
                .HasForeignKey(er => er.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRegistration>()
                .HasOne(er => er.ApplicationVoucher)
                .WithMany()
                .HasForeignKey(er => er.ApplicationVoucherId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRegistration>()
                .HasMany(er => er.ExamSubjectRegistrations)
                .WithOne(esr => esr.ExamRegistration)
                .HasForeignKey(esr => esr.ExamRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRegistration>()
                .HasMany(er => er.ExamRegistrationActionLogs)
                .WithOne(eral => eral.ExamRegistration)
                .HasForeignKey(eral => eral.ExamRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamRegistrationActionLog
            builder.Entity<ExamRegistrationActionLog>()
                .HasOne(eral => eral.ExamRegistration)
                .WithMany(er => er.ExamRegistrationActionLogs)
                .HasForeignKey(eral => eral.ExamRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamRegistrationCenterChange
            builder.Entity<ExamRegistrationCenterChange>()
                .HasOne(ercc => ercc.ExamRegistration)
                .WithOne()
                .HasForeignKey<ExamRegistrationCenterChange>(ercc => ercc.ExamRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRegistrationCenterChange>()
                .HasOne(ercc => ercc.PreferredExamCenter)
                .WithMany(pec => pec.ExamRegistrationCenterChanges)
                .HasForeignKey(ercc => ercc.PreferredExamCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamRollNumberSetup
            builder.Entity<ExamRollNumberSetup>()
                .HasOne(erns => erns.ExamScheduleParent)
                .WithMany(esp => esp.ExamRollNumberSetups)
                .HasForeignKey(erns => erns.ExamScheduleParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRollNumberSetup>()
                .HasMany(erns => erns.ExamRollNumberSetupDetails)
                .WithOne(ernsd => ernsd.ExamRollNumberSetup)
                .HasForeignKey(ernsd => ernsd.ExamRollNumberSetupId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamRollNumberSetupDetail
            builder.Entity<ExamRollNumberSetupDetail>()
                .HasOne(ernsd => ernsd.ExamRollNumberSetup)
                .WithMany(erns => erns.ExamRollNumberSetupDetails)
                .HasForeignKey(ernsd => ernsd.ExamRollNumberSetupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRollNumberSetupDetail>()
                .HasOne(ernsd => ernsd.ExamSchedule)
                .WithMany()
                .HasForeignKey(ernsd => ernsd.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRollNumberSetupDetail>()
                .HasOne(ernsd => ernsd.Program)
                .WithMany(p => p.ExamRollNumberSetupDetails)
                .HasForeignKey(ernsd => ernsd.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRollNumberSetupDetail>()
                .HasOne(ernsd => ernsd.ExamType)
                .WithMany()
                .HasForeignKey(ernsd => ernsd.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamRollNumberSetupDetail>()
                .HasOne(ernsd => ernsd.College)
                .WithMany()
                .HasForeignKey(ernsd => ernsd.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamSchedule
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
                .HasOne(es => es.YearPart)
                .WithMany(yp => yp.ExamSchedules)
                .HasForeignKey(es => es.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasOne(es => es.ExamType)
                .WithMany(et => et.ExamSchedules)
                .HasForeignKey(es => es.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasOne(es => es.ExamScheduleParent)
                .WithMany(esp => esp.ExamSchedules)
                .HasForeignKey(es => es.ExamScheduleParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasMany(es => es.ActiveExamSchedules)
                .WithOne(aes => aes.ExamSchedule)
                .HasForeignKey(aes => aes.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasMany(es => es.ApplicationVouchers)
                .WithOne(av => av.ExamSchedule)
                .HasForeignKey(av => av.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasMany(es => es.BillTitles)
                .WithOne(bt => bt.ExamSchedule)
                .HasForeignKey(bt => bt.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasMany(es => es.ExamCenters)
                .WithOne(ec => ec.ExamSchedule)
                .HasForeignKey(ec => ec.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasMany(es => es.ExamFormFeeRates)
                .WithOne(efr => efr.ExamSchedule)
                .HasForeignKey(efr => efr.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasMany(es => es.ExamRegistrations)
                .WithOne(er => er.ExamSchedule)
                .HasForeignKey(er => er.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasMany(es => es.ExamScheduleBatches)
                .WithOne(esb => esb.ExamSchedule)
                .HasForeignKey(esb => esb.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasMany(es => es.ExamScheduleDetails)
                .WithOne(esd => esd.ExamSchedule)
                .HasForeignKey(esd => esd.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSchedule>()
                .HasMany(es => es.PaymentRequestLogs)
                .WithOne(prl => prl.ExamSchedule)
                .HasForeignKey(prl => prl.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamScheduleBatch
            builder.Entity<ExamScheduleBatch>()
                .HasOne(esb => esb.ExamSchedule)
                .WithMany(es => es.ExamScheduleBatches)
                .HasForeignKey(esb => esb.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamScheduleBatch>()
                .HasOne(esb => esb.ExamType)
                .WithMany(et => et.ExamScheduleBatches)
                .HasForeignKey(esb => esb.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamScheduleBatch>()
                .HasOne(esb => esb.Batch)
                .WithMany(b => b.ExamScheduleBatches)
                .HasForeignKey(esb => esb.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamScheduleDetail
            builder.Entity<ExamScheduleDetail>()
                .HasOne(esd => esd.ExamSchedule)
                .WithMany(es => es.ExamScheduleDetails)
                .HasForeignKey(esd => esd.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamScheduleDetail>()
                .HasOne(esd => esd.ExamType)
                .WithMany(et => et.ExamScheduleDetails)
                .HasForeignKey(esd => esd.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamScheduleDetail>()
                .HasOne(esd => esd.SubjectDetail)
                .WithMany(sd => sd.ExamScheduleDetails)
                .HasForeignKey(esd => esd.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamScheduleParent
            builder.Entity<ExamScheduleParent>()
                .HasMany(esp => esp.BankVouchers)
                .WithOne(bv => bv.ExamScheduleParent)
                .HasForeignKey(bv => bv.ExamScheduleParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamScheduleParent>()
                .HasMany(esp => esp.ExamRollNumberSetups)
                .WithOne(erns => erns.ExamScheduleParent)
                .HasForeignKey(erns => erns.ExamScheduleParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamScheduleParent>()
                .HasMany(esp => esp.ExamSchedules)
                .WithOne(es => es.ExamScheduleParent)
                .HasForeignKey(es => es.ExamScheduleParentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamSubjectRegistration
            builder.Entity<ExamSubjectRegistration>()
                .HasOne(esr => esr.ExamRegistration)
                .WithMany(er => er.ExamSubjectRegistrations)
                .HasForeignKey(esr => esr.ExamRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSubjectRegistration>()
                .HasOne(esr => esr.SubjectDetail)
                .WithMany(sd => sd.ExamSubjectRegistrations)
                .HasForeignKey(esr => esr.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSubjectRegistration>()
                .HasOne(esr => esr.ExamType)
                .WithMany(et => et.ExamSubjectRegistrations)
                .HasForeignKey(esr => esr.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSubjectRegistration>()
                .HasOne(esr => esr.ExamSubjectRegistrationExamSession)
                .WithOne(esres => esres.ExamSubjectRegistration)
                .HasForeignKey<ExamSubjectRegistrationExamSession>(esres => esres.ExamSubjectRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExamSubjectRegistrationExamSession
            builder.Entity<ExamSubjectRegistrationExamSession>()
                .HasOne(esres => esres.ExamSubjectRegistration)
                .WithOne(esr => esr.ExamSubjectRegistrationExamSession)
                .HasForeignKey<ExamSubjectRegistrationExamSession>(esres => esres.ExamSubjectRegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ExamSubjectRegistrationInternal
            builder.Entity<ExamSubjectRegistrationInternal>()
                .HasOne(esri => esri.AcademicYear)
                .WithMany()
                .HasForeignKey(esri => esri.EntryAcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSubjectRegistrationInternal>()
                .HasOne(esri => esri.StudentProgramYearPart)
                .WithMany(spyp => spyp.ExamSubjectRegistrationInternals)
                .HasForeignKey(esri => esri.StudentProgramYearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSubjectRegistrationInternal>()
                .HasOne(esri => esri.SubjectDetail)
                .WithMany(sd => sd.ExamSubjectRegistrationInternals)
                .HasForeignKey(esri => esri.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamSubjectRegistrationInternal>()
                .HasOne(esri => esri.ExamSchedule)
                .WithMany()
                .HasForeignKey(esri => esri.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ExamType
            builder.Entity<ExamType>()
                .HasMany(et => et.ExamFormFeeRates)
                .WithOne(efr => efr.ExamType)
                .HasForeignKey(efr => efr.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamType>()
                .HasMany(et => et.ExamSchedules)
                .WithOne(es => es.ExamType)
                .HasForeignKey(es => es.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamType>()
                .HasMany(et => et.ExamScheduleBatches)
                .WithOne(esb => esb.ExamType)
                .HasForeignKey(esb => esb.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamType>()
                .HasMany(et => et.ExamScheduleDetails)
                .WithOne(esd => esd.ExamType)
                .HasForeignKey(esd => esd.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ExamType>()
                .HasMany(et => et.ExamSubjectRegistrations)
                .WithOne(esr => esr.ExamType)
                .HasForeignKey(esr => esr.ExamTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Faculty
            builder.Entity<Faculty>()
                .HasMany(f => f.Programs)
                .WithOne(p => p.Faculty)
                .HasForeignKey(p => p.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Faculty>()
                .HasMany(f => f.StudentRegistrations)
                .WithOne(sr => sr.Faculty)
                .HasForeignKey(sr => sr.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Gender
            builder.Entity<Gender>()
                .HasMany(g => g.StudentRegistrations)
                .WithOne(sr => sr.Gender)
                .HasForeignKey(sr => sr.GenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // IndexGroup
            builder.Entity<IndexGroup>()
                .HasMany(ig => ig.StudentRegistrations)
                .WithOne(sr => sr.IndexGroup)
                .HasForeignKey(sr => sr.IndexGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // Level
            builder.Entity<Level>()
                .HasMany(l => l.ExamSchedules)
                .WithOne(es => es.Level)
                .HasForeignKey(es => es.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Level>()
                .HasMany(l => l.Programs)
                .WithOne(p => p.Level)
                .HasForeignKey(p => p.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Level>()
                .HasMany(l => l.StudentRegistrations)
                .WithOne(sr => sr.Level)
                .HasForeignKey(sr => sr.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // LocalLevel
            builder.Entity<LocalLevel>()
                .HasOne(ll => ll.District)
                .WithMany(d => d.LocalLevels)
                .HasForeignKey(ll => ll.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LocalLevel>()
                .HasMany(ll => ll.StudentRegistrations)
                .WithOne(sr => sr.LocalLevel)
                .HasForeignKey(sr => sr.LocalLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // PasswordResetLog
            builder.Entity<PasswordResetLog>()
                .HasOne(prl => prl.User)
                .WithMany(u => u.PasswordResetLogs)
                .HasForeignKey(prl => prl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentPracticalSubjects
            builder.Entity<PaymentPracticalSubjects>()
                .HasOne(pps => pps.PaymentRequestLog)
                .WithMany(prl => prl.PaymentPracticalSubjects)
                .HasForeignKey(pps => pps.PaymentRequestLogId)
                .OnDelete(DeleteBehavior.Restrict);

            // PaymentRequestLog
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
                .HasOne(prl => prl.ExamSchedule)
                .WithMany(es => es.PaymentRequestLogs)
                .HasForeignKey(prl => prl.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PaymentRequestLog>()
                .HasOne(prl => prl.College)
                .WithMany()
                .HasForeignKey(prl => prl.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PaymentRequestLog>()
                .HasOne(prl => prl.PaymentResponseLog)
                .WithOne(prl => prl.PaymentRequestLog)
                .HasForeignKey<PaymentResponseLog>(prl => prl.PaymentRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // PaymentResponseLog
            builder.Entity<PaymentResponseLog>()
                .HasOne(prl => prl.PaymentRequestLog)
                .WithOne(prl => prl.PaymentResponseLog)
                .HasForeignKey<PaymentResponseLog>(prl => prl.PaymentRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // PaymentType
            builder.Entity<PaymentType>()
                .HasMany(pt => pt.PaymentRequestLogs)
                .WithOne(prl => prl.PaymentType)
                .HasForeignKey(prl => prl.PaymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // PreferredExamCenter
            builder.Entity<PreferredExamCenter>()
                .HasOne(pec => pec.College)
                .WithMany()
                .HasForeignKey(pec => pec.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PreferredExamCenter>()
                .HasMany(pec => pec.ExamRegistrationCenterChanges)
                .WithOne(ercc => ercc.PreferredExamCenter)
                .HasForeignKey(ercc => ercc.PreferredExamCenterId)
                .OnDelete(DeleteBehavior.Restrict);

            // PreviousLevel
            builder.Entity<PreviousLevel>()
                .HasOne(pl => pl.Level)
                .WithMany()
                .HasForeignKey(pl => pl.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PreviousLevel>()
                .HasMany(pl => pl.SchoolTypes)
                .WithOne(st => st.PreviousLevel)
                .HasForeignKey(st => st.PreviousLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PreviousLevel>()
                .HasMany(pl => pl.StudentQualifications)
                .WithOne(sq => sq.PreviousLevel)
                .HasForeignKey(sq => sq.PreviousLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Program
            builder.Entity<Programs>()
                .HasOne(p => p.Level)
                .WithMany(l => l.Programs)
                .HasForeignKey(p => p.LevelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasOne(p => p.Faculty)
                .WithMany(f => f.Programs)
                .HasForeignKey(p => p.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasOne(p => p.Board)
                .WithMany(b => b.Programs)
                .HasForeignKey(p => p.BoardId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasOne(p => p.ProgramPeriodType)
                .WithMany(ppt => ppt.Programs)
                .HasForeignKey(p => p.ProgramPeriodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.CollegePrograms)
                .WithOne(cp => cp.Program)
                .HasForeignKey(cp => cp.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.ExamRegistrations)
                .WithOne(er => er.Program)
                .HasForeignKey(er => er.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.ExamRollNumberSetupDetails)
                .WithOne(ernsd => ernsd.Program)
                .HasForeignKey(ernsd => ernsd.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.ProgramSubjectPracticalCharges)
                .WithOne(pspc => pspc.Program)
                .HasForeignKey(pspc => pspc.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.ProgramYearParts)
                .WithOne(pyp => pyp.Program)
                .HasForeignKey(pyp => pyp.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.StudentAdmissions)
                .WithOne(sa => sa.Program)
                .HasForeignKey(sa => sa.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.SubjectBatches)
                .WithOne(sb => sb.Program)
                .HasForeignKey(sb => sb.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.SubjectDetails)
                .WithOne(sd => sd.Program)
                .HasForeignKey(sd => sd.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.SubjectGroups)
                .WithOne(sg => sg.Program)
                .HasForeignKey(sg => sg.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Programs>()
                .HasMany(p => p.UserProgramMaps)
                .WithOne(upm => upm.Program)
                .HasForeignKey(upm => upm.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProgramPeriodType
            builder.Entity<ProgramPeriodType>()
                .HasMany(ppt => ppt.Programs)
                .WithOne(p => p.ProgramPeriodType)
                .HasForeignKey(p => p.ProgramPeriodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProgramPeriodType>()
                .HasMany(ppt => ppt.YearParts)
                .WithOne(yp => yp.ProgramPeriodType)
                .HasForeignKey(yp => yp.ProgramPeriodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProgramSubjectPracticalCharge
            builder.Entity<ProgramSubjectPracticalCharge>()
                .HasOne(pspc => pspc.Program)
                .WithMany(p => p.ProgramSubjectPracticalCharges)
                .HasForeignKey(pspc => pspc.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            // ProgramYearPart
            builder.Entity<ProgramYearPart>()
                .HasOne(pyp => pyp.Program)
                .WithMany(p => p.ProgramYearParts)
                .HasForeignKey(pyp => pyp.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProgramYearPart>()
                .HasOne(pyp => pyp.YearPart)
                .WithMany(yp => yp.ProgramYearParts)
                .HasForeignKey(pyp => pyp.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            // Province
            builder.Entity<Province>()
                .HasMany(p => p.Districts)
                .WithOne(d => d.Province)
                .HasForeignKey(d => d.ProvinceId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuestionSet
            builder.Entity<QuestionSet>()
                .HasMany(qs => qs.Colleges)
                .WithOne(c => c.QuestionSet)
                .HasForeignKey(c => c.QuestionSetId)
                .OnDelete(DeleteBehavior.Restrict);

            // ResultRecord
            builder.Entity<ResultRecord>()
                .HasOne(rr => rr.AcademicYear)
                .WithMany()
                .HasForeignKey(rr => rr.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ResultRecord>()
                .HasOne(rr => rr.Program)
                .WithMany()
                .HasForeignKey(rr => rr.ProgramId)
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
                .HasOne(rr => rr.SubjectDetail)
                .WithMany(sd => sd.ResultRecords)
                .HasForeignKey(rr => rr.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ResultRecord>()
                .HasOne(rr => rr.ExamSchedule)
                .WithMany()
                .HasForeignKey(rr => rr.ExamScheduleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Role
          

            // SchoolType
            builder.Entity<SchoolType>()
                .HasOne(st => st.PreviousLevel)
                .WithMany(pl => pl.SchoolTypes)
                .HasForeignKey(st => st.PreviousLevelId)
                .OnDelete(DeleteBehavior.Restrict);

            // Section
            builder.Entity<Section>()
                .HasOne(s => s.Program)
                .WithMany()
                .HasForeignKey(s => s.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Section>()
                .HasOne(s => s.Batch)
                .WithMany()
                .HasForeignKey(s => s.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Section>()
                .HasMany(s => s.StudentAdmissions)
                .WithOne(sa => sa.Section)
                .HasForeignKey(sa => sa.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentAdmission
            builder.Entity<StudentAdmission>()
                .HasOne(sa => sa.Batch)
                .WithMany(b => b.StudentAdmissions)
                .HasForeignKey(sa => sa.BatchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAdmission>()
                .HasOne(sa => sa.StudentRegistration)
                .WithMany(sr => sr.StudentAdmissions)
                .HasForeignKey(sa => sa.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAdmission>()
                .HasOne(sa => sa.Program)
                .WithMany(p => p.StudentAdmissions)
                .HasForeignKey(sa => sa.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAdmission>()
                .HasOne(sa => sa.College)
                .WithMany(c => c.StudentAdmissions)
                .HasForeignKey(sa => sa.CollegeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAdmission>()
                .HasOne(sa => sa.Section)
                .WithMany(s => s.StudentAdmissions)
                .HasForeignKey(sa => sa.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAdmission>()
                .HasOne(sa => sa.SubjectGroup)
                .WithMany(sg => sg.StudentAdmissions)
                .HasForeignKey(sa => sa.SubjectGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentAdmission>()
                .HasMany(sa => sa.StudentProgramYearParts)
                .WithOne(spyp => spyp.StudentAdmission)
                .HasForeignKey(spyp => spyp.StudentAdmissionId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentCategory
            builder.Entity<StudentCategory>()
                .HasMany(sc => sc.StudentRegistrations)
                .WithOne(sr => sr.StudentCategory)
                .HasForeignKey(sr => sr.StudentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentGuardian
            builder.Entity<StudentGuardian>()
                .HasOne(sg => sg.StudentRegistration)
                .WithMany(sr => sr.StudentGuardians)
                .HasForeignKey(sg => sg.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentProgramYearPart
            builder.Entity<StudentProgramYearPart>()
                .HasOne(spyp => spyp.StudentAdmission)
                .WithMany(sa => sa.StudentProgramYearParts)
                .HasForeignKey(spyp => spyp.StudentAdmissionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentProgramYearPart>()
                .HasOne(spyp => spyp.AcademicYear)
                .WithMany()
                .HasForeignKey(spyp => spyp.AcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentProgramYearPart>()
                .HasOne(spyp => spyp.YearPart)
                .WithMany(yp => yp.StudentProgramYearParts)
                .HasForeignKey(spyp => spyp.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentProgramYearPart>()
                .HasMany(spyp => spyp.ExamRegistrations)
                .WithOne(er => er.StudentProgramYearPart)
                .HasForeignKey(er => er.StudentProgramYearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentProgramYearPart>()
                .HasMany(spyp => spyp.ExamSubjectRegistrationInternals)
                .WithOne(esri => esri.StudentProgramYearPart)
                .HasForeignKey(esri => esri.StudentProgramYearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentQualification
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

            // StudentRegistration
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
                .HasOne(sr => sr.District)
                .WithMany(d => d.StudentRegistrations)
                .HasForeignKey(sr => sr.DistrictId)
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
                .HasOne(sr => sr.LocalLevel)
                .WithMany(ll => ll.StudentRegistrations)
                .HasForeignKey(sr => sr.LocalLevelId)
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

            builder.Entity<StudentRegistration>()
                .HasOne(sr => sr.PhotoAttachment)
                .WithMany()
                .HasForeignKey(sr => sr.PhotoAttachmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentRegistration>()
                .HasOne(sr => sr.StudentRegistrationSearch)
                .WithMany(srs => srs.StudentRegistrations)
                .HasForeignKey(sr => sr.StudentRegistrationSearchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentRegistration>()
                .HasMany(sr => sr.ApplicationVouchers)
                .WithOne(av => av.StudentRegistration)
                .HasForeignKey(av => av.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentRegistration>()
                .HasMany(sr => sr.PaymentRequestLogs)
                .WithOne(prl => prl.StudentRegistration)
                .HasForeignKey(prl => prl.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentRegistration>()
                .HasMany(sr => sr.StudentAdmissions)
                .WithOne(sa => sa.StudentRegistration)
                .HasForeignKey(sa => sa.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentRegistration>()
                .HasMany(sr => sr.StudentGuardians)
                .WithOne(sg => sg.StudentRegistration)
                .HasForeignKey(sg => sg.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentRegistration>()
                .HasMany(sr => sr.StudentQualifications)
                .WithOne(sq => sq.StudentRegistration)
                .HasForeignKey(sq => sq.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentRegistration>()
                .HasMany(sr => sr.Users)
                .WithOne(u => u.StudentRegistration)
                .HasForeignKey(u => u.StudentRegistrationId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentRegistrationSearch
            builder.Entity<StudentRegistrationSearch>()
                .HasOne(srs => srs.User)
                .WithMany()
                .HasForeignKey(srs => srs.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<StudentRegistrationSearch>()
                .HasMany(srs => srs.StudentRegistrations)
                .WithOne(sr => sr.StudentRegistrationSearch)
                .HasForeignKey(sr => sr.StudentRegistrationSearchId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubjectBatch
            builder.Entity<SubjectBatch>()
                .HasOne(sb => sb.AcademicYear)
                .WithMany()
                .HasForeignKey(sb => sb.EffectiveAcademicYearId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectBatch>()
                .HasOne(sb => sb.Program)
                .WithMany(p => p.SubjectBatches)
                .HasForeignKey(sb => sb.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubjectDetail
            builder.Entity<SubjectDetail>()
                .HasOne(sd => sd.SubjectGroup)
                .WithMany(sg => sg.SubjectDetails)
                .HasForeignKey(sd => sd.SubjectGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectDetail>()
                .HasOne(sd => sd.Program)
                .WithMany(p => p.SubjectDetails)
                .HasForeignKey(sd => sd.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectDetail>()
                .HasOne(sd => sd.YearPart)
                .WithMany(yp => yp.SubjectDetails)
                .HasForeignKey(sd => sd.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectDetail>()
                .HasOne(sd => sd.SubjectType)
                .WithMany(st => st.SubjectDetails)
                .HasForeignKey(sd => sd.SubjectTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectDetail>()
                .HasMany(sd => sd.ExamScheduleDetails)
                .WithOne(esd => esd.SubjectDetail)
                .HasForeignKey(esd => esd.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectDetail>()
                .HasMany(sd => sd.ExamSubjectRegistrations)
                .WithOne(esr => esr.SubjectDetail)
                .HasForeignKey(esr => esr.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectDetail>()
                .HasMany(sd => sd.ExamSubjectRegistrationInternals)
                .WithOne(esri => esri.SubjectDetail)
                .HasForeignKey(esri => esri.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectDetail>()
                .HasMany(sd => sd.ResultRecords)
                .WithOne(rr => rr.SubjectDetail)
                .HasForeignKey(rr => rr.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectDetail>()
                .HasMany(sd => sd.SubjectGroupDetailMaps)
                .WithOne(sgdm => sgdm.SubjectDetail)
                .HasForeignKey(sgdm => sgdm.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubjectGroup
            builder.Entity<SubjectGroup>()
                .HasOne(sg => sg.Program)
                .WithMany(p => p.SubjectGroups)
                .HasForeignKey(sg => sg.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectGroup>()
                .HasOne(sg => sg.YearPart)
                .WithMany(yp => yp.SubjectGroups)
                .HasForeignKey(sg => sg.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectGroup>()
                .HasMany(sg => sg.StudentAdmissions)
                .WithOne(sa => sa.SubjectGroup)
                .HasForeignKey(sa => sa.SubjectGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectGroup>()
                .HasMany(sg => sg.SubjectDetails)
                .WithOne(sd => sd.SubjectGroup)
                .HasForeignKey(sd => sd.SubjectGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectGroup>()
                .HasMany(sg => sg.SubjectGroupDetailMaps)
                .WithOne(sgdm => sgdm.SubjectGroup)
                .HasForeignKey(sgdm => sgdm.SubjectGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubjectGroupDetailMap (composite primary key)
            builder.Entity<SubjectGroupDetailMap>()
                .HasKey(sgdm => new { sgdm.SubjectGroupId, sgdm.SubjectDetailId });

            builder.Entity<SubjectGroupDetailMap>()
                .HasOne(sgdm => sgdm.SubjectGroup)
                .WithMany(sg => sg.SubjectGroupDetailMaps)
                .HasForeignKey(sgdm => sgdm.SubjectGroupId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<SubjectGroupDetailMap>()
                .HasOne(sgdm => sgdm.SubjectDetail)
                .WithMany(sd => sd.SubjectGroupDetailMaps)
                .HasForeignKey(sgdm => sgdm.SubjectDetailId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubjectType
            builder.Entity<SubjectType>()
                .HasMany(st => st.SubjectDetails)
                .WithOne(sd => sd.SubjectType)
                .HasForeignKey(sd => sd.SubjectTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // User
          

            // UserAttachment
            builder.Entity<UserAttachment>()
                .HasOne(ua => ua.UploadedByUser)
                .WithMany()
                .HasForeignKey(ua => ua.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // UserProgramMap
            builder.Entity<UserProgramMap>()
                .HasOne(upm => upm.User)
                .WithMany(u => u.UserProgramMaps)
                .HasForeignKey(upm => upm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserProgramMap>()
                .HasOne(upm => upm.Program)
                .WithMany(p => p.UserProgramMaps)
                .HasForeignKey(upm => upm.ProgramId)
                .OnDelete(DeleteBehavior.Restrict);

            // YearPart
            builder.Entity<YearPart>()
                .HasOne(yp => yp.ProgramPeriodType)
                .WithMany(ppt => ppt.YearParts)
                .HasForeignKey(yp => yp.ProgramPeriodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<YearPart>()
                .HasMany(yp => yp.ExamSchedules)
                .WithOne(es => es.YearPart)
                .HasForeignKey(es => es.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<YearPart>()
                .HasMany(yp => yp.ProgramYearParts)
                .WithOne(pyp => pyp.YearPart)
                .HasForeignKey(pyp => pyp.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<YearPart>()
                .HasMany(yp => yp.StudentProgramYearParts)
                .WithOne(spyp => spyp.YearPart)
                .HasForeignKey(spyp => spyp.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<YearPart>()
                .HasMany(yp => yp.SubjectDetails)
                .WithOne(sd => sd.YearPart)
                .HasForeignKey(sd => sd.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<YearPart>()
                .HasMany(yp => yp.SubjectGroups)
                .WithOne(sg => sg.YearPart)
                .HasForeignKey(sg => sg.YearPartId)
                .OnDelete(DeleteBehavior.Restrict);

            // Removed problematic explicit primary key configuration:
            // The original line:
            //     builder.Entity<Program>().HasKey(p => p.Id);
            // failed to compile because the resolved `Program` type did not contain an `Id` member.
            // Let EF Core infer the key from the actual `Program` entity, or re-add an explicit key
            // using the correct property name (for example: HasKey(p => p.ProgramId)) if your model uses
            // a different key property.
            //
            // If you must keep an explicit configuration but cannot reference the CLR type here due to
            // ambiguity, you can use the string-based API:
            //     builder.Entity("Program").HasKey("Id");
            //
            // Only reintroduce the explicit key once you confirm the correct property name on the model.

            // Map Identity table names to the names used in migrations/snapshot
            //builder.Entity<Program>().ToTable("Program");
            builder.Entity<AppUser>().ToTable("Users");
            builder.Entity<IdentityRole>().ToTable("Roles");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
        }
    }
}
