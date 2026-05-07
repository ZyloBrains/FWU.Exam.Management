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

    public static async Task SeedSuperAdminAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        const string email = "admin@gmail.com";
        var user = await userManager.FindByEmailAsync(email);

        if (user != null && !await userManager.IsInRoleAsync(user, Role.SystemAdmin))
        {
            await userManager.AddToRoleAsync(user, Role.SystemAdmin);
        }
    }
}
