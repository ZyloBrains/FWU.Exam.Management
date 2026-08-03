using FWU.Exam.Management.Domain.Entities.Semesters;

namespace FWU.Exam.Management.Application.Helpers;

public static class SemesterDisplayHelper
{
    public static string Format(Semester? semester, string? academicYearName = null)
    {
        if (semester == null) return string.Empty;
        var yearName = academicYearName ?? semester.AcademicYear?.AcademicYearName;
        return string.IsNullOrWhiteSpace(yearName)
            ? $"{semester.Name} ({semester.Code})"
            : $"{semester.Name} ({semester.Code} - {yearName})";
    }

    public static string FormatForProgram(Semester? semester, string? programShortName, string? academicYearName = null)
    {
        if (semester == null) return string.Empty;
        var yearName = academicYearName ?? semester.AcademicYear?.AcademicYearName;
        var label = string.IsNullOrWhiteSpace(programShortName) ? semester.Code : programShortName;
        return string.IsNullOrWhiteSpace(yearName)
            ? $"{semester.Name} ({label})"
            : $"{semester.Name} ({label} - {yearName})";
    }
}
