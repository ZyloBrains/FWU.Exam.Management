using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class GradeCalculationService(AppDbContext context) : IGradeCalculationService
{
    public GradeResult CalculateGrade(decimal totalMarks, SubjectOffering subjectOffering, Domain.Entities.GradingScheme? gradingScheme = null)
    {
        if (gradingScheme == null)
        {
            gradingScheme = context.GradingSchemes
                .Include(gs => gs.GradeDefinitions)
                .FirstOrDefault(gs => gs.ProgramId == subjectOffering.ProgramId && gs.IsActive);
        }

        var theoryFull = (decimal)subjectOffering.TheoryFullMarks;
        var practicalFull = subjectOffering.PracticalFullMarks.HasValue ? (decimal)subjectOffering.PracticalFullMarks.Value : 0m;
        var internalTheoryFull = subjectOffering.InternalTheoryFullMarks.HasValue ? (decimal)subjectOffering.InternalTheoryFullMarks.Value : 0m;
        var internalPracticalFull = subjectOffering.InternalPracticalFullMarks.HasValue ? (decimal)subjectOffering.InternalPracticalFullMarks.Value : 0m;
        var totalFullMarks = theoryFull + practicalFull + internalTheoryFull + internalPracticalFull;

        if (totalFullMarks == 0) totalFullMarks = 1;

        var percentage = (totalMarks / totalFullMarks) * 100m;

        if (gradingScheme?.GradeDefinitions != null && gradingScheme.GradeDefinitions.Count != 0)
        {
            var matched = gradingScheme.GradeDefinitions
                .Where(gd => percentage >= gd.MinPercentage && percentage <= gd.MaxPercentage)
                .OrderBy(gd => gd.DisplayOrder)
                .FirstOrDefault();

            if (matched != null)
            {
                return new GradeResult
                {
                    GradeLetter = matched.GradeLetter,
                    GradePoint = matched.GradePoint,
                    IsPass = matched.IsPass,
                    Remark = matched.Remark
                };
            }
        }

        return new GradeResult
        {
            GradeLetter = percentage >= 40 ? "C" : "F",
            GradePoint = percentage >= 40 ? 2.0m : 0.0m,
            IsPass = percentage >= 40,
            Remark = percentage >= 40 ? "Pass" : "Fail"
        };
    }

    public bool IsStudentPassing(decimal? theoryMarks, decimal? practicalMarks, SubjectOffering offering)
    {
        if (offering.HasTheory && theoryMarks.HasValue && theoryMarks.Value < (decimal)offering.TheoryPassMarks)
            return false;

        if (offering.HasPractical && practicalMarks.HasValue && offering.PracticalPassMarks.HasValue
            && practicalMarks.Value < (decimal)offering.PracticalPassMarks.Value)
            return false;

        return true;
    }

    public decimal CalculateTotalMarks(string? theory, string? practical, decimal? theoryInternal, decimal? practicalInternal)
    {
        decimal total = 0;
        if (decimal.TryParse(theory, out var t)) total += t;
        if (decimal.TryParse(practical, out var p)) total += p;
        if (theoryInternal.HasValue) total += theoryInternal.Value;
        if (practicalInternal.HasValue) total += practicalInternal.Value;
        return total;
    }
}
