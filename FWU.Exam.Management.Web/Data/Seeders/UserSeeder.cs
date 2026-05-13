using FWU.Exam.Management.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class UserSeeder
{
    private static readonly string[] Roles = [Role.SystemAdmin, Role.Admin, Role.ReportAdmin, Role.Student];

    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static readonly (string Email, string FullName, string Role)[] SeedUsers =
    [
        ("admin@gmail.com", "System Admin", Role.SystemAdmin),
        ("college@gmail.com", "College Admin", Role.Admin),
        ("reporter@gmail.com", "Report Admin", Role.ReportAdmin),
        ("student@gmail.com", "Test Student", Role.Student),
    ];

    public static async Task SeedSuperAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        foreach (var (email, fullName, role) in SeedUsers)
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
                };
                var result = await userManager.CreateAsync(user, "Admin@123");
                if (!result.Succeeded)
                    throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, "Admin@123");
            }

            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }
    }
}
