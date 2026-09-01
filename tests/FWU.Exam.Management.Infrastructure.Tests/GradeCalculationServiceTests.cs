using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class GradeCalculationServiceTests
{
    private static GradingScheme Scheme(int id, int programId, params (string Letter, decimal Min, decimal Max, decimal Point, bool IsPass)[] bands)
    {
        var scheme = new GradingScheme
        {
            Id = id,
            Name = $"Scheme {id}",
            IsActive = true
        };

        foreach (var (letter, min, max, point, isPass) in bands)
        {
            scheme.GradeDefinitions.Add(new GradeDefinition
            {
                Id = scheme.GradeDefinitions.Count + 1,
                GradeLetter = letter,
                MinPercentage = min,
                MaxPercentage = max,
                GradePoint = point,
                IsPass = isPass,
                DisplayOrder = scheme.GradeDefinitions.Count + 1,
                Remark = isPass ? "Pass" : "Fail",
                GradingSchemeId = id
            });
        }

        return scheme;
    }

    private static GradingSchemeProgram SchemeProgram(int schemeId, int programId)
    {
        return new GradingSchemeProgram
        {
            GradingSchemeId = schemeId,
            ProgramId = programId,
            IsActive = true
        };
    }

    private static SubjectOffering TheoryOffering(bool hasInternal = true, int theoryFull = 100, int? internalFull = null, int theoryPass = 40)
    {
        return new SubjectOffering
        {
            Id = 501,
            TenantId = TestData.TenantId,
            ProgramId = TestData.ProgramId,
            HasTheory = true,
            HasPractical = false,
            HasInternal = hasInternal,
            TheoryFullMarks = theoryFull,
            TheoryPassMarks = theoryPass,
            InternalTheoryFullMarks = internalFull
        };
    }

    private static SubjectOffering TheoryAndPracticalOffering()
    {
        return new SubjectOffering
        {
            Id = 502,
            TenantId = TestData.TenantId,
            ProgramId = TestData.ProgramId,
            HasTheory = true,
            HasPractical = true,
            HasInternal = true,
            TheoryFullMarks = 60,
            TheoryPassMarks = 24,
            InternalTheoryFullMarks = 40,
            PracticalFullMarks = 50,
            PracticalPassMarks = 20
        };
    }

    [Fact]
    public void CalculateGrade_ReturnsGradeFromScheme_WhenPercentageMatchesBand()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("B", 60, 79, 3.0m, true),
                ("C", 40, 59, 2.0m, true),
                ("F", 0, 39, 0.0m, false)));
            ctx.GradingSchemePrograms.Add(SchemeProgram(1, TestData.ProgramId));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryOffering();

        var result = service.CalculateGrade(85, offering);

        Assert.Equal("A", result.GradeLetter);
        Assert.Equal(4.0m, result.GradePoint);
        Assert.True(result.IsPass);
    }

    [Fact]
    public void CalculateGrade_FallsBackToDefault_WhenNoSchemeExists()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => TestData.SeedBase(ctx));

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryOffering();

        var pass = service.CalculateGrade(70, offering);
        Assert.Equal("C", pass.GradeLetter);
        Assert.True(pass.IsPass);

        var fail = service.CalculateGrade(20, offering);
        Assert.Equal("F", fail.GradeLetter);
        Assert.False(fail.IsPass);
    }

    [Fact]
    public void CalculateTheoryGrade_CombinesInternalMarks()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
            ctx.GradingSchemePrograms.Add(SchemeProgram(1, TestData.ProgramId));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryOffering(theoryFull: 60, internalFull: 40);

        var result = service.CalculateTheoryGrade(45, 35, offering);

        Assert.Equal("A", result.GradeLetter);
        Assert.Equal(4.0m, result.GradePoint);
    }

    [Fact]
    public void CalculateTheoryGrade_ReturnsNoPart_WhenSubjectHasNoTheory()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => TestData.SeedBase(ctx));

        var service = new GradeCalculationService(db.Context);
        var offering = new SubjectOffering
        {
            Id = 503,
            TenantId = TestData.TenantId,
            ProgramId = TestData.ProgramId,
            HasTheory = false,
            HasPractical = true,
            PracticalFullMarks = 50
        };

        var result = service.CalculateTheoryGrade(40, null, offering);

        Assert.Equal("", result.GradeLetter);
        Assert.True(result.IsPass);
    }

    [Fact]
    public void CalculatePracticalGrade_ReturnsCorrectGrade()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
            ctx.GradingSchemePrograms.Add(SchemeProgram(1, TestData.ProgramId));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = new SubjectOffering
        {
            Id = 504,
            TenantId = TestData.TenantId,
            ProgramId = TestData.ProgramId,
            HasTheory = false,
            HasPractical = true,
            PracticalFullMarks = 50
        };

        var result = service.CalculatePracticalGrade(40, offering);

        Assert.Equal("A", result.GradeLetter);
    }

    [Fact]
    public void AssignGrades_SetsPerPartAndOverallGrades()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
            ctx.GradingSchemePrograms.Add(SchemeProgram(1, TestData.ProgramId));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryAndPracticalOffering();
        var result = new ExamSubjectResult
        {
            TenantId = TestData.TenantId,
            ObtainedMarksTheory = 50,
            ObtainedMarksTheoryInternal = 35,
            ObtainedMarksPractical = 45
        };

        service.AssignGrades(result, offering);

        Assert.Equal("A", result.GradeLetterTheory);
        Assert.Equal("A", result.GradeLetterPractical);
        Assert.Equal("A", result.GradeLetter);
        Assert.Equal(130f, result.ObtainedMarks);
        Assert.Equal("Pass", result.Remarks);
    }

    [Fact]
    public void AssignGrades_LeavesOverallGradeEmpty_WhenNoMarksEntered()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
            ctx.GradingSchemePrograms.Add(SchemeProgram(1, TestData.ProgramId));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryAndPracticalOffering();
        var result = new ExamSubjectResult { TenantId = TestData.TenantId };

        service.AssignGrades(result, offering);

        Assert.Null(result.GradeLetterTheory);
        Assert.Null(result.GradeLetterPractical);
        Assert.Null(result.GradeLetter);
        Assert.Null(result.ObtainedMarks);
    }

    [Fact]
    public void AssignGrades_ClearsPreviousGrades_WhenMarksRemoved()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
            ctx.GradingSchemePrograms.Add(SchemeProgram(1, TestData.ProgramId));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryAndPracticalOffering();
        var result = new ExamSubjectResult
        {
            TenantId = TestData.TenantId,
            ObtainedMarksTheory = 50,
            ObtainedMarksTheoryInternal = 35,
            ObtainedMarksPractical = 45,
            GradeLetter = "A",
            GradeLetterTheory = "A",
            GradeLetterPractical = "A",
            ObtainedMarks = 130,
            Remarks = "Pass"
        };

        result.ObtainedMarksTheory = null;
        result.ObtainedMarksTheoryInternal = null;
        result.ObtainedMarksPractical = null;

        service.AssignGrades(result, offering);

        Assert.Null(result.GradeLetter);
        Assert.Null(result.GradeLetterTheory);
        Assert.Null(result.GradeLetterPractical);
        Assert.Null(result.ObtainedMarks);
        Assert.Null(result.Remarks);
    }

    [Fact]
    public void AssignGrades_ComputesTheoryOnlyGrade_ForTheoryOnlySubject()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
            ctx.GradingSchemePrograms.Add(SchemeProgram(1, TestData.ProgramId));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryOffering(theoryFull: 100, internalFull: 0);
        var result = new ExamSubjectResult
        {
            TenantId = TestData.TenantId,
            ObtainedMarksTheory = 82
        };

        service.AssignGrades(result, offering);

        Assert.Equal("A", result.GradeLetterTheory);
        Assert.Null(result.GradeLetterPractical);
        Assert.Equal("A", result.GradeLetter);
        Assert.Equal(82f, result.ObtainedMarks);
    }

    [Fact]
    public void CalculateTotalMarks_SumsOnlyPresentParts()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => TestData.SeedBase(ctx));
        var service = new GradeCalculationService(db.Context);

        var total = service.CalculateTotalMarks(50, 45, 35, null);

        Assert.Equal(130f, total);
    }

    [Fact]
    public void GetGradePointValue_ReturnsCachedValueFromGradeDefinitions()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            var scheme = new GradingScheme
            {
                Id = 10,
                Name = "Test Scheme",
                IsActive = true
            };
            scheme.GradeDefinitions.Add(new GradeDefinition
            {
                Id = 1,
                GradeLetter = "A+",
                MinPercentage = 90,
                MaxPercentage = 100,
                GradePoint = 4.0m,
                IsPass = true,
                DisplayOrder = 1,
                GradingSchemeId = 10
            });
            ctx.GradingSchemes.Add(scheme);
            ctx.GradingSchemePrograms.Add(new GradingSchemeProgram
            {
                GradingSchemeId = 10,
                ProgramId = TestData.ProgramId,
                IsActive = true
            });
        });

        var service = new GradeCalculationService(db.Context);
        var schemeFromDb = db.Context.GradingSchemes.First(s => s.Id == 10);

        var value = service.GetGradePointValue("A+", schemeFromDb);

        Assert.Equal(4.0m, value);
    }

    [Fact]
    public void IsStudentPassing_ReturnsFalse_WhenTheoryBelowPassMarks()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => TestData.SeedBase(ctx));
        var service = new GradeCalculationService(db.Context);
        var offering = TheoryAndPracticalOffering();

        Assert.False(service.IsStudentPassing(20, null, offering));
        Assert.True(service.IsStudentPassing(30, 25, offering));
        Assert.False(service.IsStudentPassing(30, 15, offering));
    }

    [Fact]
    public void IsStudentPassing_EnforcesPracticalPassMarks_EvenWhenSupplementary()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx => TestData.SeedBase(ctx));
        var service = new GradeCalculationService(db.Context);
        var offering = TheoryAndPracticalOffering();

        // Practical re-sat below the pass mark fails even on supplementary rows.
        Assert.False(service.IsStudentPassing(30, 15, offering, isSupplementary: true));
        // Theory unregistered (null) is skipped; a passing practical suffices.
        Assert.True(service.IsStudentPassing(null, 30, offering, isSupplementary: true));
        Assert.True(service.IsStudentPassing(30, 25, offering, isSupplementary: true));
        Assert.False(service.IsStudentPassing(20, null, offering, isSupplementary: true));
    }

    [Fact]
    public void AssignGrades_SupplementaryFailsAgain_WhenReSatPracticalBelowPass()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryAndPracticalOffering();
        var result = new ExamSubjectResult
        {
            TenantId = TestData.TenantId,
            ObtainedMarksTheory = 30,
            ObtainedMarksTheoryInternal = 30,
            ObtainedMarksPractical = 15,
            ObtainedMarksPracticalInternal = null,
            IsSupplementary = true
        };

        service.AssignGrades(result, offering, isSupplementary: true);

        Assert.Equal("Fail", result.Remarks);
    }

    [Fact]
    public void AssignGrades_SupplementaryPracticalOnlyResit_PassesWhenAbovePass()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryAndPracticalOffering();
        // Practical-only re-sit: the passed theory external is absent from the
        // row; only its internals carried forward.
        var result = new ExamSubjectResult
        {
            TenantId = TestData.TenantId,
            ObtainedMarksTheory = null,
            ObtainedMarksTheoryInternal = 35,
            ObtainedMarksPractical = 45,
            IsSupplementary = true
        };

        service.AssignGrades(result, offering, isSupplementary: true);

        Assert.Null(result.GradeLetterTheory);
        Assert.NotNull(result.GradeLetterPractical);
        Assert.Equal("Pass", result.Remarks);
    }

    [Fact]
    public void AssignGrades_UsesTheoryOnlyPass_FailsWhenTheoryBelowPass_ForSupplementary()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryAndPracticalOffering();
        var result = new ExamSubjectResult
        {
            TenantId = TestData.TenantId,
            ObtainedMarksTheory = 20,
            ObtainedMarksTheoryInternal = 30,
            ObtainedMarksPractical = 45,
            ObtainedMarksPracticalInternal = null,
            IsSupplementary = true
        };

        service.AssignGrades(result, offering, isSupplementary: true);

        Assert.Equal("Fail", result.Remarks);
    }

    [Fact]
    public void AssignGrades_UsesCombinedPass_ForRegularStudent()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
        {
            TestData.SeedBase(ctx);
            ctx.GradingSchemes.Add(Scheme(1, TestData.ProgramId,
                ("A", 80, 100, 4.0m, true),
                ("F", 0, 39, 0.0m, false)));
        });

        var service = new GradeCalculationService(db.Context);
        var offering = TheoryAndPracticalOffering();
        var result = new ExamSubjectResult
        {
            TenantId = TestData.TenantId,
            ObtainedMarksTheory = 15,
            ObtainedMarksTheoryInternal = 10,
            ObtainedMarksPractical = 10
        };

        service.AssignGrades(result, offering);

        Assert.Equal("Fail", result.Remarks);
    }
}
