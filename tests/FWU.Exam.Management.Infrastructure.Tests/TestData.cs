using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure.Data.Models;

namespace FWU.Exam.Management.Infrastructure.Tests;

public static class TestData
{
    public const int TenantId = 1;
    public const int CollegeId = 1;
    public const int LevelId = 1;
    public const int AcademicYearId = 1;
    public const int ProgramId = 1;
    public const int ProgramIdOther = 2;

    public const int Regular = 1;
    public const int Partial = 2;
    public const int Supplementary = 3;
    public const int Entrance = 4;

    public static void SeedBase(AppDbContext ctx)
    {
        ctx.Tenants.Add(new Tenant
        {
            Id = TenantId,
            Name = "Test College",
            OfficeCode = "TST",
            ContactNumber = "000",
            Address = "Kathmandu",
            Email = "t@t.com",
            TenantType = TenantType.Standard,
            IsActive = true
        });

        ctx.CollegeTypes.Add(new CollegeType { Id = 1, Code = "UNI", Name = "University", IsActive = true });
        ctx.Colleges.Add(new College
        {
            Id = CollegeId,
            Code = "CLG",
            Name = "Test College",
            Email = "c@c.com",
            PrincipalName = "Principal",
            PrincipalContactNumber = "000",
            CollegeTypeId = 1,
            IsActive = true
        });

        ctx.Levels.Add(new Level { Id = LevelId, LevelCode = "B", LevelName = "Bachelor", IsActive = true });
        ctx.AcademicYears.Add(new AcademicYear
        {
            Id = AcademicYearId,
            TenantId = TenantId,
            AcademicYearCode = "2081",
            AcademicYearName = "2081",
            AcademicYearNameNepali = "2081",
            IsActive = true,
            IsRunning = true
        });

        ctx.Programs.Add(new Program
        {
            Id = ProgramId,
            LevelId = LevelId,
            ProgramCode = "BCA",
            ProgramName = "Bachelor in Computer Application",
            ShortName = "BCA",
            Duration = 4,
            IsActive = true
        });
        ctx.Programs.Add(new Program
        {
            Id = ProgramIdOther,
            LevelId = LevelId,
            ProgramCode = "BIT",
            ProgramName = "Bachelor in Information Technology",
            ShortName = "BIT",
            Duration = 4,
            IsActive = true
        });

        ctx.Genders.Add(new Gender { Id = 1, GenderName = "Other", IsActive = true });
        ctx.StudentCategories.Add(new StudentCategory { Id = 1, StudentCategoryName = "Regular", IsActive = true });

        ctx.SubjectTypes.Add(new SubjectType { Id = 1, Code = "TH", Name = "Theory", IsActive = true });
        ctx.SubjectCatalogs.Add(new SubjectCatalog { Id = 1, TenantId = TenantId, SubjectCode = "SUB1", SubjectName = "Subject 1", SubjectTypeId = 1, IsActive = true });

        ctx.ExamTypes.Add(new ExamType { Id = Regular, Name = "Regular", Code = "1", IsActive = true });
        ctx.ExamTypes.Add(new ExamType { Id = Partial, Name = "Partial", Code = "2", IsActive = true });
        ctx.ExamTypes.Add(new ExamType { Id = Supplementary, Name = "Supplementary", Code = "3", IsActive = true });
        ctx.ExamTypes.Add(new ExamType { Id = Entrance, Name = "Entrance", Code = "4", IsActive = true });

        ctx.Semesters.Add(Semester(1, 1));
        ctx.Semesters.Add(Semester(2, 2));
        ctx.Semesters.Add(Semester(3, 3));
        ctx.Semesters.Add(Semester(4, 4));
        ctx.Semesters.Add(Semester(5, 5));
        ctx.Semesters.Add(Semester(6, 6));

        for (var semId = 1; semId <= 6; semId++)
        {
            ctx.SemesterInstances.Add(new SemesterInstance
            {
                Id = semId,
                TenantId = TenantId,
                SemesterId = semId,
                AcademicYearId = AcademicYearId,
                ProgramId = ProgramId,
                StartDate = DateTime.UtcNow.AddYears(-1),
                EndDate = DateTime.UtcNow.AddYears(-1).AddMonths(6)
            });
        }

        for (var semId = 1; semId <= 6; semId++)
        {
            ctx.SubjectOfferings.Add(Offering(100 + semId, semId, ProgramId));
            ctx.ProgramSemesters.Add(ProgramSemester(semId, semId, ProgramId));
        }

        ctx.SubjectOfferings.Add(Offering(201, 1, ProgramIdOther));
        ctx.SubjectOfferings.Add(Offering(202, 2, ProgramIdOther));
        ctx.ProgramSemesters.Add(ProgramSemester(7, 1, ProgramIdOther));
        ctx.ProgramSemesters.Add(ProgramSemester(8, 2, ProgramIdOther));
    }

    // The College global query filter only exposes colleges that have a CollegeFaculty row for
    // the current tenant, so the seeded college must be linked before it can be Included in
    // student registration queries.
    public static void SeedCollegeForStandardTenant(AppDbContext ctx)
    {
        ctx.Faculties.Add(new Faculty
        {
            Id = 99,
            Name = "Seed Faculty",
            OfficeCode = "SEED"
        });
        ctx.CollegeFaculties.Add(new CollegeFaculty
        {
            TenantId = TenantId,
            CollegeId = CollegeId,
            FacultyId = 99
        });
    }

