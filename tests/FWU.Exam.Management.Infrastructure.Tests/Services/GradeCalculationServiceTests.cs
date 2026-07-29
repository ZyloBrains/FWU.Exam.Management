using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class GradeCalculationServiceTests : TestBase
{
    private static SubjectOffering CreateDefaultOffering() => new()
    {
        ProgramId = 1,
        TheoryFullMarks = 80,
        PracticalFullMarks = 20
    };

    private static GradingScheme CreateStandardGradingScheme() => new()
    {
        Id = 1,
        ProgramId = 1,
        IsActive = true,
        GradeDefinitions = new List<GradeDefinition>
        {
            new() { GradeLetter = "A", MinPercentage = 80, MaxPercentage = 100, GradePoint = 4.0m, IsPass = true, Remark = "Excellent", DisplayOrder = 1 },
            new() { GradeLetter = "B", MinPercentage = 60, MaxPercentage = 79.99m, GradePoint = 3.0m, IsPass = true, Remark = "Good", DisplayOrder = 2 },
            new() { GradeLetter = "C", MinPercentage = 40, MaxPercentage = 59.99m, GradePoint = 2.0m, IsPass = true, Remark = "Satisfactory", DisplayOrder = 3 },
            new() { GradeLetter = "D", MinPercentage = 0, MaxPercentage = 39.99m, GradePoint = 1.0m, IsPass = false, Remark = "Fail", DisplayOrder = 4 }
        }
    };

    [Fact]
    public void CalculateGrade_WithGradingScheme_ShouldReturnGradeAForHighMarks()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = CreateDefaultOffering();
        var gradingScheme = CreateStandardGradingScheme();

        var result = service.CalculateGrade(85, offering, gradingScheme);

        result.GradeLetter.Should().Be("A");
        result.GradePoint.Should().Be(4.0m);
        result.IsPass.Should().BeTrue();
        result.Remark.Should().Be("Excellent");
    }

    [Fact]
    public void CalculateGrade_WithGradingScheme_ShouldReturnGradeDForLowMarks()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = CreateDefaultOffering();
        var gradingScheme = CreateStandardGradingScheme();

        var result = service.CalculateGrade(15, offering, gradingScheme);

        result.GradeLetter.Should().Be("D");
        result.GradePoint.Should().Be(1.0m);
        result.IsPass.Should().BeFalse();
        result.Remark.Should().Be("Fail");
    }

    [Fact]
    public void CalculateGrade_WithGradingScheme_ShouldReturnGradeBForMidMarks()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = CreateDefaultOffering();
        var gradingScheme = CreateStandardGradingScheme();

        var result = service.CalculateGrade(70, offering, gradingScheme);

        result.GradeLetter.Should().Be("B");
        result.GradePoint.Should().Be(3.0m);
        result.IsPass.Should().BeTrue();
    }

    [Fact]
    public void CalculateGrade_WithNoMatchingGradeDefinitions_ShouldFallbackToHardcoded()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = CreateDefaultOffering();
        var gradingScheme = new GradingScheme
        {
            Id = 1,
            ProgramId = 1,
            IsActive = true,
            GradeDefinitions = new List<GradeDefinition>()
        };

        var result = service.CalculateGrade(85, offering, gradingScheme);

        result.GradeLetter.Should().Be("C");
        result.GradePoint.Should().Be(2.0m);
        result.IsPass.Should().BeTrue();
        result.Remark.Should().Be("Pass");
    }

    [Fact]
    public void CalculateGrade_WithNullGradingScheme_ShouldFallbackToHardcodedPass()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = CreateDefaultOffering();

        var result = service.CalculateGrade(50, offering, null);

        result.GradeLetter.Should().Be("C");
        result.GradePoint.Should().Be(2.0m);
        result.IsPass.Should().BeTrue();
    }

    [Fact]
    public void CalculateGrade_WithNullGradingScheme_ShouldFallbackToHardcodedFail()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = CreateDefaultOffering();

        var result = service.CalculateGrade(20, offering, null);

        result.GradeLetter.Should().Be("F");
        result.GradePoint.Should().Be(0.0m);
        result.IsPass.Should().BeFalse();
    }

    [Fact]
    public void CalculateGrade_AtExactPassBoundary_ShouldPass()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = CreateDefaultOffering();

        var result = service.CalculateGrade(40, offering, null);

        result.GradeLetter.Should().Be("C");
        result.GradePoint.Should().Be(2.0m);
        result.IsPass.Should().BeTrue();
    }

    [Fact]
    public void CalculateGrade_BelowPassBoundary_ShouldFail()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = CreateDefaultOffering();

        var result = service.CalculateGrade(39.9f, offering, null);

        result.GradeLetter.Should().Be("F");
        result.GradePoint.Should().Be(0.0m);
        result.IsPass.Should().BeFalse();
    }

    [Fact]
    public void CalculateGrade_WithGradingScheme_ShouldUseDisplayOrderForOverlappingRanges()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = CreateDefaultOffering();
        var gradingScheme = new GradingScheme
        {
            Id = 1,
            ProgramId = 1,
            IsActive = true,
            GradeDefinitions = new List<GradeDefinition>
            {
                new() { GradeLetter = "B", MinPercentage = 60, MaxPercentage = 100, GradePoint = 3.0m, IsPass = true, DisplayOrder = 2 },
                new() { GradeLetter = "A", MinPercentage = 80, MaxPercentage = 100, GradePoint = 4.0m, IsPass = true, DisplayOrder = 1 }
            }
        };

        var result = service.CalculateGrade(85, offering, gradingScheme);

        result.GradeLetter.Should().Be("A");
        result.GradePoint.Should().Be(4.0m);
    }

    [Fact]
    public void CalculateGrade_WithPracticalAndInternalComponents_ShouldIncludeAllInTotal()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = new SubjectOffering
        {
            ProgramId = 1,
            TheoryFullMarks = 60,
            PracticalFullMarks = 20,
            InternalTheoryFullMarks = 10,
            InternalPracticalFullMarks = 10
        };

        var result = service.CalculateGrade(85, offering, null);

        result.GradeLetter.Should().Be("C");
        result.IsPass.Should().BeTrue();
    }

    [Fact]
    public void CalculateGrade_WithZeroFullMarks_ShouldNotDivideByZero()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = new SubjectOffering
        {
            ProgramId = 1,
            TheoryFullMarks = 0,
            PracticalFullMarks = 0
        };

        var result = service.CalculateGrade(0, offering, null);

        result.Should().NotBeNull();
    }

    [Fact]
    public void CalculateTotalMarks_WithAllComponents_ShouldSumCorrectly()
    {
        var service = new GradeCalculationService(CreateContext());

        var result = service.CalculateTotalMarks(60f, 15f, 10f, 5f);

        result.Should().Be(90f);
    }

    [Fact]
    public void CalculateTotalMarks_WithPartialComponents_ShouldIgnoreNulls()
    {
        var service = new GradeCalculationService(CreateContext());

        var result = service.CalculateTotalMarks(60f, null, 10f, null);

        result.Should().Be(70f);
    }

    [Fact]
    public void CalculateTotalMarks_WithAllNull_ShouldReturnZero()
    {
        var service = new GradeCalculationService(CreateContext());

        var result = service.CalculateTotalMarks(null, null, null, null);

        result.Should().Be(0f);
    }

    [Fact]
    public void IsStudentPassing_WhenBothComponentsPass_ShouldReturnTrue()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = new SubjectOffering
        {
            HasTheory = true,
            TheoryPassMarks = 32,
            HasPractical = true,
            PracticalPassMarks = 8
        };

        var result = service.IsStudentPassing(40f, 10f, offering);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsStudentPassing_WhenTheoryFails_ShouldReturnFalse()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = new SubjectOffering
        {
            HasTheory = true,
            TheoryPassMarks = 32,
            HasPractical = true,
            PracticalPassMarks = 8
        };

        var result = service.IsStudentPassing(20f, 10f, offering);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsStudentPassing_WhenPracticalFails_ShouldReturnFalse()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = new SubjectOffering
        {
            HasTheory = true,
            TheoryPassMarks = 32,
            HasPractical = true,
            PracticalPassMarks = 8
        };

        var result = service.IsStudentPassing(40f, 5f, offering);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsStudentPassing_WhenBothFail_ShouldReturnFalse()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = new SubjectOffering
        {
            HasTheory = true,
            TheoryPassMarks = 32,
            HasPractical = true,
            PracticalPassMarks = 8
        };

        var result = service.IsStudentPassing(20f, 4f, offering);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsStudentPassing_WhenTheoryNotRequired_ShouldNotCheckTheory()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = new SubjectOffering
        {
            HasTheory = false,
            HasPractical = true,
            PracticalPassMarks = 8
        };

        var result = service.IsStudentPassing(null, 10f, offering);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsStudentPassing_WhenPracticalNotRequired_ShouldNotCheckPractical()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = new SubjectOffering
        {
            HasTheory = true,
            TheoryPassMarks = 32,
            HasPractical = false
        };

        var result = service.IsStudentPassing(40f, null, offering);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsStudentPassing_WhenTheoryMarksNullButRequired_ShouldPass()
    {
        var service = new GradeCalculationService(CreateContext());
        var offering = new SubjectOffering
        {
            HasTheory = true,
            TheoryPassMarks = 32,
            HasPractical = false
        };

        var result = service.IsStudentPassing(null, null, offering);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CalculateGrade_WithoutGradingScheme_ShouldFallbackWhenNoSchemeInDb()
    {
        using var context = await CreateContextAsync();
        var offering = new SubjectOffering
        {
            ProgramId = 999,
            TheoryFullMarks = 80,
            PracticalFullMarks = 20
        };
        var service = new GradeCalculationService(context);

        var result = service.CalculateGrade(85, offering);

        result.GradeLetter.Should().Be("C");
        result.GradePoint.Should().Be(2.0m);
        result.IsPass.Should().BeTrue();
    }
}
