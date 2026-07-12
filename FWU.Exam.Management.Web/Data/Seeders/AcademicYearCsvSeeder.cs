using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeder;

public static class AcademicYearCsvSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        // Clear dependent tables that reference AcademicYear (in FK-safe order)
        var dependentTables = new[]
        {
            "ExamSubjectResults",
            "ExamRegistrations",
            "ExamSlots",
            "ExamCenters",
            "ExamRollNumberSetup",
            "ExamSchedules",
            "ExamFees",
            "GradingSchemes",
            "GradeDefinitions",
            "CurriculumVersions",
            "SubjectOfferings",
            "Batches",
            "Semesters",
            "SemesterEnrollments",
            "StudentAdmissions",
            "StudentRegistrations",
            "EntranceExamApplications",
        };

        foreach (var table in dependentTables)
        {
            try { await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]"); } catch { }
        }

        await context.AcademicYears.ExecuteDeleteAsync();

        var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "Seeders", "academic-years.csv");
        if (!File.Exists(csvPath))
            throw new FileNotFoundException($"Academic year seed file not found: {csvPath}");

        var lines = await File.ReadAllLinesAsync(csvPath);
        if (lines.Length < 2)
            throw new Exception("Academic year CSV file is empty or has no data rows.");

        // Parse header
        var headers = ParseCsvLine(lines[0]).Select(h => h.Trim()).ToArray();
        int idxCode       = Array.FindIndex(headers, h => h.Equals("AcademicYearCode", StringComparison.OrdinalIgnoreCase));
        int idxName       = Array.FindIndex(headers, h => h.Equals("AcademicYearName", StringComparison.OrdinalIgnoreCase));
        int idxCodeNepali = Array.FindIndex(headers, h => h.Equals("AcademicYearCodeNepali", StringComparison.OrdinalIgnoreCase));
        int idxNameNepali = Array.FindIndex(headers, h => h.Equals("AcademicYearNameNepali", StringComparison.OrdinalIgnoreCase));
        int idxRunning    = Array.FindIndex(headers, h => h.Equals("IsRunning", StringComparison.OrdinalIgnoreCase));
        int idxActive     = Array.FindIndex(headers, h => h.Equals("IsActive", StringComparison.OrdinalIgnoreCase));
        int idxRemark     = Array.FindIndex(headers, h => h.Equals("Remarks", StringComparison.OrdinalIgnoreCase));

        if (idxCode < 0 || idxName < 0)
            throw new Exception("Academic year CSV must contain 'AcademicYearCode' and 'AcademicYearName' columns.");

        var academicYears = new List<AcademicYear>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var parts = ParseCsvLine(lines[i]);
            if (parts.Length < headers.Length) continue;

            var code   = SafeGet(parts, idxCode)?.Trim() ?? "";
            var name   = SafeGet(parts, idxName)?.Trim() ?? "";
            var codeNe = SafeGet(parts, idxCodeNepali)?.Trim();
            var nameNe = SafeGet(parts, idxNameNepali)?.Trim();
            var runStr = SafeGet(parts, idxRunning)?.Trim() ?? "0";
            var actStr = SafeGet(parts, idxActive)?.Trim() ?? "1";
            var remark = SafeGet(parts, idxRemark)?.Trim();

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name)) continue;

            academicYears.Add(new AcademicYear
            {
                AcademicYearCode = code,
                AcademicYearCodeNepali = string.IsNullOrEmpty(codeNe) ? null : codeNe,
                AcademicYearName = name,
                AcademicYearNameNepali = string.IsNullOrEmpty(nameNe) ? null : nameNe,
                Remark = string.IsNullOrEmpty(remark) ? null : remark,
                IsRunning = runStr == "1" || runStr.Equals("true", StringComparison.OrdinalIgnoreCase),
                IsActive  = actStr == "1" || actStr.Equals("true", StringComparison.OrdinalIgnoreCase),
            });
        }

        if (academicYears.Count > 0)
        {
            await context.AcademicYears.AddRangeAsync(academicYears);
            await context.SaveChangesAsync();
        }
    }

    private static string? SafeGet(string[] parts, int index)
    {
        return index >= 0 && index < parts.Length ? parts[index] : null;
    }

    private static string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    inQuotes = false;
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
