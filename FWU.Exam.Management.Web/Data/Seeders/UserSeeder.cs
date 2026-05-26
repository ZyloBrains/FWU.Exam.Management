using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure;
using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class UserSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Role.AllRoles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static readonly (string Email, string FullName, string Role, string? OrgCode)[] SeedUsers =
    [
        ("admin@gmail.com", "Super Admin", Role.SuperAdmin, null),
        ("faculty@admin.com", "Faculty Admin", Role.FacultyAdmin, "SOE"),
        ("college@gmail.com", "College Admin", Role.CollegeAdmin, null),
        ("student@gmail.com", "Test Student", Role.Student, null),
    ];

    public static async Task SeedSuperAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        var orgCache = new Dictionary<string, int?>();
        Faculty? GetFaculty(string? code)
        {
            if (code == null) return null;
            if (!orgCache.ContainsKey(code))
            {
                var faculty = context.Faculties.FirstOrDefault(f => f.OfficeCode == code);
                orgCache[code] = faculty?.Id;
            }
            return context.Faculties.FirstOrDefault(f => f.OfficeCode == code);
        }

        foreach (var (email, fullName, role, orgCode) in SeedUsers)
        {
            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    IsActive = true,
                    FacultyId = GetFaculty(orgCode)?.Id
                };
                var result = await userManager.CreateAsync(user, "Admin@123");
                if (!result.Succeeded)
                    throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, "Admin@123");
                user.FacultyId = GetFaculty(orgCode)?.Id;
                await userManager.UpdateAsync(user);
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
