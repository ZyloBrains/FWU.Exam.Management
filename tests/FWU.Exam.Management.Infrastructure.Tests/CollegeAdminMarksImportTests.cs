using ClosedXML.Excel;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class CollegeAdminMarksImportTests
{
    private static CollegeAdminMarksService CreateService(TestDb db) =>
        new(
            db.Context,
            MarksEntryTestSeed.CollegeAdmin(),
            new CollegeAdminSubjectAssignmentService(db.Context, MarksEntryTestSeed.CollegeAdmin()),
            new GradeCalculationService(db.Context),
            new TestAuditLogWriter());

    private static MemoryStream BuildImportExcel(string symbolNo, string theory)
    {
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Marks");
        ws.Cell(1, 1).Value = "Symbol No.";
        ws.Cell(1, 2).Value = "Theory";
        ws.Cell(2, 1).Value = symbolNo;
        ws.Cell(2, 2).Value = theory;

        var ms = new MemoryStream();
        workbook.SaveAs(ms);
        ms.Position = 0;
        return ms;
    }

    private static void SeedMultiSubjectReg(AppDbContext ctx)
    {
        TestData.SeedBase(ctx);

        ctx.CollegePrograms.Add(new FWU.Exam.Management.Domain.Entities.Colleges.CollegeProgram
        {
            TenantId = TestData.TenantId,
            CollegeId = TestData.CollegeId,
            ProgramId = TestData.ProgramId,
            IsActive = true
        });

        ctx.ExamSchedules.Add(TestData.Schedule(21, 1, TestData.Regular,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), DateTime.UtcNow.AddDays(45)));

        var reg = new FWU.Exam.Management.Domain.Entities.Exams.ExamRegistration
        {
            Id = 1,
            TenantId = TestData.TenantId,
            AcademicYearId = TestData.AcademicYearId,
            CollegeId = TestData.CollegeId,
            ProgramsId = TestData.ProgramId,
            ExamScheduleId = 21,
            RegistrationDate = DateTime.UtcNow,
            Status = FWU.Exam.Management.Domain.Enums.RegistrationStatus.Registered,
            IsActive = true,
            IsAppliedByStudent = true,
            ExamRollNumber = "R101",
            SymbolNumber = "SYM101"
        };
        ctx.ExamRegistrations.Add(reg);

        // Seed two result rows for the same student in the same schedule but
        // different subject offerings.  The unrelated subject's row is inserted
        // first so that a naive registration-keyed lookup returns it as the
        // match — the exact bug the fix guards against.
        ctx.ExamSubjectResults.Add(new ExamSubjectResult
        {
            Id = 1,
            TenantId = TestData.TenantId,
            ExamRegistrationId = 1,
            SubjectOfferingId = 102,
            ExamScheduleId = 21,
            ExamTypeId = TestData.Regular,
            IsActive = true,
            IsSubmitted = false
        });

        ctx.ExamSubjectResults.Add(new ExamSubjectResult
        {
            Id = 2,
            TenantId = TestData.TenantId,
            ExamRegistrationId = 1,
            SubjectOfferingId = 101,
            ExamScheduleId = 21,
            ExamTypeId = TestData.Regular,
            IsActive = true,
            IsSubmitted = false
        });
    }

    [Fact]
    public async Task ImportMarks_NonReExam_UpdatesOnlyTargetOfferingRow()
    {
        using var db = new TestDb(TestTenantContext.Standard(), SeedMultiSubjectReg);
        using var excel = BuildImportExcel("R101", "30");

        var result = await CreateService(db)
            .ImportMarksFromExcelAsync(excel, 101, 21, MarksEntryTestSeed.AdminUserId);

        Assert.True(result.Success);
        Assert.Equal(1, result.ImportedCount);

        var rows = await db.Context.ExamSubjectResults
            .Where(esr => esr.ExamRegistrationId == 1 && esr.ExamScheduleId == 21)
            .ToListAsync();

        Assert.Equal(2, rows.Count);
        Assert.Equal(30f, rows.Single(r => r.SubjectOfferingId == 101).ObtainedMarksTheory);
        Assert.Null(rows.Single(r => r.SubjectOfferingId == 102).ObtainedMarksTheory);
    }
}
