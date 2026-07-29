using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.CollegeAdmins;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Domain.Entities.Permissions;
using FWU.Exam.Management.Domain.Entities.Semesters;
using FWU.Exam.Management.Domain.Entities.Students;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Domain.Interfaces;
using FluentAssertions;

namespace FWU.Exam.Management.Domain.Tests.Entities;

public class ApplicationVoucherTests
{
    [Fact]
    public void ApplicationVoucher_ShouldSetProperties()
    {
        var entity = new ApplicationVoucher
        {
            Id = 1,
            TenantId = 10,
            VoucherNumber = "VCH-001",
            StudentName = "Ram Sharma",
            DateOfBirthAd = new DateOnly(2000, 1, 15),
            DateOfBirthBs = "2056-10-01",
            Amount = 1500.00m,
            VoucherDate = new DateTime(2026, 7, 10),
            Timestamp = new DateTime(2026, 7, 10, 10, 0, 0),
            ContactNumber = "9841234567",
            Branch = "Mahendranagar",
            ExamScheduleId = 5,
            StudentRegistrationId = 100
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.VoucherNumber.Should().Be("VCH-001");
        entity.StudentName.Should().Be("Ram Sharma");
        entity.DateOfBirthAd.Should().Be(new DateOnly(2000, 1, 15));
        entity.DateOfBirthBs.Should().Be("2056-10-01");
        entity.Amount.Should().Be(1500.00m);
        entity.VoucherDate.Should().Be(new DateTime(2026, 7, 10));
        entity.Timestamp.Should().Be(new DateTime(2026, 7, 10, 10, 0, 0));
        entity.ContactNumber.Should().Be("9841234567");
        entity.Branch.Should().Be("Mahendranagar");
        entity.ExamScheduleId.Should().Be(5);
        entity.StudentRegistrationId.Should().Be(100);
    }

    [Fact]
    public void ApplicationVoucher_Implements_ITenantScoped()
    {
        var entity = new ApplicationVoucher();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ApplicationVoucher_Defaults_ShouldBeNull()
    {
        var entity = new ApplicationVoucher();
        entity.VoucherNumber.Should().BeNull();
        entity.StudentName.Should().BeNull();
        entity.DateOfBirthAd.Should().BeNull();
        entity.DateOfBirthBs.Should().BeNull();
        entity.Amount.Should().Be(0);
        entity.VoucherDate.Should().BeNull();
        entity.Timestamp.Should().BeNull();
        entity.ContactNumber.Should().BeNull();
        entity.Branch.Should().BeNull();
        entity.StudentRegistrationId.Should().BeNull();
    }
}

public class AuditLogTests
{
    [Fact]
    public void AuditLog_ShouldSetProperties()
    {
        var entity = new AuditLog
        {
            Id = 1,
            TenantId = 10,
            EntityName = "StudentRegistration",
            EntityId = "100",
            Action = "Create",
            UserName = "admin@fwu.edu.np",
            UserId = "user-1",
            Timestamp = new DateTime(2026, 7, 15, 14, 30, 0),
            ChangesJson = "{\"Name\":\"Old\"}"
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.EntityName.Should().Be("StudentRegistration");
        entity.EntityId.Should().Be("100");
        entity.Action.Should().Be("Create");
        entity.UserName.Should().Be("admin@fwu.edu.np");
        entity.UserId.Should().Be("user-1");
        entity.Timestamp.Should().Be(new DateTime(2026, 7, 15, 14, 30, 0));
        entity.ChangesJson.Should().Be("{\"Name\":\"Old\"}");
    }

    [Fact]
    public void AuditLog_Implements_ITenantScoped()
    {
        var entity = new AuditLog();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void AuditLog_Defaults_ShouldBeCorrect()
    {
        var entity = new AuditLog();
        entity.TenantId.Should().BeNull();
        entity.Timestamp.Should().Be(default);
        entity.EntityName.Should().BeNull();
        entity.EntityId.Should().BeNull();
        entity.Action.Should().BeNull();
        entity.UserName.Should().BeNull();
        entity.UserId.Should().BeNull();
        entity.ChangesJson.Should().BeNull();
    }
}

public class BatchTests
{
    [Fact]
    public void Batch_ShouldSetProperties()
    {
        var entity = new Batch
        {
            Id = 1,
            AcademicYearId = 5,
            BatchName = "2081 Batch A",
            Remarks = "Morning batch",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.AcademicYearId.Should().Be(5);
        entity.BatchName.Should().Be("2081 Batch A");
        entity.Remarks.Should().Be("Morning batch");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Batch_HasNavigationToAcademicYear()
    {
        var entity = new Batch();
        entity.AcademicYear.Should().BeNull();
        entity.StudentAdmissions.Should().BeNull();
        entity.ExamSlots.Should().BeNull();
    }

    [Fact]
    public void Batch_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new Batch();
        entity.IsActive.Should().BeFalse();
    }
}

public class BoardTests
{
    [Fact]
    public void Board_ShouldSetProperties()
    {
        var entity = new Board
        {
            Id = 1,
            CountryId = 1,
            BoardName = "NEB",
            Remarks = "National Examination Board",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.CountryId.Should().Be(1);
        entity.BoardName.Should().Be("NEB");
        entity.Remarks.Should().Be("National Examination Board");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Board_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new Board();
        entity.IsActive.Should().BeFalse();
    }
}

public class BulkUserCreationJobTests
{
    [Fact]
    public void BulkUserCreationJob_ShouldSetProperties()
    {
        var entity = new BulkUserCreationJob
        {
            Id = 1,
            TenantId = 10,
            UserId = "admin@fwu.edu.np",
            TotalStudents = 100,
            ProcessedCount = 50,
            SuccessCount = 45,
            FailedCount = 5,
            Status = "Processing",
            ErrorMessage = null,
            CreatedAt = new DateTime(2026, 7, 15),
            CompletedAt = null
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.UserId.Should().Be("admin@fwu.edu.np");
        entity.TotalStudents.Should().Be(100);
        entity.ProcessedCount.Should().Be(50);
        entity.SuccessCount.Should().Be(45);
        entity.FailedCount.Should().Be(5);
        entity.Status.Should().Be("Processing");
        entity.ErrorMessage.Should().BeNull();
        entity.CreatedAt.Should().Be(new DateTime(2026, 7, 15));
        entity.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void BulkUserCreationJob_Implements_ITenantScoped()
    {
        var entity = new BulkUserCreationJob();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void BulkUserCreationJob_DefaultStatus_ShouldBePending()
    {
        var entity = new BulkUserCreationJob();
        entity.Status.Should().Be("Pending");
        entity.UserId.Should().Be(string.Empty);
        entity.TotalStudents.Should().Be(0);
        entity.ProcessedCount.Should().Be(0);
        entity.SuccessCount.Should().Be(0);
        entity.FailedCount.Should().Be(0);
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}

public class CollegeTests
{
    [Fact]
    public void College_ShouldSetProperties()
    {
        var entity = new College
        {
            Id = 1,
            TenantId = 10,
            Code = "FWU001",
            Name = "Far Western University",
            CollegeNameNepali = "सुदूरपश्चिम विश्वविद्यालय",
            ShortName = "FWU",
            EstablishedDate = new DateTime(2010, 1, 1),
            ClosedDate = null,
            Website = "https://fwu.edu.np",
            Email = "info@fwu.edu.np",
            Phone1 = "099-525000",
            Phone2 = null,
            PrincipalName = "Prof. Dr. Example",
            PrincipalContactNumber = "9851234567",
            Fax = "099-525001",
            Remarks = "Main campus",
            IsExamCenterOnly = false,
            IsActive = true,
            AllocatedAmount = 500000,
            DisplayOrder = 1,
            AddressId = 1,
            CollegeTypeId = 1,
            CollegeProfileId = 1
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.Code.Should().Be("FWU001");
        entity.Name.Should().Be("Far Western University");
        entity.CollegeNameNepali.Should().Be("सुदूरपश्चिम विश्वविद्यालय");
        entity.ShortName.Should().Be("FWU");
        entity.EstablishedDate.Should().Be(new DateTime(2010, 1, 1));
        entity.ClosedDate.Should().BeNull();
        entity.Website.Should().Be("https://fwu.edu.np");
        entity.Email.Should().Be("info@fwu.edu.np");
        entity.Phone1.Should().Be("099-525000");
        entity.Phone2.Should().BeNull();
        entity.PrincipalName.Should().Be("Prof. Dr. Example");
        entity.PrincipalContactNumber.Should().Be("9851234567");
        entity.Fax.Should().Be("099-525001");
        entity.Remarks.Should().Be("Main campus");
        entity.IsExamCenterOnly.Should().BeFalse();
        entity.IsActive.Should().BeTrue();
        entity.AllocatedAmount.Should().Be(500000);
        entity.DisplayOrder.Should().Be(1);
        entity.AddressId.Should().Be(1);
        entity.CollegeTypeId.Should().Be(1);
        entity.CollegeProfileId.Should().Be(1);
    }

    [Fact]
    public void College_Implements_ITenantScoped()
    {
        var entity = new College();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void College_RequiredStrings_ShouldBeEmptyByDefault()
    {
        var entity = new College();
        entity.Code.Should().Be(string.Empty);
        entity.Name.Should().Be(string.Empty);
        entity.PrincipalName.Should().Be(string.Empty);
        entity.PrincipalContactNumber.Should().Be(string.Empty);
        entity.Email.Should().Be(string.Empty);
        entity.CollegeNameNepali.Should().BeNull();
        entity.IsActive.Should().BeFalse();
        entity.IsExamCenterOnly.Should().BeFalse();
    }
}

public class CollegeProgramTests
{
    [Fact]
    public void CollegeProgram_ShouldSetProperties()
    {
        var entity = new CollegeProgram
        {
            Id = 1,
            TenantId = 10,
            AffiliationDate = new DateTime(2020, 1, 1),
            NumberOfStudents = 150,
            Remarks = "Affiliated",
            IsActive = true,
            CollegeId = 1,
            ProgramId = 3
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.AffiliationDate.Should().Be(new DateTime(2020, 1, 1));
        entity.NumberOfStudents.Should().Be(150);
        entity.Remarks.Should().Be("Affiliated");
        entity.IsActive.Should().BeTrue();
        entity.CollegeId.Should().Be(1);
        entity.ProgramId.Should().Be(3);
    }

    [Fact]
    public void CollegeProgram_Implements_ITenantScoped()
    {
        var entity = new CollegeProgram();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void CollegeProgram_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new CollegeProgram();
        entity.IsActive.Should().BeFalse();
        entity.NumberOfStudents.Should().Be(0);
        entity.AffiliationDate.Should().BeNull();
    }
}

public class CollegeProfileTests
{
    [Fact]
    public void CollegeProfile_ShouldSetProperties()
    {
        var entity = new CollegeProfile
        {
            Id = 1,
            TenantId = 10,
            BankName = "Nepal Bank Ltd",
            BankBranchName = "Mahendranagar",
            BankAccountNumber = "001-123-456-789",
            ContactPersonName = "Hari Adhikari",
            ContactPersonMobileNumber = "9851234567",
            ContactPersonEmail = "hari@college.edu.np",
            Status = 1,
            CollegeId = 1,
            BlankChequeUserAttachmentId = 10,
            AuditReportUserAttachmentId = 11
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.BankName.Should().Be("Nepal Bank Ltd");
        entity.BankBranchName.Should().Be("Mahendranagar");
        entity.BankAccountNumber.Should().Be("001-123-456-789");
        entity.ContactPersonName.Should().Be("Hari Adhikari");
        entity.ContactPersonMobileNumber.Should().Be("9851234567");
        entity.ContactPersonEmail.Should().Be("hari@college.edu.np");
        entity.Status.Should().Be(1);
        entity.CollegeId.Should().Be(1);
        entity.BlankChequeUserAttachmentId.Should().Be(10);
        entity.AuditReportUserAttachmentId.Should().Be(11);
    }

    [Fact]
    public void CollegeProfile_Implements_ITenantScoped()
    {
        var entity = new CollegeProfile();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void CollegeProfile_Defaults_ShouldBeNull()
    {
        var entity = new CollegeProfile();
        entity.BankName.Should().BeNull();
        entity.BankAccountNumber.Should().BeNull();
        entity.ContactPersonName.Should().BeNull();
        entity.ContactPersonMobileNumber.Should().BeNull();
        entity.ContactPersonEmail.Should().BeNull();
        entity.Status.Should().BeNull();
    }
}

public class CollegeTypeTests
{
    [Fact]
    public void CollegeType_ShouldSetProperties()
    {
        var entity = new CollegeType
        {
            Id = 1,
            Code = "PUB",
            Name = "Public",
            Remarks = "Public college",
            IsDefault = true,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.Code.Should().Be("PUB");
        entity.Name.Should().Be("Public");
        entity.Remarks.Should().Be("Public college");
        entity.IsDefault.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CollegeType_DefaultIsDefault_ShouldBeFalse()
    {
        var entity = new CollegeType();
        entity.IsDefault.Should().BeFalse();
        entity.IsActive.Should().BeFalse();
        entity.Code.Should().BeNull();
        entity.Name.Should().BeNull();
    }
}

public class CountryTests
{
    [Fact]
    public void Country_ShouldSetProperties()
    {
        var entity = new Country
        {
            Id = 1,
            CountryName = "Nepal",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.CountryName.Should().Be("Nepal");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Country_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new Country();
        entity.IsActive.Should().BeFalse();
        entity.CountryName.Should().BeNull();
    }
}

public class CurriculumVersionTests
{
    [Fact]
    public void CurriculumVersion_ShouldSetProperties()
    {
        var entity = new CurriculumVersion
        {
            Id = 1,
            TenantId = 10,
            Name = "2081 Curriculum",
            ProgramId = 3,
            EffectiveAcademicYearId = 5,
            Description = "Updated curriculum for 2081",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.Name.Should().Be("2081 Curriculum");
        entity.ProgramId.Should().Be(3);
        entity.EffectiveAcademicYearId.Should().Be(5);
        entity.Description.Should().Be("Updated curriculum for 2081");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CurriculumVersion_Implements_ITenantScoped()
    {
        var entity = new CurriculumVersion();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void CurriculumVersion_RequiredName_ShouldBeEmptyByDefault()
    {
        var entity = new CurriculumVersion();
        entity.Name.Should().Be(string.Empty);
        entity.IsActive.Should().BeFalse();
        entity.Description.Should().BeNull();
    }
}

public class DistrictTests
{
    [Fact]
    public void District_ShouldSetProperties()
    {
        var entity = new District
        {
            Id = 1,
            ProvinceId = 2,
            DistrictCode = "KAN",
            DistrictName = "Kanchanpur",
            IsActive = true,
            Remarks = "Far western district"
        };

        entity.Id.Should().Be(1);
        entity.ProvinceId.Should().Be(2);
        entity.DistrictCode.Should().Be("KAN");
        entity.DistrictName.Should().Be("Kanchanpur");
        entity.IsActive.Should().BeTrue();
        entity.Remarks.Should().Be("Far western district");
    }

    [Fact]
    public void District_HasNavigationToProvince()
    {
        var entity = new District();
        entity.Province.Should().BeNull();
        entity.Colleges.Should().BeNull();
        entity.LocalLevels.Should().BeNull();
    }

    [Fact]
    public void District_DefaultIsActive_ShouldBeTrue()
    {
        var entity = new District();
        entity.IsActive.Should().BeTrue();
    }
}

public class EntryFormatTests
{
    [Fact]
    public void EntryFormat_ShouldSetProperties()
    {
        var entity = new EntryFormat
        {
            Id = 1,
            EntryFormatName = "Regular",
            Remarks = "Regular entry",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.EntryFormatName.Should().Be("Regular");
        entity.Remarks.Should().Be("Regular entry");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void EntryFormat_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new EntryFormat();
        entity.IsActive.Should().BeFalse();
        entity.EntryFormatName.Should().BeNull();
    }
}

public class EthnicityTests
{
    [Fact]
    public void Ethnicity_ShouldSetProperties()
    {
        var entity = new Ethnicity
        {
            Id = 1,
            EthnicityName = "Brahmin",
            IsDefault = true,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.EthnicityName.Should().Be("Brahmin");
        entity.IsDefault.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Ethnicity_DefaultIsDefault_ShouldBeFalse()
    {
        var entity = new Ethnicity();
        entity.IsDefault.Should().BeFalse();
        entity.IsActive.Should().BeFalse();
        entity.EthnicityName.Should().BeNull();
    }
}

public class ExamCenterTests
{
    [Fact]
    public void ExamCenter_ShouldSetProperties()
    {
        var entity = new ExamCenter
        {
            Id = 1,
            TenantId = 10,
            ExamScheduleId = 5,
            CollegeId = 3,
            Remark = "Main center",
            IsActive = true,
            Code = "EC001"
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ExamScheduleId.Should().Be(5);
        entity.CollegeId.Should().Be(3);
        entity.Remark.Should().Be("Main center");
        entity.IsActive.Should().BeTrue();
        entity.Code.Should().Be("EC001");
    }

    [Fact]
    public void ExamCenter_Implements_ITenantScoped()
    {
        var entity = new ExamCenter();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ExamCenter_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new ExamCenter();
        entity.IsActive.Should().BeFalse();
        entity.CollegeId.Should().BeNull();
        entity.Code.Should().BeNull();
    }
}

public class ExamCenterCollegeTests
{
    [Fact]
    public void ExamCenterCollege_ShouldSetProperties()
    {
        var entity = new ExamCenterCollege
        {
            Id = 1,
            TenantId = 10,
            ExamCenterId = 2,
            CollegeId = 3
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ExamCenterId.Should().Be(2);
        entity.CollegeId.Should().Be(3);
    }

    [Fact]
    public void ExamCenterCollege_Implements_ITenantScoped()
    {
        var entity = new ExamCenterCollege();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }
}

public class ExamCenterSymbolRangeTests
{
    [Fact]
    public void ExamCenterSymbolRange_ShouldSetProperties()
    {
        var entity = new ExamCenterSymbolRange
        {
            Id = 1,
            TenantId = 10,
            ExamScheduleId = 5,
            ExamCenterId = 2,
            FromSymbolNumber = 1001,
            ToSymbolNumber = 1500
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ExamScheduleId.Should().Be(5);
        entity.ExamCenterId.Should().Be(2);
        entity.FromSymbolNumber.Should().Be(1001);
        entity.ToSymbolNumber.Should().Be(1500);
    }

    [Fact]
    public void ExamCenterSymbolRange_Implements_ITenantScoped()
    {
        var entity = new ExamCenterSymbolRange();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ExamCenterSymbolRange_Defaults_ShouldBeZero()
    {
        var entity = new ExamCenterSymbolRange();
        entity.FromSymbolNumber.Should().Be(0);
        entity.ToSymbolNumber.Should().Be(0);
    }
}

public class ExamCenterVenueTests
{
    [Fact]
    public void ExamCenterVenue_ShouldSetProperties()
    {
        var entity = new ExamCenterVenue
        {
            Id = 1,
            TenantId = 10,
            ExamCenterId = 2,
            CollegeId = 3
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ExamCenterId.Should().Be(2);
        entity.CollegeId.Should().Be(3);
    }

    [Fact]
    public void ExamCenterVenue_Implements_ITenantScoped()
    {
        var entity = new ExamCenterVenue();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }
}

public class ExamFeeTests
{
    [Fact]
    public void ExamFee_ShouldSetProperties()
    {
        var entity = new ExamFee
        {
            Id = 1,
            TenantId = 10,
            Name = "Regular Exam Fee",
            ExamScheduleId = 5,
            Amount = 1500.00m,
            CollegeTypeId = 2,
            ExamTypeId = 1,
            ThroughDate = new DateTime(2026, 8, 15),
            ApplicableDate = new DateTime(2026, 7, 15),
            IsCollegeFee = true
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.Name.Should().Be("Regular Exam Fee");
        entity.ExamScheduleId.Should().Be(5);
        entity.Amount.Should().Be(1500.00m);
        entity.CollegeTypeId.Should().Be(2);
        entity.ExamTypeId.Should().Be(1);
        entity.ThroughDate.Should().Be(new DateTime(2026, 8, 15));
        entity.ApplicableDate.Should().Be(new DateTime(2026, 7, 15));
        entity.IsCollegeFee.Should().BeTrue();
    }

    [Fact]
    public void ExamFee_Implements_ITenantScoped()
    {
        var entity = new ExamFee();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ExamFee_Defaults_ShouldBeCorrect()
    {
        var entity = new ExamFee();
        entity.Amount.Should().Be(0);
        entity.IsCollegeFee.Should().BeFalse();
        entity.Name.Should().BeNull();
        entity.CollegeTypeId.Should().BeNull();
        entity.ExamTypeId.Should().BeNull();
        entity.ThroughDate.Should().BeNull();
        entity.ApplicableDate.Should().BeNull();
    }
}

public class ExamRegistrationTests
{
    [Fact]
    public void ExamRegistration_ShouldSetProperties()
    {
        var entity = new ExamRegistration
        {
            Id = 1,
            TenantId = 10,
            AcademicYearId = 5,
            ExamCenterId = 2,
            CollegeId = 3,
            ExamRollNumber = "2081-001",
            ExamRollNumberCoding = 2081001,
            FeeEnclosed = 1500.00m,
            AttendancePercentage = 85.5m,
            RegistrationDate = new DateTime(2026, 7, 15),
            Status = RegistrationStatus.Registered,
            VerifiedByUsername = "admin",
            VerifiedDate = new DateTime(2026, 7, 16),
            Sgpa = "3.5",
            Remarks = "Verified",
            IsActive = true,
            ExamScheduleId = 5,
            RollNumberIndex = 1,
            IsAppliedByStudent = true,
            ProgramsId = 3,
            ApplicationVoucherId = 10,
            AdminVerifiedByUsername = "superadmin",
            SymbolNumber = "SN001",
            AdminVerifiedDate = new DateTime(2026, 7, 17)
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.AcademicYearId.Should().Be(5);
        entity.ExamCenterId.Should().Be(2);
        entity.CollegeId.Should().Be(3);
        entity.ExamRollNumber.Should().Be("2081-001");
        entity.ExamRollNumberCoding.Should().Be(2081001);
        entity.FeeEnclosed.Should().Be(1500.00m);
        entity.AttendancePercentage.Should().Be(85.5m);
        entity.RegistrationDate.Should().Be(new DateTime(2026, 7, 15));
        entity.Status.Should().Be(RegistrationStatus.Registered);
        entity.VerifiedByUsername.Should().Be("admin");
        entity.VerifiedDate.Should().Be(new DateTime(2026, 7, 16));
        entity.Sgpa.Should().Be("3.5");
        entity.Remarks.Should().Be("Verified");
        entity.IsActive.Should().BeTrue();
        entity.ExamScheduleId.Should().Be(5);
        entity.RollNumberIndex.Should().Be(1);
        entity.IsAppliedByStudent.Should().BeTrue();
        entity.ProgramsId.Should().Be(3);
        entity.ApplicationVoucherId.Should().Be(10);
        entity.AdminVerifiedByUsername.Should().Be("superadmin");
        entity.SymbolNumber.Should().Be("SN001");
        entity.AdminVerifiedDate.Should().Be(new DateTime(2026, 7, 17));
    }

    [Fact]
    public void ExamRegistration_Implements_ITenantScoped()
    {
        var entity = new ExamRegistration();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ExamRegistration_DefaultStatus_ShouldBePending()
    {
        var entity = new ExamRegistration();
        entity.Status.Should().Be(default(RegistrationStatus));
        entity.IsActive.Should().BeFalse();
        entity.ExamCenterId.Should().BeNull();
        entity.IsAppliedByStudent.Should().BeNull();
    }
}

public class ExamRollNumberSetupTests
{
    [Fact]
    public void ExamRollNumberSetup_ShouldSetProperties()
    {
        var entity = new ExamRollNumberSetup
        {
            Id = 1,
            TenantId = 10,
            ExamScheduleId = 5,
            FirstExamRollNumber = 1001,
            Prefix = "FWU",
            Suffix = "K",
            DetailsJson = "{}",
            MinimumRollNumberLength = 5,
            Round = 1,
            MinimumGap = 0,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ExamScheduleId.Should().Be(5);
        entity.FirstExamRollNumber.Should().Be(1001);
        entity.Prefix.Should().Be("FWU");
        entity.Suffix.Should().Be("K");
        entity.DetailsJson.Should().Be("{}");
        entity.MinimumRollNumberLength.Should().Be(5);
        entity.Round.Should().Be(1);
        entity.MinimumGap.Should().Be(0);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void ExamRollNumberSetup_Implements_ITenantScoped()
    {
        var entity = new ExamRollNumberSetup();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ExamRollNumberSetup_Defaults_ShouldBeCorrect()
    {
        var entity = new ExamRollNumberSetup();
        entity.FirstExamRollNumber.Should().Be(0);
        entity.MinimumRollNumberLength.Should().Be(0);
        entity.Round.Should().Be(0);
        entity.MinimumGap.Should().Be(0);
        entity.IsActive.Should().BeFalse();
    }
}

public class ExamScheduleTests
{
    [Fact]
    public void ExamSchedule_ShouldSetProperties()
    {
        var entity = new ExamSchedule
        {
            Id = 1,
            TenantId = 10,
            CollegeId = 3,
            ExamScheduleName = "2081 Regular Exam",
            StartDateBs = "2081-04-01",
            EndDateBs = "2081-04-15",
            StartDate = new DateOnly(2025, 7, 15),
            EndDate = new DateOnly(2025, 7, 30),
            PublishedDate = new DateTime(2025, 6, 1),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(13, 0),
            Remarks = "Regular exam schedule",
            IsActive = true,
            ExtendedDate = null,
            ExtendedDateCharge = null,
            ExamFee = 1500.00m,
            PracticalSubjectFee = 500.00m,
            CollegeApprovalDate = null,
            AdmissionCardReleaseDate = null,
            ExamScheduleCode = "2081-REG",
            AcademicYearId = 5,
            ProgramId = 3,
            SemesterId = 2,
            ExamTypeId = 1,
            LevelId = 1
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.CollegeId.Should().Be(3);
        entity.ExamScheduleName.Should().Be("2081 Regular Exam");
        entity.StartDateBs.Should().Be("2081-04-01");
        entity.EndDateBs.Should().Be("2081-04-15");
        entity.StartDate.Should().Be(new DateOnly(2025, 7, 15));
        entity.EndDate.Should().Be(new DateOnly(2025, 7, 30));
        entity.PublishedDate.Should().Be(new DateTime(2025, 6, 1));
        entity.StartTime.Should().Be(new TimeOnly(10, 0));
        entity.EndTime.Should().Be(new TimeOnly(13, 0));
        entity.Remarks.Should().Be("Regular exam schedule");
        entity.IsActive.Should().BeTrue();
        entity.ExamFee.Should().Be(1500.00m);
        entity.PracticalSubjectFee.Should().Be(500.00m);
        entity.ExamScheduleCode.Should().Be("2081-REG");
        entity.AcademicYearId.Should().Be(5);
        entity.ProgramId.Should().Be(3);
        entity.SemesterId.Should().Be(2);
        entity.ExamTypeId.Should().Be(1);
        entity.LevelId.Should().Be(1);
    }

    [Fact]
    public void ExamSchedule_Implements_ITenantScoped()
    {
        var entity = new ExamSchedule();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ExamSchedule_Defaults_ShouldBeCorrect()
    {
        var entity = new ExamSchedule();
        entity.IsActive.Should().BeFalse();
        entity.CollegeId.Should().BeNull();
        entity.StartTime.Should().Be(default);
        entity.EndTime.Should().Be(default);
        entity.LevelId.Should().BeNull();
    }
}

public class ExamSlotTests
{
    [Fact]
    public void ExamSlot_ShouldSetProperties()
    {
        var entity = new ExamSlot
        {
            Id = 1,
            TenantId = 10,
            ExamScheduleId = 5,
            SubjectOfferingId = 10,
            BatchId = 2,
            ExamCenterId = 3,
            ExamDate = "2081-04-05",
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(13, 0),
            RoomNumber = "Hall A",
            Remarks = "Morning slot"
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ExamScheduleId.Should().Be(5);
        entity.SubjectOfferingId.Should().Be(10);
        entity.BatchId.Should().Be(2);
        entity.ExamCenterId.Should().Be(3);
        entity.ExamDate.Should().Be("2081-04-05");
        entity.StartTime.Should().Be(new TimeOnly(10, 0));
        entity.EndTime.Should().Be(new TimeOnly(13, 0));
        entity.RoomNumber.Should().Be("Hall A");
        entity.Remarks.Should().Be("Morning slot");
    }

    [Fact]
    public void ExamSlot_Implements_ITenantScoped()
    {
        var entity = new ExamSlot();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ExamSlot_TimeOnlyDefaults_ShouldBeDefault()
    {
        var entity = new ExamSlot();
        entity.StartTime.Should().Be(default);
        entity.EndTime.Should().Be(default);
        entity.ExamDate.Should().BeNull();
        entity.RoomNumber.Should().BeNull();
    }
}

public class ExamSubjectResultTests
{
    [Fact]
    public void ExamSubjectResult_ShouldSetProperties()
    {
        var entity = new ExamSubjectResult
        {
            Id = 1,
            TenantId = 10,
            ExamRegistrationId = 5,
            ExamTypeId = 1,
            SubjectOfferingId = 10,
            ExamScheduleId = 3,
            ObtainedMarksTheory = 75.5f,
            ObtainedMarksTheoryConfirm = 75.5f,
            ObtainedMarksPractical = 20.0f,
            ObtainedMarksPracticalConfirm = 20.0f,
            ObtainedMarksTheoryInternal = 10.0f,
            ObtainedMarksPracticalInternal = 5.0f,
            GradeLetter = "A",
            Remarks = "Good",
            IsActive = true,
            IsLooseEntry = false,
            IsTheoryRegistered = true,
            IsPracticalRegistered = true,
            IsExtra = false,
            ExamStartedDateTime = new DateTime(2026, 7, 15, 10, 0, 0),
            IsSubmitted = true,
            ObtainedMarks = 85.5f,
            ExamSubmittedDateTime = new DateTime(2026, 7, 15, 13, 0, 0),
            IsAutoSubmitted = false,
            LastStatusSyncDateTime = new DateTime(2026, 7, 15, 14, 0, 0)
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ExamRegistrationId.Should().Be(5);
        entity.ExamTypeId.Should().Be(1);
        entity.SubjectOfferingId.Should().Be(10);
        entity.ExamScheduleId.Should().Be(3);
        entity.ObtainedMarksTheory.Should().Be(75.5f);
        entity.ObtainedMarksTheoryConfirm.Should().Be(75.5f);
        entity.ObtainedMarksPractical.Should().Be(20.0f);
        entity.ObtainedMarksPracticalConfirm.Should().Be(20.0f);
        entity.ObtainedMarksTheoryInternal.Should().Be(10.0f);
        entity.ObtainedMarksPracticalInternal.Should().Be(5.0f);
        entity.GradeLetter.Should().Be("A");
        entity.Remarks.Should().Be("Good");
        entity.IsActive.Should().BeTrue();
        entity.IsLooseEntry.Should().BeFalse();
        entity.IsTheoryRegistered.Should().BeTrue();
        entity.IsPracticalRegistered.Should().BeTrue();
        entity.IsExtra.Should().BeFalse();
        entity.ExamStartedDateTime.Should().Be(new DateTime(2026, 7, 15, 10, 0, 0));
        entity.IsSubmitted.Should().BeTrue();
        entity.ObtainedMarks.Should().Be(85.5f);
        entity.ExamSubmittedDateTime.Should().Be(new DateTime(2026, 7, 15, 13, 0, 0));
        entity.IsAutoSubmitted.Should().BeFalse();
        entity.LastStatusSyncDateTime.Should().Be(new DateTime(2026, 7, 15, 14, 0, 0));
    }

    [Fact]
    public void ExamSubjectResult_Implements_IAuditable()
    {
        var entity = new ExamSubjectResult();
        entity.Should().BeAssignableTo<IAuditable>();
    }

    [Fact]
    public void ExamSubjectResult_Implements_ITenantScoped()
    {
        var entity = new ExamSubjectResult();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ExamSubjectResult_Defaults_ShouldBeCorrect()
    {
        var entity = new ExamSubjectResult();
        entity.IsActive.Should().BeFalse();
        entity.IsSubmitted.Should().BeFalse();
        entity.ExamScheduleId.Should().BeNull();
        entity.IsLooseEntry.Should().BeNull();
        entity.IsTheoryRegistered.Should().BeNull();
        entity.IsPracticalRegistered.Should().BeNull();
        entity.IsExtra.Should().BeNull();
        entity.IsAutoSubmitted.Should().BeNull();
    }
}

public class ExamTypeTests
{
    [Fact]
    public void ExamType_ShouldSetProperties()
    {
        var entity = new ExamType
        {
            Id = 1,
            Name = "Regular",
            Remarks = "Regular examination",
            IsActive = true,
            Code = "REG"
        };

        entity.Id.Should().Be(1);
        entity.Name.Should().Be("Regular");
        entity.Remarks.Should().Be("Regular examination");
        entity.IsActive.Should().BeTrue();
        entity.Code.Should().Be("REG");
    }

    [Fact]
    public void ExamType_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new ExamType();
        entity.IsActive.Should().BeFalse();
        entity.Name.Should().BeNull();
        entity.Code.Should().BeNull();
    }
}

public class EntranceExamApplicationTests
{
    [Fact]
    public void EntranceExamApplication_ShouldSetProperties()
    {
        var entity = new EntranceExamApplication
        {
            Id = 1,
            AcademicYearId = 5,
            CollegeId = 3,
            ProgramId = 2,
            FirstName = "Ram",
            MiddleName = "Prasad",
            LastName = "Sharma",
            NepaliName = "राम प्रसाद शर्मा",
            DateOfBirthBS = "2056-10-01",
            DateOfBirthAD = "2000-01-15",
            GenderId = 1,
            Email = "ram@example.com",
            ContactNumber = "9841234567",
            Phone = "099-525000",
            PermanentAddressId = 10,
            FatherName = "Hari Sharma",
            FatherContact = "9851234567",
            MotherName = "Sita Sharma",
            MotherContact = "9861234567",
            GuardianEmail = "guardian@example.com",
            FatherProfession = "Teacher",
            MotherProfession = "Housewife",
            CitizenshipNo = "12345",
            CitizenshipDistrictId = 2,
            CitizenshipIssueDateBs = "2075-01-01",
            CitizenshipIssueDateAd = "2018-04-14",
            BloodGroup = "O+",
            BirthPlace = "Kanchanpur",
            Country = "Nepal",
            PostalCode = "10400",
            PhotoPath = "/photos/ram.jpg",
            DocumentsPath = "/docs/ram.pdf",
            VoucherPath = "/vouchers/ram.pdf",
            PreviousSchoolCollege = "FWU College",
            PreviousLevelId = 1,
            PreviousPassedYear = "2077",
            PreviousSymbolNumber = "SN001",
            PreviousGPA = 3.5m,
            PreviousDivision = "First",
            PreviousLevel2Id = null,
            PreviousSchoolCollege2 = null,
            PreviousBoard2 = null,
            PreviousSymbolNumber2 = null,
            PreviousPassedYear2 = null,
            PreviousGPA2 = null,
            PreviousDivision2 = null,
            PreviousLevel3Id = null,
            PreviousSchoolCollege3 = null,
            PreviousBoard3 = null,
            PreviousSymbolNumber3 = null,
            PreviousPassedYear3 = null,
            PreviousGPA3 = null,
            PreviousDivision3 = null,
            ApplicationVoucherId = 5,
            PaymentVerified = true,
            Status = ApplicationStatus.Approved,
            ReviewedBy = "admin",
            ReviewDate = new DateTime(2026, 7, 20),
            ReviewRemarks = "Approved",
            TenantId = 10,
            CreatedAt = new DateTime(2026, 7, 15)
        };

        entity.Id.Should().Be(1);
        entity.AcademicYearId.Should().Be(5);
        entity.CollegeId.Should().Be(3);
        entity.ProgramId.Should().Be(2);
        entity.FirstName.Should().Be("Ram");
        entity.MiddleName.Should().Be("Prasad");
        entity.LastName.Should().Be("Sharma");
        entity.NepaliName.Should().Be("राम प्रसाद शर्मा");
        entity.DateOfBirthBS.Should().Be("2056-10-01");
        entity.DateOfBirthAD.Should().Be("2000-01-15");
        entity.GenderId.Should().Be(1);
        entity.Email.Should().Be("ram@example.com");
        entity.ContactNumber.Should().Be("9841234567");
        entity.Phone.Should().Be("099-525000");
        entity.PermanentAddressId.Should().Be(10);
        entity.FatherName.Should().Be("Hari Sharma");
        entity.FatherContact.Should().Be("9851234567");
        entity.MotherName.Should().Be("Sita Sharma");
        entity.MotherContact.Should().Be("9861234567");
        entity.GuardianEmail.Should().Be("guardian@example.com");
        entity.FatherProfession.Should().Be("Teacher");
        entity.MotherProfession.Should().Be("Housewife");
        entity.CitizenshipNo.Should().Be("12345");
        entity.CitizenshipDistrictId.Should().Be(2);
        entity.CitizenshipIssueDateBs.Should().Be("2075-01-01");
        entity.CitizenshipIssueDateAd.Should().Be("2018-04-14");
        entity.BloodGroup.Should().Be("O+");
        entity.BirthPlace.Should().Be("Kanchanpur");
        entity.Country.Should().Be("Nepal");
        entity.PostalCode.Should().Be("10400");
        entity.PhotoPath.Should().Be("/photos/ram.jpg");
        entity.DocumentsPath.Should().Be("/docs/ram.pdf");
        entity.VoucherPath.Should().Be("/vouchers/ram.pdf");
        entity.PreviousSchoolCollege.Should().Be("FWU College");
        entity.PreviousLevelId.Should().Be(1);
        entity.PreviousPassedYear.Should().Be("2077");
        entity.PreviousSymbolNumber.Should().Be("SN001");
        entity.PreviousGPA.Should().Be(3.5m);
        entity.PreviousDivision.Should().Be("First");
        entity.ApplicationVoucherId.Should().Be(5);
        entity.PaymentVerified.Should().BeTrue();
        entity.Status.Should().Be(ApplicationStatus.Approved);
        entity.ReviewedBy.Should().Be("admin");
        entity.ReviewDate.Should().Be(new DateTime(2026, 7, 20));
        entity.ReviewRemarks.Should().Be("Approved");
        entity.TenantId.Should().Be(10);
        entity.CreatedAt.Should().Be(new DateTime(2026, 7, 15));
    }

    [Fact]
    public void EntranceExamApplication_Implements_IAuditable()
    {
        var entity = new EntranceExamApplication();
        entity.Should().BeAssignableTo<IAuditable>();
    }

    [Fact]
    public void EntranceExamApplication_Implements_ITenantScoped()
    {
        var entity = new EntranceExamApplication();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void EntranceExamApplication_DefaultStatus_ShouldBeSubmitted()
    {
        var entity = new EntranceExamApplication();
        entity.Status.Should().Be(ApplicationStatus.Submitted);
        entity.PaymentVerified.Should().BeFalse();
        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        entity.FirstName.Should().BeNull();
        entity.LastName.Should().BeNull();
        entity.DateOfBirthBS.Should().BeNull();
    }
}

public class AdmitCardTests
{
    [Fact]
    public void AdmitCard_ShouldSetProperties()
    {
        var entity = new AdmitCard
        {
            Id = 1,
            TenantId = 10,
            ExamRegistrationId = 5,
            ExamScheduleId = 3,
            StudentRegistrationId = 100,
            AdmitCardNumber = "ADC-001",
            ExamRollNo = "2081-001",
            Campus = "Main Campus",
            Level = "Bachelor",
            Program = "BBS",
            RegistrationNumber = "REG-001",
            Semester = "First",
            ExamType = "Regular",
            Year = "2081",
            PhotoPath = "/photos/student.jpg",
            SignaturePath = "/signatures/student.jpg",
            ControllerSignaturePath = "/signatures/controller.jpg",
            GeneratedDate = new DateTime(2026, 7, 15),
            IsDownloaded = true,
            DownloadedDate = new DateTime(2026, 7, 16),
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ExamRegistrationId.Should().Be(5);
        entity.ExamScheduleId.Should().Be(3);
        entity.StudentRegistrationId.Should().Be(100);
        entity.AdmitCardNumber.Should().Be("ADC-001");
        entity.ExamRollNo.Should().Be("2081-001");
        entity.Campus.Should().Be("Main Campus");
        entity.Level.Should().Be("Bachelor");
        entity.Program.Should().Be("BBS");
        entity.RegistrationNumber.Should().Be("REG-001");
        entity.Semester.Should().Be("First");
        entity.ExamType.Should().Be("Regular");
        entity.Year.Should().Be("2081");
        entity.PhotoPath.Should().Be("/photos/student.jpg");
        entity.SignaturePath.Should().Be("/signatures/student.jpg");
        entity.ControllerSignaturePath.Should().Be("/signatures/controller.jpg");
        entity.GeneratedDate.Should().Be(new DateTime(2026, 7, 15));
        entity.IsDownloaded.Should().BeTrue();
        entity.DownloadedDate.Should().Be(new DateTime(2026, 7, 16));
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AdmitCard_Implements_ITenantScoped()
    {
        var entity = new AdmitCard();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void AdmitCard_GeneratedDateDefault_ShouldBeMinValue()
    {
        var entity = new AdmitCard();
        entity.GeneratedDate.Should().Be(default);
        entity.IsDownloaded.Should().BeFalse();
        entity.IsActive.Should().BeFalse();
        entity.DownloadedDate.Should().BeNull();
        entity.StudentRegistrationId.Should().BeNull();
    }
}

public class RetotalRequestTests
{
    [Fact]
    public void RetotalRequest_ShouldSetProperties()
    {
        var entity = new RetotalRequest
        {
            Id = 1,
            TenantId = 10,
            ExamSubjectResultId = 5,
            StudentRegistrationId = 100,
            ExamRegistrationId = 3,
            RequestedDate = new DateTime(2026, 7, 20),
            Reason = "Marks mismatch",
            Status = RetotalStatus.Pending,
            OriginalGradeLetter = "B",
            OriginalObtainedMarks = 65.5f,
            RetotalledGradeLetter = "A",
            RetotalledObtainedMarks = 75.0f,
            ReviewedByUsername = "admin",
            ReviewedDate = new DateTime(2026, 7, 25),
            AdminRemarks = "Corrected",
            FeeAmount = 500.00m,
            FeePaid = true,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ExamSubjectResultId.Should().Be(5);
        entity.StudentRegistrationId.Should().Be(100);
        entity.ExamRegistrationId.Should().Be(3);
        entity.RequestedDate.Should().Be(new DateTime(2026, 7, 20));
        entity.Reason.Should().Be("Marks mismatch");
        entity.Status.Should().Be(RetotalStatus.Pending);
        entity.OriginalGradeLetter.Should().Be("B");
        entity.OriginalObtainedMarks.Should().Be(65.5f);
        entity.RetotalledGradeLetter.Should().Be("A");
        entity.RetotalledObtainedMarks.Should().Be(75.0f);
        entity.ReviewedByUsername.Should().Be("admin");
        entity.ReviewedDate.Should().Be(new DateTime(2026, 7, 25));
        entity.AdminRemarks.Should().Be("Corrected");
        entity.FeeAmount.Should().Be(500.00m);
        entity.FeePaid.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RetotalRequest_Implements_ITenantScoped()
    {
        var entity = new RetotalRequest();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void RetotalRequest_Defaults_ShouldBeCorrect()
    {
        var entity = new RetotalRequest();
        entity.Status.Should().Be(default(RetotalStatus));
        entity.FeePaid.Should().BeFalse();
        entity.IsActive.Should().BeFalse();
        entity.RequestedDate.Should().Be(default);
        entity.FeeAmount.Should().BeNull();
    }
}

public class FacultyTests
{
    [Fact]
    public void Faculty_ShouldSetProperties()
    {
        var entity = new Faculty
        {
            Id = 1,
            Name = "Faculty of Management",
            OfficeCode = "FOM001",
            ShortName = "FOM",
            ContactNumber = "099-525000",
            Address = "Mahendranagar",
            Email = "fom@fwu.edu.np",
            LogoPath = "/logos/fom.png",
            TenantId = 10
        };

        entity.Id.Should().Be(1);
        entity.Name.Should().Be("Faculty of Management");
        entity.OfficeCode.Should().Be("FOM001");
        entity.ShortName.Should().Be("FOM");
        entity.ContactNumber.Should().Be("099-525000");
        entity.Address.Should().Be("Mahendranagar");
        entity.Email.Should().Be("fom@fwu.edu.np");
        entity.LogoPath.Should().Be("/logos/fom.png");
        entity.TenantId.Should().Be(10);
    }

    [Fact]
    public void Faculty_Implements_ITenantScoped()
    {
        var entity = new Faculty();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void Faculty_RequiredStrings_ShouldBeEmptyByDefault()
    {
        var entity = new Faculty();
        entity.Name.Should().Be(string.Empty);
        entity.OfficeCode.Should().Be(string.Empty);
        entity.ContactNumber.Should().Be(string.Empty);
        entity.Address.Should().Be(string.Empty);
        entity.Email.Should().Be(string.Empty);
        entity.TenantId.Should().BeNull();
        entity.ShortName.Should().BeNull();
    }
}

public class FiscalYearTests
{
    [Fact]
    public void FiscalYear_ShouldSetProperties()
    {
        var entity = new FiscalYear
        {
            Id = 1,
            FiscalYearName = "2081/2082",
            StartDate = "2081-04-01",
            EndDate = "2082-03-31",
            IsRunning = true,
            Remarks = "Current fiscal year",
            IsActive = true,
            FiscalYearCode = "FY2081"
        };

        entity.Id.Should().Be(1);
        entity.FiscalYearName.Should().Be("2081/2082");
        entity.StartDate.Should().Be("2081-04-01");
        entity.EndDate.Should().Be("2082-03-31");
        entity.IsRunning.Should().BeTrue();
        entity.Remarks.Should().Be("Current fiscal year");
        entity.IsActive.Should().BeTrue();
        entity.FiscalYearCode.Should().Be("FY2081");
    }

    [Fact]
    public void FiscalYear_DefaultIsRunning_ShouldBeFalse()
    {
        var entity = new FiscalYear();
        entity.IsRunning.Should().BeFalse();
        entity.IsActive.Should().BeFalse();
        entity.FiscalYearName.Should().BeNull();
        entity.StartDate.Should().BeNull();
        entity.EndDate.Should().BeNull();
    }
}

public class GenderTests
{
    [Fact]
    public void Gender_ShouldSetProperties()
    {
        var entity = new Gender
        {
            Id = 1,
            GenderName = "Male",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.GenderName.Should().Be("Male");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Gender_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new Gender();
        entity.IsActive.Should().BeFalse();
        entity.GenderName.Should().BeNull();
    }
}

public class GradeDefinitionTests
{
    [Fact]
    public void GradeDefinition_ShouldSetProperties()
    {
        var entity = new GradeDefinition
        {
            Id = 1,
            GradeLetter = "A",
            MinPercentage = 80,
            MaxPercentage = 100,
            GradePoint = 4.0m,
            Remark = "Excellent",
            IsPass = true,
            DisplayOrder = 1,
            GradingSchemeId = 1
        };

        entity.Id.Should().Be(1);
        entity.GradeLetter.Should().Be("A");
        entity.MinPercentage.Should().Be(80);
        entity.MaxPercentage.Should().Be(100);
        entity.GradePoint.Should().Be(4.0m);
        entity.Remark.Should().Be("Excellent");
        entity.IsPass.Should().BeTrue();
        entity.DisplayOrder.Should().Be(1);
        entity.GradingSchemeId.Should().Be(1);
    }

    [Fact]
    public void GradeDefinition_DefaultIsPass_ShouldBeTrue()
    {
        var entity = new GradeDefinition();
        entity.IsPass.Should().BeTrue();
        entity.GradeLetter.Should().Be(string.Empty);
        entity.MinPercentage.Should().Be(0);
        entity.MaxPercentage.Should().Be(0);
        entity.GradePoint.Should().Be(0);
        entity.DisplayOrder.Should().Be(0);
    }
}

public class GradingSchemeTests
{
    [Fact]
    public void GradingScheme_ShouldSetProperties()
    {
        var entity = new GradingScheme
        {
            Id = 1,
            Name = "Four Point Scale",
            ProgramId = 3,
            AcademicYearId = 5,
            Description = "Standard 4.0 grading",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.Name.Should().Be("Four Point Scale");
        entity.ProgramId.Should().Be(3);
        entity.AcademicYearId.Should().Be(5);
        entity.Description.Should().Be("Standard 4.0 grading");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void GradingScheme_Implements_IAuditable()
    {
        var entity = new GradingScheme();
        entity.Should().BeAssignableTo<IAuditable>();
    }

    [Fact]
    public void GradingScheme_Defaults_ShouldBeCorrect()
    {
        var entity = new GradingScheme();
        entity.Name.Should().Be(string.Empty);
        entity.AcademicYearId.Should().BeNull();
        entity.Description.Should().BeNull();
        entity.IsActive.Should().BeFalse();
    }
}

public class IndexGroupTests
{
    [Fact]
    public void IndexGroup_ShouldSetProperties()
    {
        var entity = new IndexGroup
        {
            Id = 1,
            IndexGroupName = "A-K",
            Remarks = "Index group for surnames A to K",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.IndexGroupName.Should().Be("A-K");
        entity.Remarks.Should().Be("Index group for surnames A to K");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IndexGroup_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new IndexGroup();
        entity.IsActive.Should().BeFalse();
        entity.IndexGroupName.Should().BeNull();
    }
}

public class LevelTests
{
    [Fact]
    public void Level_ShouldSetProperties()
    {
        var entity = new Level
        {
            Id = 1,
            LevelCode = "B",
            LevelName = "Bachelor",
            LevelDisplayOrder = 1,
            Remarks = "Bachelor level",
            IsRunning = true,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.LevelCode.Should().Be("B");
        entity.LevelName.Should().Be("Bachelor");
        entity.LevelDisplayOrder.Should().Be(1);
        entity.Remarks.Should().Be("Bachelor level");
        entity.IsRunning.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Level_Defaults_ShouldBeCorrect()
    {
        var entity = new Level();
        entity.IsActive.Should().BeFalse();
        entity.IsRunning.Should().BeNull();
        entity.LevelDisplayOrder.Should().BeNull();
        entity.LevelName.Should().BeNull();
    }
}

public class LocalLevelTests
{
    [Fact]
    public void LocalLevel_ShouldSetProperties()
    {
        var entity = new LocalLevel
        {
            Id = 1,
            DistrictId = 2,
            LocalLevelName = "Bedkot Municipality",
            LocalLevelType = LocalLevelType.Metropolitan,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.DistrictId.Should().Be(2);
        entity.LocalLevelName.Should().Be("Bedkot Municipality");
        entity.LocalLevelType.Should().Be(LocalLevelType.Metropolitan);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void LocalLevel_HasNavigationToDistrict()
    {
        var entity = new LocalLevel();
        entity.District.Should().BeNull();
        entity.StudentRegistrations.Should().BeNull();
        entity.Addresses.Should().BeNull();
    }

    [Fact]
    public void LocalLevel_DefaultIsActive_ShouldBeTrue()
    {
        var entity = new LocalLevel();
        entity.IsActive.Should().BeTrue();
        entity.LocalLevelType.Should().Be(default);
    }
}

public class NepaliDateTests
{
    [Fact]
    public void NepaliDate_ShouldSetProperties()
    {
        var entity = new NepaliDate
        {
            Id = 1,
            GregorianDate = new DateTime(2025, 7, 15),
            NepaliDateShort = "2081-03-31",
            NepaliDateFull = "२८८१-०३-३१",
            NepaliDateString = "२०८१ असार ३१"
        };

        entity.Id.Should().Be(1);
        entity.GregorianDate.Should().Be(new DateTime(2025, 7, 15));
        entity.NepaliDateShort.Should().Be("2081-03-31");
        entity.NepaliDateFull.Should().Be("२८८१-०३-३१");
        entity.NepaliDateString.Should().Be("२०८१ असार ३१");
    }

    [Fact]
    public void NepaliDate_Defaults_ShouldBeNull()
    {
        var entity = new NepaliDate();
        entity.GregorianDate.Should().BeNull();
        entity.NepaliDateShort.Should().BeNull();
        entity.NepaliDateFull.Should().BeNull();
        entity.NepaliDateString.Should().BeNull();
    }
}

public class PaymentTypeTests
{
    [Fact]
    public void PaymentType_ShouldSetProperties()
    {
        var entity = new PaymentType
        {
            Id = 1,
            PaymentTypeName = "eSewa",
            LogoUrl = "/logos/esewa.png",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.PaymentTypeName.Should().Be("eSewa");
        entity.LogoUrl.Should().Be("/logos/esewa.png");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void PaymentType_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new PaymentType();
        entity.IsActive.Should().BeFalse();
        entity.PaymentTypeName.Should().BeNull();
        entity.LogoUrl.Should().BeNull();
    }
}

public class PeriodTypeTests
{
    [Fact]
    public void PeriodType_ShouldSetProperties()
    {
        var entity = new PeriodType
        {
            Id = 1,
            PeriodTypeName = "Annual",
            NumberOfMonths = 12,
            IsActive = true,
            Remarks = "Annual period"
        };

        entity.Id.Should().Be(1);
        entity.PeriodTypeName.Should().Be("Annual");
        entity.NumberOfMonths.Should().Be(12);
        entity.IsActive.Should().BeTrue();
        entity.Remarks.Should().Be("Annual period");
    }

    [Fact]
    public void PeriodType_Defaults_ShouldBeNull()
    {
        var entity = new PeriodType();
        entity.PeriodTypeName.Should().BeNull();
        entity.NumberOfMonths.Should().BeNull();
        entity.IsActive.Should().BeNull();
        entity.Remarks.Should().BeNull();
    }
}

public class PreviousLevelTests
{
    [Fact]
    public void PreviousLevel_ShouldSetProperties()
    {
        var entity = new PreviousLevel
        {
            Id = 1,
            PreviousLevelName = "+2 Science",
            LevelId = 2,
            LevelDisplayOrder = 1,
            Remarks = "Plus 2 level",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.PreviousLevelName.Should().Be("+2 Science");
        entity.LevelId.Should().Be(2);
        entity.LevelDisplayOrder.Should().Be(1);
        entity.Remarks.Should().Be("Plus 2 level");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void PreviousLevel_Defaults_ShouldBeCorrect()
    {
        var entity = new PreviousLevel();
        entity.IsActive.Should().BeFalse();
        entity.LevelId.Should().BeNull();
        entity.LevelDisplayOrder.Should().BeNull();
        entity.PreviousLevelName.Should().BeNull();
    }
}

public class ProgramEntityTests
{
    [Fact]
    public void Program_ShouldSetProperties()
    {
        var entity = new Program
        {
            Id = 1,
            LevelId = 2,
            BoardId = 1,
            FacultyId = 3,
            ProgramCode = "BBS",
            ProgramName = "Bachelor of Business Studies",
            ShortName = "BBS",
            Duration = 4,
            GrandTotalMarks = 500,
            HasMultipleIntakes = false,
            NumberOfSeats = "100",
            ScholarshipSeats = 10,
            Remarks = "Regular program",
            IsActive = true,
            RollNumberPrefix = "BBS"
        };

        entity.Id.Should().Be(1);
        entity.LevelId.Should().Be(2);
        entity.BoardId.Should().Be(1);
        entity.FacultyId.Should().Be(3);
        entity.ProgramCode.Should().Be("BBS");
        entity.ProgramName.Should().Be("Bachelor of Business Studies");
        entity.ShortName.Should().Be("BBS");
        entity.Duration.Should().Be(4);
        entity.GrandTotalMarks.Should().Be(500);
        entity.HasMultipleIntakes.Should().BeFalse();
        entity.NumberOfSeats.Should().Be("100");
        entity.ScholarshipSeats.Should().Be(10);
        entity.Remarks.Should().Be("Regular program");
        entity.IsActive.Should().BeTrue();
        entity.RollNumberPrefix.Should().Be("BBS");
    }

    [Fact]
    public void Program_Defaults_ShouldBeCorrect()
    {
        var entity = new Program();
        entity.IsActive.Should().BeFalse();
        entity.HasMultipleIntakes.Should().BeFalse();
        entity.BoardId.Should().BeNull();
        entity.FacultyId.Should().BeNull();
        entity.GrandTotalMarks.Should().BeNull();
        entity.ScholarshipSeats.Should().BeNull();
        entity.ProgramCode.Should().BeNull();
        entity.ProgramName.Should().BeNull();
        entity.ShortName.Should().BeNull();
        entity.Duration.Should().Be(0);
    }
}

public class ProgramSubjectPracticalChargeTests
{
    [Fact]
    public void ProgramSubjectPracticalCharge_ShouldSetProperties()
    {
        var entity = new ProgramSubjectPracticalCharge
        {
            Id = 1,
            TenantId = 10,
            ProgramsId = 3,
            PracticalSubjectCharge = 500.00m
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ProgramsId.Should().Be(3);
        entity.PracticalSubjectCharge.Should().Be(500.00m);
    }

    [Fact]
    public void ProgramSubjectPracticalCharge_Implements_ITenantScoped()
    {
        var entity = new ProgramSubjectPracticalCharge();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ProgramSubjectPracticalCharge_Defaults_ShouldBeZero()
    {
        var entity = new ProgramSubjectPracticalCharge();
        entity.PracticalSubjectCharge.Should().Be(0);
        entity.ProgramsId.Should().Be(0);
    }
}

public class ProvinceTests
{
    [Fact]
    public void Province_ShouldSetProperties()
    {
        var entity = new Province
        {
            Id = 1,
            ProvinceName = "Sudurpashchim",
            ProvinceCode = "SUD",
            IsActive = true,
            Remarks = "Far western province"
        };

        entity.Id.Should().Be(1);
        entity.ProvinceName.Should().Be("Sudurpashchim");
        entity.ProvinceCode.Should().Be("SUD");
        entity.IsActive.Should().BeTrue();
        entity.Remarks.Should().Be("Far western province");
    }

    [Fact]
    public void Province_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new Province();
        entity.IsActive.Should().BeFalse();
        entity.ProvinceName.Should().BeNull();
        entity.ProvinceCode.Should().BeNull();
    }
}

public class QuestionSetTests
{
    [Fact]
    public void QuestionSet_ShouldSetProperties()
    {
        var entity = new QuestionSet
        {
            Id = 1,
            TenantId = 10,
            QuestionSetName = "Set A",
            Description = "Question set for regular exam",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.QuestionSetName.Should().Be("Set A");
        entity.Description.Should().Be("Question set for regular exam");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void QuestionSet_Implements_ITenantScoped()
    {
        var entity = new QuestionSet();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void QuestionSet_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new QuestionSet();
        entity.IsActive.Should().BeFalse();
        entity.QuestionSetName.Should().BeNull();
    }
}

public class ResultRecordTests
{
    [Fact]
    public void ResultRecord_ShouldSetProperties()
    {
        var entity = new ResultRecord
        {
            Id = 1,
            TenantId = 10,
            AcademicYearId = 5,
            ProgramsId = 3,
            ExamTypeId = 1,
            CollegeId = 2,
            Year = "1",
            Part = "1",
            RegistrationNumber = "REG-001",
            SymbolNumber = "SN001",
            Alphabet = "A",
            DateOfBirthBs = "2056-10-01",
            Sex = "Male",
            TheoryObtainedMarks = "75",
            InternalObtainedMarks = "15",
            PracticalObtainedMarks = "10",
            TheoryObtainedGrade = "A",
            InternalObtainedGrade = "A",
            PracticalObtainedGrade = "A",
            TotalObtainedMarks = "100",
            TotalObtainedGrade = "A",
            TotalGradePoints = "4.0",
            Gpa = "4.0",
            Result = "Passed",
            StudentName = "Ram Sharma",
            ResultRecordMasterId = 1,
            ExamScheduleId = 5,
            CreatedDate = new DateTime(2026, 7, 15)
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.AcademicYearId.Should().Be(5);
        entity.ProgramsId.Should().Be(3);
        entity.ExamTypeId.Should().Be(1);
        entity.CollegeId.Should().Be(2);
        entity.Year.Should().Be("1");
        entity.Part.Should().Be("1");
        entity.RegistrationNumber.Should().Be("REG-001");
        entity.SymbolNumber.Should().Be("SN001");
        entity.Alphabet.Should().Be("A");
        entity.DateOfBirthBs.Should().Be("2056-10-01");
        entity.Sex.Should().Be("Male");
        entity.TheoryObtainedMarks.Should().Be("75");
        entity.InternalObtainedMarks.Should().Be("15");
        entity.PracticalObtainedMarks.Should().Be("10");
        entity.TheoryObtainedGrade.Should().Be("A");
        entity.InternalObtainedGrade.Should().Be("A");
        entity.PracticalObtainedGrade.Should().Be("A");
        entity.TotalObtainedMarks.Should().Be("100");
        entity.TotalObtainedGrade.Should().Be("A");
        entity.TotalGradePoints.Should().Be("4.0");
        entity.Gpa.Should().Be("4.0");
        entity.Result.Should().Be("Passed");
        entity.StudentName.Should().Be("Ram Sharma");
        entity.ResultRecordMasterId.Should().Be(1);
        entity.ExamScheduleId.Should().Be(5);
        entity.CreatedDate.Should().Be(new DateTime(2026, 7, 15));
    }

    [Fact]
    public void ResultRecord_Implements_ITenantScoped()
    {
        var entity = new ResultRecord();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void ResultRecord_Defaults_ShouldBeCorrect()
    {
        var entity = new ResultRecord();
        entity.Year.Should().BeNull();
        entity.Part.Should().BeNull();
        entity.SymbolNumber.Should().BeNull();
        entity.DateOfBirthBs.Should().BeNull();
        entity.CreatedDate.Should().BeNull();
        entity.ExamScheduleId.Should().BeNull();
    }
}

public class SchoolTypeTests
{
    [Fact]
    public void SchoolType_ShouldSetProperties()
    {
        var entity = new SchoolType
        {
            Id = 1,
            PreviousLevelId = 2,
            SchoolTypeName = "Government School"
        };

        entity.Id.Should().Be(1);
        entity.PreviousLevelId.Should().Be(2);
        entity.SchoolTypeName.Should().Be("Government School");
    }

    [Fact]
    public void SchoolType_Defaults_ShouldBeCorrect()
    {
        var entity = new SchoolType();
        entity.SchoolTypeName.Should().BeNull();
        entity.PreviousLevel.Should().BeNull();
    }
}

public class SmsConfigurationTests
{
    [Fact]
    public void SmsConfiguration_ShouldSetProperties()
    {
        var entity = new SmsConfiguration
        {
            Id = 1,
            ApiUrl = "https://sms.example.com/api",
            ApiKey = "secret-key",
            Mode = "live",
            Tags = "tag1,tag2",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.ApiUrl.Should().Be("https://sms.example.com/api");
        entity.ApiKey.Should().Be("secret-key");
        entity.Mode.Should().Be("live");
        entity.Tags.Should().Be("tag1,tag2");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SmsConfiguration_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new SmsConfiguration();
        entity.IsActive.Should().BeFalse();
        entity.ApiUrl.Should().BeNull();
        entity.ApiKey.Should().BeNull();
    }
}

public class SmtpConfigurationTests
{
    [Fact]
    public void SmtpConfiguration_ShouldSetProperties()
    {
        var entity = new SmtpConfiguration
        {
            Id = 1,
            Host = "smtp.example.com",
            From = "noreply@fwu.edu.np",
            Port = 587,
            UserName = "smtp-user",
            Password = "smtp-pass",
            EnableSsl = true,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.Host.Should().Be("smtp.example.com");
        entity.From.Should().Be("noreply@fwu.edu.np");
        entity.Port.Should().Be(587);
        entity.UserName.Should().Be("smtp-user");
        entity.Password.Should().Be("smtp-pass");
        entity.EnableSsl.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SmtpConfiguration_Defaults_ShouldBeCorrect()
    {
        var entity = new SmtpConfiguration();
        entity.IsActive.Should().BeFalse();
        entity.EnableSsl.Should().BeFalse();
        entity.Port.Should().Be(0);
        entity.Host.Should().BeNull();
        entity.From.Should().BeNull();
        entity.UserName.Should().BeNull();
        entity.Password.Should().BeNull();
    }
}

public class SubjectCatalogTests
{
    [Fact]
    public void SubjectCatalog_ShouldSetProperties()
    {
        var entity = new SubjectCatalog
        {
            Id = 1,
            SubjectCode = "MTH101",
            SubjectName = "Mathematics I",
            ShortName = "Maths",
            Description = "First semester mathematics",
            CreditHours = 3,
            SubjectTypeId = 1,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.SubjectCode.Should().Be("MTH101");
        entity.SubjectName.Should().Be("Mathematics I");
        entity.ShortName.Should().Be("Maths");
        entity.Description.Should().Be("First semester mathematics");
        entity.CreditHours.Should().Be(3);
        entity.SubjectTypeId.Should().Be(1);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SubjectCatalog_Defaults_ShouldBeCorrect()
    {
        var entity = new SubjectCatalog();
        entity.SubjectName.Should().Be(string.Empty);
        entity.IsActive.Should().BeFalse();
        entity.CreditHours.Should().BeNull();
        entity.SubjectCode.Should().BeNull();
    }
}

public class SubjectOfferingTests
{
    [Fact]
    public void SubjectOffering_ShouldSetProperties()
    {
        var entity = new SubjectOffering
        {
            Id = 1,
            TenantId = 10,
            SubjectCatalogId = 5,
            ProgramId = 3,
            SemesterId = 2,
            IsCompulsory = true,
            DisplayOrder = 1,
            HasTheory = true,
            HasPractical = true,
            HasInternal = true,
            TheoryFullMarks = 80,
            TheoryPassMarks = 32,
            PracticalFullMarks = 20,
            PracticalPassMarks = 8,
            InternalTheoryFullMarks = 20,
            InternalTheoryPassMarks = 8,
            InternalPracticalFullMarks = 10,
            InternalPracticalPassMarks = 4
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.SubjectCatalogId.Should().Be(5);
        entity.ProgramId.Should().Be(3);
        entity.SemesterId.Should().Be(2);
        entity.IsCompulsory.Should().BeTrue();
        entity.DisplayOrder.Should().Be(1);
        entity.HasTheory.Should().BeTrue();
        entity.HasPractical.Should().BeTrue();
        entity.HasInternal.Should().BeTrue();
        entity.TheoryFullMarks.Should().Be(80);
        entity.TheoryPassMarks.Should().Be(32);
        entity.PracticalFullMarks.Should().Be(20);
        entity.PracticalPassMarks.Should().Be(8);
        entity.InternalTheoryFullMarks.Should().Be(20);
        entity.InternalTheoryPassMarks.Should().Be(8);
        entity.InternalPracticalFullMarks.Should().Be(10);
        entity.InternalPracticalPassMarks.Should().Be(4);
    }

    [Fact]
    public void SubjectOffering_Implements_ITenantScoped()
    {
        var entity = new SubjectOffering();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void SubjectOffering_Defaults_ShouldBeCorrect()
    {
        var entity = new SubjectOffering();
        entity.IsCompulsory.Should().BeFalse();
        entity.HasTheory.Should().BeFalse();
        entity.HasPractical.Should().BeFalse();
        entity.HasInternal.Should().BeFalse();
        entity.DisplayOrder.Should().Be(0);
        entity.TheoryFullMarks.Should().Be(0);
        entity.TheoryPassMarks.Should().Be(0);
        entity.PracticalFullMarks.Should().BeNull();
        entity.PracticalPassMarks.Should().BeNull();
        entity.InternalTheoryFullMarks.Should().BeNull();
        entity.InternalTheoryPassMarks.Should().BeNull();
        entity.InternalPracticalFullMarks.Should().BeNull();
        entity.InternalPracticalPassMarks.Should().BeNull();
    }
}

public class SubjectTypeTests
{
    [Fact]
    public void SubjectType_ShouldSetProperties()
    {
        var entity = new SubjectType
        {
            Id = 1,
            Code = "TH",
            Name = "Theory",
            MaxAllowedSubjects = 8,
            IsDefault = true,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.Code.Should().Be("TH");
        entity.Name.Should().Be("Theory");
        entity.MaxAllowedSubjects.Should().Be(8);
        entity.IsDefault.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void SubjectType_Defaults_ShouldBeCorrect()
    {
        var entity = new SubjectType();
        entity.Code.Should().Be(string.Empty);
        entity.Name.Should().Be(string.Empty);
        entity.IsDefault.Should().BeFalse();
        entity.IsActive.Should().BeFalse();
        entity.MaxAllowedSubjects.Should().BeNull();
    }
}

public class UserAttachmentTests
{
    [Fact]
    public void UserAttachment_ShouldSetProperties()
    {
        var entity = new UserAttachment
        {
            Id = 1,
            FileName = "photo.jpg",
            FilePath = "/uploads/photo.jpg",
            ContentType = "image/jpeg",
            FileSize = 102400,
            UploadedByUserId = "user-1",
            UploadedDate = new DateTime(2026, 7, 15, 10, 0, 0),
            Remarks = "Profile photo"
        };

        entity.Id.Should().Be(1);
        entity.FileName.Should().Be("photo.jpg");
        entity.FilePath.Should().Be("/uploads/photo.jpg");
        entity.ContentType.Should().Be("image/jpeg");
        entity.FileSize.Should().Be(102400);
        entity.UploadedByUserId.Should().Be("user-1");
        entity.UploadedDate.Should().Be(new DateTime(2026, 7, 15, 10, 0, 0));
        entity.Remarks.Should().Be("Profile photo");
    }

    [Fact]
    public void UserAttachment_UploadedDateDefault_ShouldBeMinValue()
    {
        var entity = new UserAttachment();
        entity.UploadedDate.Should().Be(default);
        entity.FileSize.Should().BeNull();
        entity.FileName.Should().BeNull();
        entity.FilePath.Should().BeNull();
        entity.UploadedByUserId.Should().BeNull();
    }
}

public class PermissionTests
{
    [Fact]
    public void Permission_ShouldSetProperties()
    {
        var entity = new Permission
        {
            Id = 1,
            Name = "students.view",
            DisplayName = "View Students",
            Description = "View student registrations",
            Group = "students",
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.Name.Should().Be("students.view");
        entity.DisplayName.Should().Be("View Students");
        entity.Description.Should().Be("View student registrations");
        entity.Group.Should().Be("students");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Permission_DefaultIsActive_ShouldBeTrue()
    {
        var entity = new Permission();
        entity.IsActive.Should().BeTrue();
        entity.Name.Should().Be(string.Empty);
        entity.Group.Should().Be(string.Empty);
        entity.DisplayName.Should().BeNull();
    }
}

public class RolePermissionTests
{
    [Fact]
    public void RolePermission_ShouldSetProperties()
    {
        var entity = new RolePermission
        {
            RoleId = "admin",
            PermissionId = 5
        };

        entity.RoleId.Should().Be("admin");
        entity.PermissionId.Should().Be(5);
    }

    [Fact]
    public void RolePermission_RoleIdDefault_ShouldBeEmpty()
    {
        var entity = new RolePermission();
        entity.RoleId.Should().Be(string.Empty);
    }
}

public class PaymentRequestLogTests
{
    [Fact]
    public void PaymentRequestLog_ShouldSetProperties()
    {
        var entity = new PaymentRequestLog
        {
            Id = 1,
            TenantId = 10,
            PaymentRequestLogStatus = 1,
            InvoiceNumber = "INV-001",
            ForwardedTimestamp = new DateTime(2026, 7, 15, 10, 0, 0),
            DateOfBirthAd = new DateTime(2000, 1, 15),
            MobileNumber = "9841234567",
            Email = "ram@example.com",
            FullName = "Ram Sharma",
            Amount = 1500.00m,
            FullRequestContent = "{\"key\":\"value\"}",
            PaymentTypeId = 1,
            StudentRegistrationId = 100,
            ExamScheduleId = 5,
            TransactionId = "TXN-001",
            CollegeId = 3,
            StudentCount = 1,
            SelectedSubjectIds = "1,2,3"
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.PaymentRequestLogStatus.Should().Be(1);
        entity.InvoiceNumber.Should().Be("INV-001");
        entity.ForwardedTimestamp.Should().Be(new DateTime(2026, 7, 15, 10, 0, 0));
        entity.DateOfBirthAd.Should().Be(new DateTime(2000, 1, 15));
        entity.MobileNumber.Should().Be("9841234567");
        entity.Email.Should().Be("ram@example.com");
        entity.FullName.Should().Be("Ram Sharma");
        entity.Amount.Should().Be(1500.00m);
        entity.FullRequestContent.Should().Be("{\"key\":\"value\"}");
        entity.PaymentTypeId.Should().Be(1);
        entity.StudentRegistrationId.Should().Be(100);
        entity.ExamScheduleId.Should().Be(5);
        entity.TransactionId.Should().Be("TXN-001");
        entity.CollegeId.Should().Be(3);
        entity.StudentCount.Should().Be(1);
        entity.SelectedSubjectIds.Should().Be("1,2,3");
    }

    [Fact]
    public void PaymentRequestLog_Implements_ITenantScoped()
    {
        var entity = new PaymentRequestLog();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void PaymentRequestLog_Defaults_ShouldBeCorrect()
    {
        var entity = new PaymentRequestLog();
        entity.Amount.Should().Be(0);
        entity.StudentCount.Should().Be(0);
        entity.PaymentRequestLogStatus.Should().BeNull();
        entity.DateOfBirthAd.Should().BeNull();
        entity.StudentRegistrationId.Should().BeNull();
        entity.CollegeId.Should().BeNull();
        entity.TransactionId.Should().BeNull();
        entity.InvoiceNumber.Should().BeNull();
        entity.FullName.Should().BeNull();
        entity.FullRequestContent.Should().BeNull();
    }
}

public class StudentRegistrationTests
{
    [Fact]
    public void StudentRegistration_ShouldSetProperties()
    {
        var entity = new StudentRegistration
        {
            Id = 1,
            TenantId = 10,
            LevelId = 2,
            CollegeId = 3,
            FacultyId = 1,
            ProgramId = 4,
            RegistrationNumber = "REG-001",
            FirstName = "Ram",
            MiddleName = "Prasad",
            LastName = "Sharma",
            NepaliName = "राम प्रसाद शर्मा",
            ContactNumber = "9841234567",
            Phone = "099-525000",
            Email = "ram@example.com",
            DateOfBirthBS = "2056-10-01",
            DateOfBirthAD = "2000-01-15",
            GenderId = 1,
            BloodGroup = "O+",
            Nationality = "Nepali",
            Religion = "Hindu",
            PermanentAddressId = 10,
            CurrentAddressId = 11,
            IsActive = true,
            StudentCategoryId = 1,
            VerifiedBy = 1,
            VerifiedDate = new DateTime(2026, 7, 15),
            EthnicityId = 2,
            EntranceRollNumber = "ENT-001",
            IsRegistrationNumberGenerated = true,
            StudentRegistrationIndex = 1,
            AcademicYearId = 5,
            StudentAdmissionId = 10
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.LevelId.Should().Be(2);
        entity.CollegeId.Should().Be(3);
        entity.FacultyId.Should().Be(1);
        entity.ProgramId.Should().Be(4);
        entity.RegistrationNumber.Should().Be("REG-001");
        entity.FirstName.Should().Be("Ram");
        entity.MiddleName.Should().Be("Prasad");
        entity.LastName.Should().Be("Sharma");
        entity.NepaliName.Should().Be("राम प्रसाद शर्मा");
        entity.ContactNumber.Should().Be("9841234567");
        entity.Phone.Should().Be("099-525000");
        entity.Email.Should().Be("ram@example.com");
        entity.DateOfBirthBS.Should().Be("2056-10-01");
        entity.DateOfBirthAD.Should().Be("2000-01-15");
        entity.GenderId.Should().Be(1);
        entity.BloodGroup.Should().Be("O+");
        entity.Nationality.Should().Be("Nepali");
        entity.Religion.Should().Be("Hindu");
        entity.PermanentAddressId.Should().Be(10);
        entity.CurrentAddressId.Should().Be(11);
        entity.IsActive.Should().BeTrue();
        entity.StudentCategoryId.Should().Be(1);
        entity.VerifiedBy.Should().Be(1);
        entity.VerifiedDate.Should().Be(new DateTime(2026, 7, 15));
        entity.EthnicityId.Should().Be(2);
        entity.EntranceRollNumber.Should().Be("ENT-001");
        entity.IsRegistrationNumberGenerated.Should().BeTrue();
        entity.StudentRegistrationIndex.Should().Be(1);
        entity.AcademicYearId.Should().Be(5);
        entity.StudentAdmissionId.Should().Be(10);
    }

    [Fact]
    public void StudentRegistration_Implements_ITenantScoped()
    {
        var entity = new StudentRegistration();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void StudentRegistration_Defaults_ShouldBeCorrect()
    {
        var entity = new StudentRegistration();
        entity.IsActive.Should().BeFalse();
        entity.IsRegistrationNumberGenerated.Should().BeNull();
        entity.FacultyId.Should().BeNull();
        entity.ProgramId.Should().BeNull();
        entity.DateOfBirthBS.Should().Be(string.Empty);
        entity.FirstName.Should().BeNull();
        entity.LastName.Should().BeNull();
        entity.VerifiedBy.Should().BeNull();
        entity.VerifiedDate.Should().BeNull();
        entity.EthnicityId.Should().BeNull();
        entity.StudentAdmissionId.Should().BeNull();
        entity.RegistrationNumber.Should().BeNull();
    }
}

public class StudentAdmissionTests
{
    [Fact]
    public void StudentAdmission_ShouldSetProperties()
    {
        var entity = new StudentAdmission
        {
            Id = 1,
            TenantId = 10,
            ProgramsId = 3,
            CollegeId = 2,
            AcademicYearId = 5,
            AdmissionDate = new DateTime(2026, 7, 15),
            CheckedBy = 1,
            IsCompleted = true,
            IsActive = true,
            CollegeRollNumber = "CR-001",
            HasFeeExemption = false,
            AppUserId = "user-1"
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.ProgramsId.Should().Be(3);
        entity.CollegeId.Should().Be(2);
        entity.AcademicYearId.Should().Be(5);
        entity.AdmissionDate.Should().Be(new DateTime(2026, 7, 15));
        entity.CheckedBy.Should().Be(1);
        entity.IsCompleted.Should().BeTrue();
        entity.IsActive.Should().BeTrue();
        entity.CollegeRollNumber.Should().Be("CR-001");
        entity.HasFeeExemption.Should().BeFalse();
        entity.AppUserId.Should().Be("user-1");
    }

    [Fact]
    public void StudentAdmission_Implements_IAuditable()
    {
        var entity = new StudentAdmission();
        entity.Should().BeAssignableTo<IAuditable>();
    }

    [Fact]
    public void StudentAdmission_Implements_ITenantScoped()
    {
        var entity = new StudentAdmission();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void StudentAdmission_Defaults_ShouldBeCorrect()
    {
        var entity = new StudentAdmission();
        entity.IsActive.Should().BeFalse();
        entity.IsCompleted.Should().BeFalse();
        entity.HasFeeExemption.Should().BeFalse();
        entity.AdmissionDate.Should().Be(default);
        entity.CheckedBy.Should().BeNull();
        entity.AppUserId.Should().BeNull();
    }
}

public class StudentQualificationTests
{
    [Fact]
    public void StudentQualification_ShouldSetProperties()
    {
        var entity = new StudentQualification
        {
            Id = 1,
            TenantId = 10,
            StudentRegistrationId = 5,
            BoardId = 2,
            PreviousLevelId = 3,
            ProgramName = "Science",
            InstituteName = "ABC College",
            PassedYear = "2077",
            Specialization = "Biology",
            Percentage = 85.5m,
            TotalCredits = "32",
            Remarks = "Good",
            IsHigherDegree = false,
            IsActive = true,
            DocumentPath = "/docs/qual.pdf",
            ExamRollNumber = "EX-001"
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.StudentRegistrationId.Should().Be(5);
        entity.BoardId.Should().Be(2);
        entity.PreviousLevelId.Should().Be(3);
        entity.ProgramName.Should().Be("Science");
        entity.InstituteName.Should().Be("ABC College");
        entity.PassedYear.Should().Be("2077");
        entity.Specialization.Should().Be("Biology");
        entity.Percentage.Should().Be(85.5m);
        entity.TotalCredits.Should().Be("32");
        entity.Remarks.Should().Be("Good");
        entity.IsHigherDegree.Should().BeFalse();
        entity.IsActive.Should().BeTrue();
        entity.DocumentPath.Should().Be("/docs/qual.pdf");
        entity.ExamRollNumber.Should().Be("EX-001");
    }

    [Fact]
    public void StudentQualification_Implements_ITenantScoped()
    {
        var entity = new StudentQualification();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void StudentQualification_Defaults_ShouldBeCorrect()
    {
        var entity = new StudentQualification();
        entity.IsActive.Should().BeFalse();
        entity.IsHigherDegree.Should().BeFalse();
        entity.InstituteName.Should().BeNull();
        entity.Percentage.Should().BeNull();
    }
}

public class StudentGuardianTests
{
    [Fact]
    public void StudentGuardian_ShouldSetProperties()
    {
        var entity = new StudentGuardian
        {
            Id = 1,
            TenantId = 10,
            StudentRegistrationId = 5,
            FatherName = "Hari Sharma",
            FatherContactNumber = "9851234567",
            FatherPhone = "099-525000",
            FatherEmail = "hari@example.com",
            FatherQualification = "MA",
            FatherProfession = "Teacher",
            FatherAddress = "Kanchanpur",
            FatherOrganization = "School",
            FatherOrganizationAddress = "Kanchanpur",
            MotherName = "Sita Sharma",
            MotherContactNumber = "9861234567",
            MotherPhone = "099-525001",
            MotherEmail = "sita@example.com",
            MotherQualification = "BA",
            MotherProfession = "Housewife",
            MotherAddress = "Kanchanpur",
            MotherOrganization = "N/A",
            MotherOrganizationAddress = "Kanchanpur",
            GuardianName = "Shyam Sharma",
            GuardianContactNumber = "9871234567",
            GuardianPhone = "099-525002",
            GuardianEmail = "shyam@example.com",
            GuardianQualification = "MSc",
            GuardianProfession = "Engineer",
            GuardianAddress = "Kanchanpur",
            GuardianOrganization = "Company",
            GuardianOrganizationAddress = "Kanchanpur",
            RelationWithStudent = "Uncle"
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.StudentRegistrationId.Should().Be(5);
        entity.FatherName.Should().Be("Hari Sharma");
        entity.FatherContactNumber.Should().Be("9851234567");
        entity.FatherPhone.Should().Be("099-525000");
        entity.FatherEmail.Should().Be("hari@example.com");
        entity.FatherQualification.Should().Be("MA");
        entity.FatherProfession.Should().Be("Teacher");
        entity.FatherAddress.Should().Be("Kanchanpur");
        entity.FatherOrganization.Should().Be("School");
        entity.FatherOrganizationAddress.Should().Be("Kanchanpur");
        entity.MotherName.Should().Be("Sita Sharma");
        entity.MotherContactNumber.Should().Be("9861234567");
        entity.MotherPhone.Should().Be("099-525001");
        entity.MotherEmail.Should().Be("sita@example.com");
        entity.MotherQualification.Should().Be("BA");
        entity.MotherProfession.Should().Be("Housewife");
        entity.MotherAddress.Should().Be("Kanchanpur");
        entity.MotherOrganization.Should().Be("N/A");
        entity.MotherOrganizationAddress.Should().Be("Kanchanpur");
        entity.GuardianName.Should().Be("Shyam Sharma");
        entity.GuardianContactNumber.Should().Be("9871234567");
        entity.GuardianPhone.Should().Be("099-525002");
        entity.GuardianEmail.Should().Be("shyam@example.com");
        entity.GuardianQualification.Should().Be("MSc");
        entity.GuardianProfession.Should().Be("Engineer");
        entity.GuardianAddress.Should().Be("Kanchanpur");
        entity.GuardianOrganization.Should().Be("Company");
        entity.GuardianOrganizationAddress.Should().Be("Kanchanpur");
        entity.RelationWithStudent.Should().Be("Uncle");
    }

    [Fact]
    public void StudentGuardian_Implements_IAuditable()
    {
        var entity = new StudentGuardian();
        entity.Should().BeAssignableTo<IAuditable>();
    }

    [Fact]
    public void StudentGuardian_Implements_ITenantScoped()
    {
        var entity = new StudentGuardian();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void StudentGuardian_RequiredFields_ShouldBeNullByDefault()
    {
        var entity = new StudentGuardian();
        entity.FatherName.Should().BeNull();
        entity.MotherName.Should().BeNull();
        entity.GuardianName.Should().BeNull();
        entity.RelationWithStudent.Should().BeNull();
    }
}

public class StudentCategoryTests
{
    [Fact]
    public void StudentCategory_ShouldSetProperties()
    {
        var entity = new StudentCategory
        {
            Id = 1,
            StudentCategoryName = "General",
            IsActive = true,
            Remarks = "General category"
        };

        entity.Id.Should().Be(1);
        entity.StudentCategoryName.Should().Be("General");
        entity.IsActive.Should().BeTrue();
        entity.Remarks.Should().Be("General category");
    }

    [Fact]
    public void StudentCategory_DefaultIsActive_ShouldBeFalse()
    {
        var entity = new StudentCategory();
        entity.IsActive.Should().BeFalse();
        entity.StudentCategoryName.Should().BeNull();
    }
}

public class SemesterTests
{
    [Fact]
    public void Semester_ShouldSetProperties()
    {
        var entity = new Semester
        {
            Id = 1,
            Number = 1,
            Year = 1,
            Name = "First Semester",
            Code = "SEM1",
            Remark = "First semester of first year",
            StartDate = new DateTime(2025, 7, 15),
            EndDate = new DateTime(2025, 12, 15),
            AcademicYearId = 5,
            FacultyId = 1
        };

        entity.Id.Should().Be(1);
        entity.Number.Should().Be(1);
        entity.Year.Should().Be(1);
        entity.Name.Should().Be("First Semester");
        entity.Code.Should().Be("SEM1");
        entity.Remark.Should().Be("First semester of first year");
        entity.StartDate.Should().Be(new DateTime(2025, 7, 15));
        entity.EndDate.Should().Be(new DateTime(2025, 12, 15));
        entity.AcademicYearId.Should().Be(5);
        entity.FacultyId.Should().Be(1);
    }

    [Fact]
    public void Semester_Defaults_ShouldBeCorrect()
    {
        var entity = new Semester();
        entity.Number.Should().Be(0);
        entity.Year.Should().Be(0);
        entity.StartDate.Should().Be(default);
        entity.EndDate.Should().Be(default);
        entity.Name.Should().BeNull();
        entity.Code.Should().BeNull();
        entity.FacultyId.Should().BeNull();
    }
}

public class SemesterEnrollmentTests
{
    [Fact]
    public void SemesterEnrollment_ShouldSetProperties()
    {
        var entity = new SemesterEnrollment
        {
            Id = 1,
            TenantId = 10,
            StudentAdmissionId = 5,
            SemesterId = 2,
            EnrollmentStatus = StudentEnrollmentStatus.Active,
            EnrollmentType = EnrollmentType.FullTime,
            PaymentStatus = PaymentStatus.Paid,
            EnrolledDate = new DateTime(2026, 7, 15),
            DropDate = null,
            DropReason = null,
            SemesterResultDate = null,
            TotalCredits = 15.0,
            GradePoints = 3.5,
            TotalFee = 50000.0,
            PaidAmount = 50000.0,
            Deficiency = false,
            ResultStatus = ResultStatus.Passed
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.StudentAdmissionId.Should().Be(5);
        entity.SemesterId.Should().Be(2);
        entity.EnrollmentStatus.Should().Be(StudentEnrollmentStatus.Active);
        entity.EnrollmentType.Should().Be(EnrollmentType.FullTime);
        entity.PaymentStatus.Should().Be(PaymentStatus.Paid);
        entity.EnrolledDate.Should().Be(new DateTime(2026, 7, 15));
        entity.DropDate.Should().BeNull();
        entity.DropReason.Should().BeNull();
        entity.SemesterResultDate.Should().BeNull();
        entity.TotalCredits.Should().Be(15.0);
        entity.GradePoints.Should().Be(3.5);
        entity.TotalFee.Should().Be(50000.0);
        entity.PaidAmount.Should().Be(50000.0);
        entity.Deficiency.Should().BeFalse();
        entity.ResultStatus.Should().Be(ResultStatus.Passed);
    }

    [Fact]
    public void SemesterEnrollment_Implements_ITenantScoped()
    {
        var entity = new SemesterEnrollment();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void SemesterEnrollment_Defaults_ShouldBeCorrect()
    {
        var entity = new SemesterEnrollment();
        entity.EnrollmentStatus.Should().Be(default(StudentEnrollmentStatus));
        entity.EnrollmentType.Should().Be(default(EnrollmentType));
        entity.PaymentStatus.Should().Be(default(PaymentStatus));
        entity.ResultStatus.Should().Be(default(ResultStatus));
        entity.EnrolledDate.Should().Be(default);
        entity.Deficiency.Should().BeFalse();
        entity.TotalCredits.Should().Be(0);
        entity.GradePoints.Should().Be(0);
        entity.TotalFee.Should().Be(0);
        entity.PaidAmount.Should().Be(0);
    }
}

public class CollegeAdminSubjectAssignmentTests
{
    [Fact]
    public void CollegeAdminSubjectAssignment_ShouldSetProperties()
    {
        var entity = new CollegeAdminSubjectAssignment
        {
            Id = 1,
            TenantId = 10,
            CollegeAdminUserId = "admin-user-1",
            SubjectOfferingId = 5,
            ExamScheduleId = 3,
            IsActive = true
        };

        entity.Id.Should().Be(1);
        entity.TenantId.Should().Be(10);
        entity.CollegeAdminUserId.Should().Be("admin-user-1");
        entity.SubjectOfferingId.Should().Be(5);
        entity.ExamScheduleId.Should().Be(3);
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CollegeAdminSubjectAssignment_Implements_ITenantScoped()
    {
        var entity = new CollegeAdminSubjectAssignment();
        entity.Should().BeAssignableTo<ITenantScoped>();
    }

    [Fact]
    public void CollegeAdminSubjectAssignment_Defaults_ShouldBeCorrect()
    {
        var entity = new CollegeAdminSubjectAssignment();
        entity.CollegeAdminUserId.Should().Be(string.Empty);
        entity.IsActive.Should().BeFalse();
        entity.ExamScheduleId.Should().BeNull();
    }
}
