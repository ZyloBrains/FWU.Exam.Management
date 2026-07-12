using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeder;

public static class LevelCsvSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        // Clear dependent tables that reference Level (in FK-safe order)
        var dependentTables = new[]
        {
            "StudentRegistrations",
            "ExamSchedules",
            "Programs",
            "PreviousLevels",
        };

        foreach (var table in dependentTables)
        {
            try { await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{table}]"); } catch { }
        }

        await context.Levels.ExecuteDeleteAsync();
        await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Levels', RESEED, 0)");

        var csvPath = Path.Combine(AppContext.BaseDirectory, "Data", "Seeders", "levels.csv");
        if (!File.Exists(csvPath))
            throw new FileNotFoundException($"Level seed file not found: {csvPath}");

        var lines = await File.ReadAllLinesAsync(csvPath);
        if (lines.Length < 2)
            throw new Exception("Level CSV file is empty or has no data rows.");

        var headers = ParseCsvLine(lines[0]).Select(h => h.Trim()).ToArray();
        int idxCode       = Array.FindIndex(headers, h => h.Equals("LevelCode", StringComparison.OrdinalIgnoreCase));
        int idxName       = Array.FindIndex(headers, h => h.Equals("LevelName", StringComparison.OrdinalIgnoreCase));
        int idxNameNepali = Array.FindIndex(headers, h => h.Equals("LevelNameNepali", StringComparison.OrdinalIgnoreCase));
        int idxDisplay    = Array.FindIndex(headers, h => h.Equals("LevelDisplayOrder", StringComparison.OrdinalIgnoreCase));
        int idxRemark     = Array.FindIndex(headers, h => h.Equals("Remarks", StringComparison.OrdinalIgnoreCase));
        int idxActive     = Array.FindIndex(headers, h => h.Equals("IsActive", StringComparison.OrdinalIgnoreCase));

        if (idxCode < 0 || idxName < 0)
            throw new Exception("Level CSV must contain 'LevelCode' and 'LevelName' columns.");

        var levels = new List<Level>();

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var parts = ParseCsvLine(lines[i]);
            if (parts.Length < headers.Length) continue;

            var code       = SafeGet(parts, idxCode)?.Trim() ?? "";
            var name       = SafeGet(parts, idxName)?.Trim() ?? "";
            var nameNe     = SafeGet(parts, idxNameNepali)?.Trim();
            var displayStr = SafeGet(parts, idxDisplay)?.Trim();
            var remark     = SafeGet(parts, idxRemark)?.Trim();
            var actStr     = SafeGet(parts, idxActive)?.Trim() ?? "1";

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name)) continue;

            int? displayOrder = int.TryParse(displayStr, out var d) ? d : null;

            levels.Add(new Level
            {
                LevelCode = code,
                LevelName = name,
                LevelNameNepali = string.IsNullOrEmpty(nameNe) ? null : nameNe,
                LevelDisplayOrder = displayOrder,
                Remarks = string.IsNullOrEmpty(remark) ? null : remark,
                IsActive = actStr == "1" || actStr.Equals("true", StringComparison.OrdinalIgnoreCase),
            });
        }

        if (levels.Count > 0)
        {
            await context.Levels.AddRangeAsync(levels);
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
