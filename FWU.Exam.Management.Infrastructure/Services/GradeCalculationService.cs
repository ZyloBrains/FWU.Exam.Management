using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Subjects;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class GradeCalculationService(AppDbContext context) : IGradeCalculationService
{
    public GradeResult CalculateGrade(float totalMarks, SubjectOffering subjectOffering, Domain.Entities.GradingScheme? gradingScheme = null)
    {
        if (gradingScheme == null)
        {
            gradingScheme = context.GradingSchemes
                .Include(gs => gs.GradeDefinitions)
                .FirstOrDefault(gs => gs.ProgramId == subjectOffering.ProgramId && gs.IsActive);
        }

        var theoryFull = subjectOffering.TheoryFullMarks;
        var practicalFull = subjectOffering.PracticalFullMarks ?? 0f;
        var internalTheoryFull = subjectOffering.InternalTheoryFullMarks ?? 0f;
        var totalFullMarks = theoryFull + practicalFull + internalTheoryFull;

        if (totalFullMarks == 0) totalFullMarks = 1;

        var percentage = (totalMarks / totalFullMarks) * 100f;

        if (gradingScheme?.GradeDefinitions != null && gradingScheme.GradeDefinitions.Count != 0)
        {
            var percentageDecimal = (decimal)percentage;
            var matched = gradingScheme.GradeDefinitions
                .Where(gd => percentageDecimal >= gd.MinPercentage && percentageDecimal <= gd.MaxPercentage)
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

    public bool IsStudentPassing(float? theoryMarks, float? practicalMarks, SubjectOffering offering)
    {
        if (offering.HasTheory && theoryMarks.HasValue && theoryMarks.Value < offering.TheoryPassMarks)
            return false;

        if (offering.HasPractical && practicalMarks.HasValue && offering.PracticalPassMarks.HasValue
            && practicalMarks.Value < offering.PracticalPassMarks.Value)
            return false;

        return true;
    }

    public float CalculateTotalMarks(float? theory, float? practical, float? theoryInternal, float? practicalInternal)
    {
        float total = 0;
        if (theory.HasValue) total += theory.Value;
        if (practical.HasValue) total += practical.Value;
        if (theoryInternal.HasValue) total += theoryInternal.Value;
        if (practicalInternal.HasValue) total += practicalInternal.Value;
        return total;
    }
}
