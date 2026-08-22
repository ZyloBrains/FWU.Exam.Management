using System.Collections.Concurrent;
using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public class GradeCalculationService(AppDbContext context) : IGradeCalculationService
{
    private static readonly ConcurrentDictionary<(int GroupId, string Grade), decimal?> GradePointCache = new();

    public GradeResult CalculateGrade(float totalMarks, SubjectOffering subjectOffering, Domain.Entities.GradingScheme? gradingScheme = null)
    {
        if (gradingScheme == null)
            gradingScheme = GetSchemeForOffering(subjectOffering);

        var theoryFull = subjectOffering.TheoryFullMarks ?? 0f;
        var practicalFull = subjectOffering.PracticalFullMarks ?? 0f;
        var internalTheoryFull = subjectOffering.InternalTheoryFullMarks ?? 0f;
        var totalFullMarks = theoryFull + practicalFull + internalTheoryFull;

        if (totalFullMarks == 0) totalFullMarks = 1;

        var percentage = (totalMarks / totalFullMarks) * 100f;

        return ResolvePercentage(percentage, gradingScheme);
    }

    public GradeResult CalculateTheoryGrade(float? theoryMarks, float? theoryInternalMarks, SubjectOffering offering, Domain.Entities.GradingScheme? gradingScheme = null)
    {
        if (!offering.HasTheory || !theoryMarks.HasValue)
            return NoPart();

        var full = (offering.TheoryFullMarks ?? 0f) + (offering.InternalTheoryFullMarks ?? 0f);
        if (full <= 0) return NoPart();

        var obtained = theoryMarks.Value + (offering.HasInternal ? (theoryInternalMarks ?? 0f) : 0f);
        var percentage = (obtained / full) * 100f;

        if (gradingScheme == null)
            gradingScheme = GetSchemeForOffering(offering);

        return ResolvePercentage(percentage, gradingScheme);
    }

    public GradeResult CalculatePracticalGrade(float? practicalMarks, SubjectOffering offering, Domain.Entities.GradingScheme? gradingScheme = null)
    {
        if (!offering.HasPractical || !practicalMarks.HasValue)
            return NoPart();

        var full = offering.PracticalFullMarks ?? 0f;
        if (full <= 0) return NoPart();

        var percentage = (practicalMarks.Value / full) * 100f;

        if (gradingScheme == null)
            gradingScheme = GetSchemeForOffering(offering);

        return ResolvePercentage(percentage, gradingScheme);
    }

    public void AssignGrades(ExamSubjectResult result, SubjectOffering offering, bool isSupplementary = false)
    {
        result.GradeLetterTheory = null;
        result.GradeLetterPractical = null;
        result.GradeLetter = null;
        result.ObtainedMarks = null;
        result.Remarks = null;

        if (offering.HasTheory && result.ObtainedMarksTheory.HasValue)
        {
            var theoryGrade = CalculateTheoryGrade(result.ObtainedMarksTheory, result.ObtainedMarksTheoryInternal, offering);
            result.GradeLetterTheory = theoryGrade.GradeLetter;
        }

        if (offering.HasPractical && result.ObtainedMarksPractical.HasValue)
        {
            var practicalGrade = CalculatePracticalGrade(result.ObtainedMarksPractical, offering);
            result.GradeLetterPractical = practicalGrade.GradeLetter;
        }

        if (result.ObtainedMarksTheory.HasValue
            || result.ObtainedMarksPractical.HasValue
            || result.ObtainedMarksTheoryInternal.HasValue
            || result.ObtainedMarksPracticalInternal.HasValue)
        {
            var totalMarks = CalculateTotalMarks(
                result.ObtainedMarksTheory,
                result.ObtainedMarksPractical,
                result.ObtainedMarksTheoryInternal,
                result.ObtainedMarksPracticalInternal);

            result.ObtainedMarks = totalMarks;

            var overall = CalculateGrade(totalMarks, offering);
            result.GradeLetter = overall.GradeLetter;

            if (isSupplementary)
            {
                var passing = IsStudentPassing(result.ObtainedMarksTheory, result.ObtainedMarksPractical, offering, true);
                result.Remarks = passing ? "Pass" : "Fail";
            }
            else
            {
                result.Remarks = overall.Remark;
            }
        }
    }

    private Domain.Entities.GradingScheme? GetSchemeForOffering(SubjectOffering subjectOffering)
    {
        return context.GradingSchemes
            .Include(gs => gs.GradeDefinitions)
            .Where(gs => gs.ProgramId == subjectOffering.ProgramId && gs.IsActive)
            .OrderByDescending(gs => gs.GradeGroupId.HasValue)
            .ThenBy(gs => gs.Id)
            .FirstOrDefault();
    }

    private GradeResult ResolvePercentage(float percentage, Domain.Entities.GradingScheme? gradingScheme)
    {
        if (gradingScheme?.GradeGroupId.HasValue == true)
        {
            var obtainedMark = Math.Clamp((int)Math.Round(percentage), 0, 100);
            var gradePoint = context.GradePoints
                .FirstOrDefault(gp => gp.GradeGroupId == gradingScheme.GradeGroupId.Value && gp.ObtainedMark == obtainedMark);

            if (gradePoint != null)
            {
                return new GradeResult
                {
                    GradeLetter = gradePoint.Grade,
                    GradePoint = gradePoint.GradePointValue,
                    IsPass = gradePoint.GradePointValue > 0,
                    Remark = gradePoint.GradePointValue > 0 ? "Pass" : "Fail"
                };
            }
        }

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

    private static GradeResult NoPart()
    {
        return new GradeResult { GradeLetter = "", GradePoint = 0m, IsPass = true, Remark = "N/A" };
    }

    public decimal? GetGradePointValue(string gradeLetter, int? gradeGroupId)
    {
        if (string.IsNullOrWhiteSpace(gradeLetter) || !gradeGroupId.HasValue)
            return null;

        var key = (gradeGroupId.Value, gradeLetter);
        if (GradePointCache.TryGetValue(key, out var cached))
            return cached;

        var value = context.GradePoints
            .AsNoTracking()
            .Where(gp => gp.GradeGroupId == gradeGroupId.Value && gp.Grade == gradeLetter)
            .Select(gp => (decimal?)gp.GradePointValue)
            .FirstOrDefault();

        GradePointCache[key] = value;
        return value;
    }

    public bool IsStudentPassing(float? theoryMarks, float? practicalMarks, SubjectOffering offering, bool isSupplementary = false)
    {
        if (offering.HasTheory && theoryMarks.HasValue && offering.TheoryPassMarks.HasValue
            && theoryMarks.Value < offering.TheoryPassMarks.Value)
            return false;

        if (!isSupplementary)
        {
            if (offering.HasPractical && practicalMarks.HasValue && offering.PracticalPassMarks.HasValue
                && practicalMarks.Value < offering.PracticalPassMarks.Value)
                return false;
        }

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
