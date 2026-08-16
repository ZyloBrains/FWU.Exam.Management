using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IGradeCalculationService
{
    GradeResult CalculateGrade(float totalMarks, SubjectOffering subjectOffering, Domain.Entities.GradingScheme? gradingScheme = null);
    decimal? GetGradePointValue(string gradeLetter, int? gradeGroupId);
    bool IsStudentPassing(float? theoryMarks, float? practicalMarks, SubjectOffering offering);
    float CalculateTotalMarks(float? theory, float? practical, float? theoryInternal, float? practicalInternal);
}
