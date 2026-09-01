using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class AcademicYearSeeder
{
    public static async Task SeedAcademicYearsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.AcademicYears.IgnoreQueryFilters().AnyAsync())
            return;

        var tenants = await context.Tenants.Where(t => t.IsActive).ToListAsync();
        if (tenants.Count == 0)
            return;

        foreach (var tenant in tenants)
        {
            var academicYears = new[]
            {
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2013", AcademicYearName = "2013", AcademicYearNameNepali = "२०७०", Remark = "Y001", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2014", AcademicYearName = "2014", AcademicYearNameNepali = "२०७१", Remark = "Y002", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2015", AcademicYearName = "2015", AcademicYearNameNepali = "२०७२", Remark = "Y003", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2016", AcademicYearName = "2016", AcademicYearNameNepali = "२०७३", Remark = "Y004", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2017", AcademicYearName = "2017", AcademicYearNameNepali = "२०७४", Remark = "Y005", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2018", AcademicYearName = "2018", AcademicYearNameNepali = "२०७५", Remark = "Y006", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2019", AcademicYearName = "2019", AcademicYearNameNepali = "२०७६", Remark = "Y007", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2020", AcademicYearName = "2020", AcademicYearNameNepali = "२०७७", Remark = "Y008", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2021", AcademicYearName = "2021", AcademicYearNameNepali = "२०७८", Remark = "Y009", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2022", AcademicYearName = "2022", AcademicYearNameNepali = "२०७९", Remark = "Y010", IsRunning = false, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2023", AcademicYearName = "2023", AcademicYearNameNepali = "२०८०", Remark = "Y011", IsRunning = true, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2024", AcademicYearName = "2024", AcademicYearNameNepali = "२०८१", Remark = "Y012", IsRunning = true, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2025", AcademicYearName = "2025", AcademicYearNameNepali = "२०८२", Remark = "Y013", IsRunning = true, IsActive = true },
                new AcademicYear { TenantId = tenant.Id, AcademicYearCode = "2026", AcademicYearName = "2026", AcademicYearNameNepali = "२०८३", Remark = "Y014", IsRunning = true, IsActive = true },
            };

            await context.AcademicYears.AddRangeAsync(academicYears);
        }

        await context.SaveChangesAsync();
    }
}
