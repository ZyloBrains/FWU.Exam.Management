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
            new Faculty { Name = "Agriculture Science", OfficeCode = "L103", ShortName = "AG" },
            new Faculty { Name = "Education", OfficeCode = "L006", ShortName = "ED" },
            new Faculty { Name = "Engineering", OfficeCode = "L091", ShortName = "EG" },
            new Faculty { Name = "Humanities and Social Sciences", OfficeCode = "L003", ShortName = "HU" },
            new Faculty { Name = "Law", OfficeCode = "L140", ShortName = "LW" },
            new Faculty { Name = "Management", OfficeCode = "L001", ShortName = "MG" },
            new Faculty { Name = "Science and Technology", OfficeCode = "L002", ShortName = "SC" },
            new Faculty { Name = "Health Sciences", OfficeCode = "L010", ShortName = "HS" },
            new Faculty { Name = "Natural Resource Management", OfficeCode = "L011", ShortName = "NR" },
        };

        await context.Faculties.AddRangeAsync(faculties);
        await context.SaveChangesAsync();
    }
}
