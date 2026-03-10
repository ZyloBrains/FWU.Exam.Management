using Microsoft.AspNetCore.Identity;

namespace fwu_examination_management_system.Data
{
    public static class DbSeeder
    {
        private static readonly string[] Roles = ["SystemAdmin", "Admin", "ReportAdmin", "Student"];

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
