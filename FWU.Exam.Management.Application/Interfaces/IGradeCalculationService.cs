using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IGradeCalculationService
{
    GradeResult CalculateGrade(decimal totalMarks, SubjectOffering subjectOffering, Domain.Entities.GradingScheme? gradingScheme = null);
    bool IsStudentPassing(decimal? theoryMarks, decimal? practicalMarks, SubjectOffering offering);
    decimal CalculateTotalMarks(string? theory, string? practical, decimal? theoryInternal, decimal? practicalInternal);
}
