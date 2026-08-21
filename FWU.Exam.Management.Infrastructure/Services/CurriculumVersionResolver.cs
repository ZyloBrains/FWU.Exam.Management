using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Infrastructure.Services;

public static class CurriculumVersionResolver
{
    public static async Task<int?> ResolveAsync(AppDbContext context, int programId, int academicYearId)
    {
        return await context.CurriculumVersions!
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(cv => cv.ProgramId == programId
                      && cv.EffectiveAcademicYearId <= academicYearId)
            .OrderByDescending(cv => cv.EffectiveAcademicYearId)
            .ThenByDescending(cv => cv.IsActive)
            .ThenByDescending(cv => cv.Id)
            .Select(cv => (int?)cv.Id)
            .FirstOrDefaultAsync();
    }
}
