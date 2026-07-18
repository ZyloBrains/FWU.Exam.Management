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

    private static readonly (string Email, string FullName, string Role, string? FacultyCode, string? CollegeCode)[] SeedUsers =
    [
        ("admin@gmail.com", "Super Admin", Role.SuperAdmin, null, null),
        ("faculty@admin.com", "Faculty Admin (L091)", Role.FacultyAdmin, "L091", null),
        ("college@gmail.com", "College Admin (COC)", Role.CollegeAdmin, null, "COC"),
        ("campuschief@gmail.com", "College Admin (SOM)", Role.CollegeAdmin, null, "SOM"),
        ("student@gmail.com", "Test Student (COC)", Role.Student, null, "COC"),
        ("student2@gmail.com", "Test Student (SOM)", Role.Student, null, "SOM"),
    ];

    public static async Task SeedSuperAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        foreach (var (email, fullName, role, facultyCode, collegeCode) in SeedUsers)
        {
            var user = await userManager.FindByEmailAsync(email);
            int? facultyId = null;
            int? collegeId = null;

            if (facultyCode != null)
                facultyId = (await context.Faculties.FirstOrDefaultAsync(f => f.OfficeCode == facultyCode))?.Id;

            if (collegeCode != null)
            {
                var college = await context.Colleges.FirstOrDefaultAsync(c => c.Code == collegeCode);
                if (college != null)
                {
                    collegeId = college.Id;
                    if (facultyId == null && college.Faculties != null)
                        facultyId = college.Faculties.FirstOrDefault()?.Id;
                }
            }

            if (user == null)
            {
                user = new AppUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    IsActive = true,
                    FacultyId = facultyId,
                    CollegeId = collegeId
                };
                var result = await userManager.CreateAsync(user, "Admin@123");
                if (!result.Succeeded)
                    throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, "Admin@123");
                user.FacultyId = facultyId;
                user.CollegeId = collegeId;
                await userManager.UpdateAsync(user);
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
