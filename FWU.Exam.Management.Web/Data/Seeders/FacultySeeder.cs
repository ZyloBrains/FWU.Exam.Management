using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class FacultySeeder
{
    public static async Task SeedFacultiesAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.Faculties.AnyAsync())
            return;

        var faculties = new[]
        {
            new Faculty { Name = "Management", OfficeCode = "L001" },
            new Faculty { Name = "Science and Technology", OfficeCode = "L002" },
            new Faculty { Name = "Humanities and Social Sciences", OfficeCode = "L003" },
            new Faculty { Name = "Education", OfficeCode = "L006" },
            new Faculty { Name = "Health Sciences", OfficeCode = "L010" },
            new Faculty { Name = "Natural Resource Management", OfficeCode = "L011" },
            new Faculty { Name = "Engineering", OfficeCode = "L091" },
            new Faculty { Name = "Agriculture Science", OfficeCode = "L103" },
            new Faculty { Name = "Law", OfficeCode = "L140" },
        };

        await context.Faculties.AddRangeAsync(faculties);
        await context.SaveChangesAsync();
    }
}
