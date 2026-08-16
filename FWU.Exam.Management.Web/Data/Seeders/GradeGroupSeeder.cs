using System.Globalization;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class GradeGroupSeeder
{
    public static async Task SeedGradeGroupsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        if (await context.GradeGroups.AnyAsync())
            return;

        var environment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
        if (string.IsNullOrEmpty(environment.WebRootPath))
            return;

        var groupPath = Path.Combine(environment.WebRootPath, "GradeGroup.csv");
        var pointPath = Path.Combine(environment.WebRootPath, "GradePoint.csv");
        if (!File.Exists(groupPath) || !File.Exists(pointPath))
            return;

        var gradeGroups = new List<GradeGroup>();
        foreach (var line in File.ReadAllLines(groupPath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = line.Split(',');
            if (fields.Length < 2 || !int.TryParse(fields[0].Trim(), out var groupId))
                continue;

            gradeGroups.Add(new GradeGroup
            {
                Id = groupId,
                GradeGroupName = fields[1].Trim(),
                Remarks = fields.Length > 2 ? NullIfEmpty(fields[2]) : null,
                CreatedBy = fields.Length > 3 ? NullIfEmpty(fields[3]) : null,
                CreatedDate = fields.Length > 4 && DateTime.TryParse(fields[4].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var createdDate)
                    ? createdDate
                    : null,
            });
        }

        if (gradeGroups.Count == 0)
            return;

        await context.GradeGroups.AddRangeAsync(gradeGroups);

        var gradePoints = new List<GradePoint>();
        foreach (var line in File.ReadAllLines(pointPath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = line.Split(',');
            if (fields.Length < 5)
                continue;

            if (!int.TryParse(fields[0].Trim(), out var pointId) ||
                !int.TryParse(fields[2].Trim(), out var obtainedMark) ||
                !int.TryParse(fields[4].Trim(), out var gradeGroupId))
                continue;

            if (!gradeGroups.Any(g => g.Id == gradeGroupId))
                continue;

            if (!decimal.TryParse(fields[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var gradePointValue))
                continue;

            gradePoints.Add(new GradePoint
            {
                Id = pointId,
                Grade = fields[1].Trim(),
                ObtainedMark = obtainedMark,
                GradePointValue = gradePointValue,
                GradeGroupId = gradeGroupId,
            });
        }

        if (gradePoints.Count == 0)
            return;

        await context.GradePoints.AddRangeAsync(gradePoints);
        await context.SaveChangesAsync();
    }

    private static string? NullIfEmpty(string value)
    {
        var trimmed = value.Trim();
        return trimmed.Length == 0 || string.Equals(trimmed, "NULL", StringComparison.OrdinalIgnoreCase)
            ? null
            : trimmed;
    }
}
