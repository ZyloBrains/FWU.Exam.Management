using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class GradingSeeder
{
    public static async Task SeedGradingDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.GradingSchemes.AnyAsync())
            return;

        var program = await context.Programs.FirstOrDefaultAsync(p => p.ProgramCode == "BSCSIT");
        if (program == null)
            return;

        var cbcScheme = new GradingScheme
        {
            Name = "CBCS Standard (4.0)",
            Description = "Choice Based Credit System - Standard 4.0 scale",
            ProgramId = program.Id,
            IsActive = true
        };
        await context.GradingSchemes.AddAsync(cbcScheme);
        await context.SaveChangesAsync();

        var cbcGrades = new GradeDefinition[]
        {
            new() { GradingSchemeId = cbcScheme.Id, GradeLetter = "A+", MinPercentage = 90, MaxPercentage = 100, GradePoint = 4.0m, Remark = "Outstanding", IsPass = true, DisplayOrder = 1 },
            new() { GradingSchemeId = cbcScheme.Id, GradeLetter = "A", MinPercentage = 80, MaxPercentage = 89.99m, GradePoint = 3.7m, Remark = "Excellent", IsPass = true, DisplayOrder = 2 },
            new() { GradingSchemeId = cbcScheme.Id, GradeLetter = "B+", MinPercentage = 70, MaxPercentage = 79.99m, GradePoint = 3.3m, Remark = "Very Good", IsPass = true, DisplayOrder = 3 },
            new() { GradingSchemeId = cbcScheme.Id, GradeLetter = "B", MinPercentage = 60, MaxPercentage = 69.99m, GradePoint = 3.0m, Remark = "Good", IsPass = true, DisplayOrder = 4 },
            new() { GradingSchemeId = cbcScheme.Id, GradeLetter = "C+", MinPercentage = 50, MaxPercentage = 59.99m, GradePoint = 2.7m, Remark = "Above Average", IsPass = true, DisplayOrder = 5 },
            new() { GradingSchemeId = cbcScheme.Id, GradeLetter = "C", MinPercentage = 45, MaxPercentage = 49.99m, GradePoint = 2.3m, Remark = "Average", IsPass = true, DisplayOrder = 6 },
            new() { GradingSchemeId = cbcScheme.Id, GradeLetter = "D", MinPercentage = 40, MaxPercentage = 44.99m, GradePoint = 2.0m, Remark = "Pass", IsPass = true, DisplayOrder = 7 },
            new() { GradingSchemeId = cbcScheme.Id, GradeLetter = "F", MinPercentage = 0, MaxPercentage = 39.99m, GradePoint = 0.0m, Remark = "Fail", IsPass = false, DisplayOrder = 8 },
        };
        await context.GradeDefinitions.AddRangeAsync(cbcGrades);
        await context.SaveChangesAsync();

        var tuScheme = new GradingScheme
        {
            Name = "TU Semester System",
            Description = "Tribhuvan University semester grading",
            ProgramId = program.Id,
            IsActive = true
        };
        await context.GradingSchemes.AddAsync(tuScheme);
        await context.SaveChangesAsync();

        var tuGrades = new GradeDefinition[]
        {
            new() { GradingSchemeId = tuScheme.Id, GradeLetter = "A+", MinPercentage = 90, MaxPercentage = 100, GradePoint = 4.0m, Remark = "Outstanding", IsPass = true, DisplayOrder = 1 },
            new() { GradingSchemeId = tuScheme.Id, GradeLetter = "A", MinPercentage = 80, MaxPercentage = 89.99m, GradePoint = 3.7m, Remark = "Excellent", IsPass = true, DisplayOrder = 2 },
            new() { GradingSchemeId = tuScheme.Id, GradeLetter = "B+", MinPercentage = 70, MaxPercentage = 79.99m, GradePoint = 3.3m, Remark = "Very Good", IsPass = true, DisplayOrder = 3 },
            new() { GradingSchemeId = tuScheme.Id, GradeLetter = "B", MinPercentage = 60, MaxPercentage = 69.99m, GradePoint = 3.0m, Remark = "Good", IsPass = true, DisplayOrder = 4 },
            new() { GradingSchemeId = tuScheme.Id, GradeLetter = "C+", MinPercentage = 50, MaxPercentage = 59.99m, GradePoint = 2.7m, Remark = "Above Average", IsPass = true, DisplayOrder = 5 },
            new() { GradingSchemeId = tuScheme.Id, GradeLetter = "C", MinPercentage = 45, MaxPercentage = 49.99m, GradePoint = 2.3m, Remark = "Average", IsPass = true, DisplayOrder = 6 },
            new() { GradingSchemeId = tuScheme.Id, GradeLetter = "D", MinPercentage = 40, MaxPercentage = 44.99m, GradePoint = 2.0m, Remark = "Pass", IsPass = true, DisplayOrder = 7 },
            new() { GradingSchemeId = tuScheme.Id, GradeLetter = "F", MinPercentage = 0, MaxPercentage = 39.99m, GradePoint = 0.0m, Remark = "Fail", IsPass = false, DisplayOrder = 8 },
        };
        await context.GradeDefinitions.AddRangeAsync(tuGrades);
        await context.SaveChangesAsync();
    }
}