    public static AppUser User(string id, string email) => new()
    {
        Id = id,
        UserName = email,
        Email = email,
        FullName = "Test User",
        IsActive = true
    };

    public static Semester Semester(int id, int number) => new()
    {
        Id = id,
        Number = number,
        Name = $"Semester {number}",
        Code = $"SEM{number}"
    };

    public static ProgramSemester ProgramSemester(int id, int semesterId, int programId) => new()
    {
        Id = id,
        ProgramId = programId,
        SemesterId = semesterId,
        IsActive = true,
        DisplayOrder = semesterId
    };

    public static ExamSchedule Schedule(int id, int semesterInstanceId, int examTypeId, DateOnly? endDate,
        DateTime? admissionCardReleaseDate, int programId = ProgramId, bool isActive = true)
    {
        return new ExamSchedule
        {
            Id = id,
            TenantId = TenantId,
            CollegeId = CollegeId,
            ExamScheduleName = $"Schedule {id}",
            ExamScheduleCode = $"SCH{id}",
            StartDate = endDate?.AddDays(-15),
            EndDate = endDate,
            IsActive = isActive,
            ProgramId = programId,
            SemesterInstanceId = semesterInstanceId,
            ExamTypeId = examTypeId,
            AdmissionCardReleaseDate = admissionCardReleaseDate,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(13, 0)
        };
    }

    public static SubjectOffering Offering(int id, int semesterId, int programId) => new()
    {
        Id = id,
        TenantId = TenantId,
        SubjectCatalogId = 1,
        ProgramId = programId,
        SemesterId = semesterId,
        IsActive = true,
        IsCompulsory = true,
        DisplayOrder = 1,
        HasTheory = true,
        HasPractical = false,
        HasInternal = true,
        TheoryFullMarks = 100,
        TheoryPassMarks = 40
    };

    public static StudentRegistration StudentRegistration(int id, string email, int programId = ProgramId) => new()
    {
        Id = id,
        TenantId = TenantId,
        LevelId = LevelId,
        CollegeId = CollegeId,
        ProgramId = programId,
        RegistrationNumber = $"REG{id}",
        FirstName = "Test",
        LastName = "Student",
        Email = email,
        DateOfBirthBS = "2050-01-01",
        GenderId = 1,
        StudentCategoryId = 1,
        AcademicYearId = AcademicYearId,
        IsActive = true
    };

    public static StudentAdmission Admission(int id, string userId, int programId = ProgramId) => new()
    {
        Id = id,
        TenantId = TenantId,
        ProgramsId = programId,
        CollegeId = CollegeId,
        AcademicYearId = AcademicYearId,
        AdmissionDate = DateTime.UtcNow,
        IsActive = true,
        FirstName = "Test",
        LastName = "Student",
        GenderId = 1,
        ContactNumber = "000",
        CollegeRollNumber = $"ROLL{id}",
        AppUserId = userId
    };

    public static SemesterEnrollment Enrollment(int id, int admissionId, int semesterInstanceId,
        StudentEnrollmentStatus status = StudentEnrollmentStatus.Active) => new()
    {
        Id = id,
        TenantId = TenantId,
        StudentAdmissionId = admissionId,
        SemesterInstanceId = semesterInstanceId,
        EnrollmentStatus = status,
        EnrollmentType = EnrollmentType.FullTime,
        PaymentStatus = PaymentStatus.Paid,
        EnrolledDate = DateTime.UtcNow,
        TotalCredits = 0,
        GradePoints = 0,
        TotalFee = 0,
        PaidAmount = 0,
        Deficiency = false,
        ResultStatus = ResultStatus.Incomplete
    };

    public static ApplicationVoucher Voucher(int id, int studentRegistrationId, int examScheduleId) => new()
    {
        Id = id,
        TenantId = TenantId,
        VoucherNumber = $"VCH{id}",
        StudentName = "Test Student",
        ContactNumber = "000",
        Amount = 1000,
        VoucherDate = DateTime.UtcNow,
        Timestamp = DateTime.UtcNow,
        StudentRegistrationId = studentRegistrationId,
        ExamScheduleId = examScheduleId
    };

    public static ExamRegistration ExamRegistration(int id, int examScheduleId, int voucherId,
        int programId = ProgramId, int? semesterEnrollmentId = null) => new()
    {
        Id = id,
        TenantId = TenantId,
        AcademicYearId = AcademicYearId,
        CollegeId = CollegeId,
        ExamScheduleId = examScheduleId,
        ApplicationVoucherId = voucherId,
        ProgramsId = programId,
        SemesterEnrollmentId = semesterEnrollmentId,
        RegistrationDate = DateTime.UtcNow,
        Status = RegistrationStatus.Pending,
        IsActive = true,
        IsAppliedByStudent = true
    };

    public static ExamSubjectResult Result(int id, int examRegistrationId, int subjectOfferingId,
        int examTypeId, string grade, int? examScheduleId = null) => new()
    {
        Id = id,
        TenantId = TenantId,
        ExamRegistrationId = examRegistrationId,
        SubjectOfferingId = subjectOfferingId,
        ExamTypeId = examTypeId,
        ExamScheduleId = examScheduleId,
        GradeLetter = grade,
        IsActive = true,
        IsSubmitted = true
    };
}
