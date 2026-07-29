using FWU.Exam.Management.Domain.Entities;
using FluentAssertions;

namespace FWU.Exam.Management.Domain.Tests.Entities;

public class AcademicYearTests
{
    [Fact]
    public void CreateAcademicYear_ShouldSetProperties()
    {
        var year = new AcademicYear
        {
            Id = 1,
            AcademicYearCode = "2081/082",
            AcademicYearName = "2081/082",
            AcademicYearCodeNepali = "२०८१/०८२",
            AcademicYearNameNepali = "२०८१/०८२",
            IsRunning = true,
            IsActive = true,
            Remark = "Current academic year"
        };

        year.Id.Should().Be(1);
        year.AcademicYearCode.Should().Be("2081/082");
        year.AcademicYearName.Should().Be("2081/082");
        year.AcademicYearCodeNepali.Should().Be("२०८१/०८२");
        year.AcademicYearNameNepali.Should().Be("२०८१/०८२");
        year.IsRunning.Should().BeTrue();
        year.IsActive.Should().BeTrue();
        year.Remark.Should().Be("Current academic year");
    }

    [Fact]
    public void AcademicYear_DefaultIsRunning_ShouldBeFalse()
    {
        var year = new AcademicYear();
        year.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void AcademicYear_DefaultIsActive_ShouldBeFalse()
    {
        var year = new AcademicYear();
        year.IsActive.Should().BeFalse();
    }

    [Fact]
    public void AcademicYear_Collections_ShouldBeNullByDefault()
    {
        var year = new AcademicYear();
        year.Batches.Should().BeNull();
        year.ExamRegistrations.Should().BeNull();
        year.ExamSchedules.Should().BeNull();
        year.StudentRegistrations.Should().BeNull();
    }
}
