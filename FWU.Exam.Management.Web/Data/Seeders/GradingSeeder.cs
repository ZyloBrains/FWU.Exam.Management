using System.Text.RegularExpressions;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class GradingSeeder
{
    public static async Task SeedGradingDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        var gradeGroups = await context.GradeGroups.ToListAsync();
        var academicYears = await context.AcademicYears.ToListAsync();
        var gradePoints = await context.GradePoints.ToListAsync();
        var programs = await context.Programs.ToListAsync();

        // Grade groups are seeded from GradeGroup.csv (FWU schemes: old / new / latest).
        // The batch year is taken from the group's "From YYYY" remark (e.g. "From 2025").
        var ugNew = FindGroup(gradeGroups, "UG New", remarksContaining: "2025");
        var ugNewBase = FindGroup(gradeGroups, "UG New", remarksContaining: "UG all except BA LLB");
        var ugOld = FindGroup(gradeGroups, "UG Old");
        var ugBaLLbNew = FindGroup(gradeGroups, "UG BA.LLB New");
        var ugBeOld = FindGroup(gradeGroups, "UG BE Old");
        var ugAgOld = FindGroup(gradeGroups, "UG AG Old");
        var graduateNew2081 = FindGroup(gradeGroups, "Graduate New 2081");
        var graduateNew = FindGroup(gradeGroups, "Graduate New");
        var graduateOld = FindGroup(gradeGroups, "Graduate Old");
        var bscMlt = FindGroup(gradeGroups, "BSC MLT");
        var bph = FindGroup(gradeGroups, "BPH");
        var mphil = FindGroup(gradeGroups, "MPhil");

        var standardUGCodes = new[] { "L008", "L011", "L005", "L113", "L013", "L094", "L003", "L110", "L015", "L154", "L018", "L016" };
        var graduateCodes = new[] { "L126", "L097", "L009", "L096", "L134", "L093", "L012", "L124", "L119", "L131", "L108", "L153", "L098", "L095", "L099", "L105" };
        var mphilCodes = new[] { "L143", "L144", "L149", "L150", "L151", "L152" };

        var mappings = new (string[] ProgramCodes, GradeGroup? GradeGroup, bool IsActive)[]
        {
            // Active (latest) schemes
            // Undergraduate - UG New (From 2025, latest UG scheme)
            (standardUGCodes, ugNew, true),
            // Undergraduate yearly - UG Old
            (["L004"], ugOld, true),
            // BA LLB - UG BA.LLB New
            (["L115"], ugBaLLbNew, true),
            // Agriculture - UG AG Old
            (["L104"], ugAgOld, true),
            // Engineering - UG BE Old
            (["L092", "L117", "L118"], ugBeOld, true),
            // B.Sc. MLT (From 2024)
            (["L010"], bscMlt, true),
            // BPH (From 2025)
            (["L014"], bph, true),
            // Graduate - Graduate New 2081 (From 2025, latest graduate scheme)
            (graduateCodes, graduateNew2081, true),
            // MPhil
            (mphilCodes, mphil, true),

            // Inactive (older versions kept so older batches can still be assigned them)
            // Undergraduate - UG New (base, used before 2025)
            (standardUGCodes, ugNewBase, false),
            // Graduate - Graduate New (base)
            (graduateCodes, graduateNew, false),
            // Graduate - Graduate Old
            (graduateCodes, graduateOld, false),
        };

        foreach (var (programCodes, gradeGroup, isActive) in mappings)
        {
            if (gradeGroup == null)
                continue;

            var fromYear = ExtractFromYear(gradeGroup.Remarks);
            var academicYear = fromYear != null
                ? academicYears.FirstOrDefault(ay => ay.AcademicYearName == fromYear)
                : null;

            var matchingPrograms = programs.Where(p => programCodes.Contains(p.ProgramCode ?? string.Empty)).ToList();

            await SeedSchemeAsync(
                context,
                matchingPrograms,
                gradeGroup,
                gradePoints,
                academicYear,
                BuildSchemeName(gradeGroup, fromYear),
                isActive);
        }
    }

    private static async Task SeedSchemeAsync(
        AppDbContext context,
        List<Program> programs,
        GradeGroup gradeGroup,
        List<GradePoint> gradePoints,
        AcademicYear? academicYear,
        string schemeName,
        bool isActive)
    {
        // Find existing scheme with the same name (across any program)
        var existingSchemes = await context.GradingSchemes
            .Include(s => s.GradeDefinitions)
            .Include(s => s.ProgramAssignments)
            .Where(s => s.Name == schemeName)
            .ToListAsync();

        var existing = existingSchemes.FirstOrDefault();

        var definitions = BuildDefinitions(gradeGroup.Id, gradePoints);

        if (existing != null)
        {
            existing.Description = BuildDescription(gradeGroup);
            existing.IsActive = isActive;

            context.GradeDefinitions.RemoveRange(existing.GradeDefinitions);
            foreach (var gd in definitions)
                gd.GradingSchemeId = existing.Id;
            await context.GradeDefinitions.AddRangeAsync(definitions);

            // Update junction table assignments
            foreach (var program in programs)
            {
                var alreadyAssigned = existing.ProgramAssignments.Any(ga => ga.ProgramId == program.Id);
                if (!alreadyAssigned)
                {
                    context.GradingSchemePrograms.Add(new GradingSchemeProgram
                    {
                        GradingSchemeId = existing.Id,
                        ProgramId = program.Id,
                        AcademicYearId = academicYear?.Id,
                        IsActive = isActive
                    });
                }
                else
                {
                    // Update AcademicYearId if needed
                    var assignment = existing.ProgramAssignments.First(ga => ga.ProgramId == program.Id);
                    if (assignment.AcademicYearId != academicYear?.Id)
                    {
                        assignment.AcademicYearId = academicYear?.Id;
                    }
                }
            }

            await context.SaveChangesAsync();
        }
        else
        {
            var scheme = new GradingScheme
            {
                Name = schemeName,
                Description = BuildDescription(gradeGroup),
                IsActive = isActive
            };
            await context.GradingSchemes.AddAsync(scheme);
            await context.SaveChangesAsync();

            foreach (var gd in definitions)
                gd.GradingSchemeId = scheme.Id;
            await context.GradeDefinitions.AddRangeAsync(definitions);

            // Create junction table entries
            foreach (var program in programs)
            {
                context.GradingSchemePrograms.Add(new GradingSchemeProgram
                {
                    GradingSchemeId = scheme.Id,
                    ProgramId = program.Id,
                    AcademicYearId = academicYear?.Id,
                    IsActive = isActive
                });
            }

            await context.SaveChangesAsync();
        }

        // Remove legacy seed schemes (e.g. "CBCS Standard (4.0)", "TU ...") so each program
        // keeps only its FWU scheme.
        var legacySchemes = await context.GradingSchemes
            .Where(s => s.Name == "CBCS Standard (4.0)"
                || s.Name.StartsWith("TU ", StringComparison.OrdinalIgnoreCase))
            .ToListAsync();

        if (legacySchemes.Count > 0)
        {
            context.GradingSchemes.RemoveRange(legacySchemes);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Builds GradeDefinitions from the per-mark GradePoint.csv data of a grade group.
    /// Each grade letter becomes one range (min to max obtained mark).
    /// </summary>
    private static List<GradeDefinition> BuildDefinitions(int gradeGroupId, List<GradePoint> gradePoints)
    {
        return gradePoints
            .Where(gp => gp.GradeGroupId == gradeGroupId)
            .GroupBy(gp => gp.Grade.Trim().ToUpperInvariant())
            .Select(g => new
            {
                Grade = g.Key,
                Min = g.Min(p => p.ObtainedMark),
                Max = g.Max(p => p.ObtainedMark),
                Point = g.First().GradePointValue
            })
            .OrderBy(x => x.Min)
            .Select((x, index) => new GradeDefinition
            {
                GradeLetter = x.Grade,
                MinPercentage = x.Min,
                MaxPercentage = x.Max,
                GradePoint = x.Point,
                Remark = x.Point > 0 ? "Pass" : "Fail",
                IsPass = x.Point > 0,
                DisplayOrder = index + 1
            })
            .ToList();
    }

    private static GradeGroup? FindGroup(List<GradeGroup> gradeGroups, string name, string? remarksContaining = null)
    {
        return gradeGroups.FirstOrDefault(g => g.GradeGroupName == name
            && (remarksContaining == null
                || (g.Remarks != null && g.Remarks.Contains(remarksContaining, StringComparison.OrdinalIgnoreCase))));
    }

    private static string? ExtractFromYear(string? remarks)
    {
        if (string.IsNullOrWhiteSpace(remarks))
            return null;

        var match = Regex.Match(remarks, @"From\s+(\d{4})", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static string BuildSchemeName(GradeGroup gradeGroup, string? fromYear)
    {
        return fromYear != null
            ? $"FWU {gradeGroup.GradeGroupName} (From {fromYear})"
            : $"FWU {gradeGroup.GradeGroupName}";
    }

    private static string BuildDescription(GradeGroup gradeGroup)
    {
        return $"FWU grading scheme - {gradeGroup.GradeGroupName}"
            + (string.IsNullOrEmpty(gradeGroup.Remarks) ? "" : $" ({gradeGroup.Remarks})");
    }
}
