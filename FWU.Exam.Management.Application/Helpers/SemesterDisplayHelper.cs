using FWU.Exam.Management.Domain.Entities.Semesters;

namespace FWU.Exam.Management.Application.Helpers;

public static class SemesterDisplayHelper
{
    public static string Format(Semester? semester, string? academicYearName = null)
    {
        if (semester == null) return string.Empty;
        return string.IsNullOrWhiteSpace(academicYearName)
            ? $"{semester.Name} ({semester.Code})"
            : $"{semester.Name} ({semester.Code} - {academicYearName})";
    }

    public static string FormatForProgram(Semester? semester, string? programShortName, string? academicYearName = null)
    {
        if (semester == null) return string.Empty;
        var label = string.IsNullOrWhiteSpace(programShortName) ? semester.Code : programShortName;
        return string.IsNullOrWhiteSpace(academicYearName)
            ? $"{semester.Name} ({label})"
            : $"{semester.Name} ({label} - {academicYearName})";
    }
}
