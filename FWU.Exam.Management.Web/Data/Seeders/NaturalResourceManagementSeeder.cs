using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class NaturalResourceManagementSeeder
{
    public static async Task SeedNaturalResourceManagementAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        var nrmFaculty = await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == "L011");

        // College
        College? nrmCollege;
        if (!await context.Colleges.IgnoreQueryFilters().AnyAsync(c => c.Code == "CDNRM"))
        {
            nrmCollege = new College
            {
                Code = "CDNRM",
                Name = "Central Department of Natural Resource Management",
                TenantId = 1,
                IsActive = true,
            };
            context.Colleges.Add(nrmCollege);
            await context.SaveChangesAsync();
            if (nrmFaculty != null)
            {
                nrmCollege.Faculties = new List<Faculty> { nrmFaculty };
                await context.SaveChangesAsync();
            }
        }
    }
}
