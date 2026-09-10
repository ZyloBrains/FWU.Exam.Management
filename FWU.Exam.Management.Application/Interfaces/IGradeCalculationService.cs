using FWU.Exam.Management.Application.DTOs;
using FWU.Exam.Management.Domain.Entities.Exams;
using FWU.Exam.Management.Domain.Entities.Subjects;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IGradeCalculationService
{
    Domain.Entities.GradingScheme? ResolveSchemeForProgram(int programId, int? academicYearId = null);
    GradeResult CalculateGrade(float totalMarks, SubjectOffering subjectOffering, Domain.Entities.GradingScheme? gradingScheme = null);
    GradeResult CalculateTheoryGrade(float? theoryMarks, float? theoryInternalMarks, SubjectOffering offering, Domain.Entities.GradingScheme? gradingScheme = null);
    GradeResult CalculatePracticalGrade(float? practicalMarks, SubjectOffering offering, Domain.Entities.GradingScheme? gradingScheme = null);
    decimal? GetGradePointValue(string gradeLetter, Domain.Entities.GradingScheme gradingScheme);
    bool IsStudentPassing(float? theoryMarks, float? practicalMarks, SubjectOffering offering, bool isSupplementary = false);
    float CalculateTotalMarks(float? theory, float? practical, float? theoryInternal, float? practicalInternal);
    void AssignGrades(ExamSubjectResult result, SubjectOffering offering, bool isSupplementary = false);
}
