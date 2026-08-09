using FWU.Exam.Management.Domain.Entities.Permissions;
using FWU.Exam.Management.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class PermissionSeeder
{
    public static async Task SeedPermissionsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        var existingNames = await context.Permissions!.Select(p => p.Name).ToListAsync();
        var missing = Permissions.All.Where(p => !existingNames.Contains(p.Name)).ToList();

        if (missing.Count > 0)
        {
            var permissionEntities = missing.Select(p => new Permission
            {
                Name = p.Name,
                DisplayName = p.DisplayName,
                Description = p.Description,
                Group = p.Group,
                IsActive = true,
            }).ToList();

            await context.Permissions!.AddRangeAsync(permissionEntities);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedRolePermissionsAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var (roleName, permissionNames) in Permissions.RolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                role = await roleManager.FindByNameAsync(roleName);
            }

            var existingPermIds = await context.RolePermissions!
                .Where(rp => rp.RoleId == role!.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var existingPermNames = await context.Permissions!
                .Where(p => existingPermIds.Contains(p.Id))
                .Select(p => p.Name)
                .ToListAsync();

            var missingPermNames = permissionNames.Except(existingPermNames).ToList();
            if (missingPermNames.Count > 0)
            {
                var missingPermissions = await context.Permissions!
                    .Where(p => missingPermNames.Contains(p.Name))
                    .ToListAsync();

                var rolePermissions = missingPermissions.Select(p => new RolePermission
                {
                    RoleId = role!.Id,
                    PermissionId = p.Id,
                }).ToList();

                await context.RolePermissions!.AddRangeAsync(rolePermissions);
            }

            var extraPermNames = existingPermNames.Except(permissionNames).ToList();
            if (extraPermNames.Count > 0)
            {
                var extraPermIds = await context.Permissions!
                    .Where(p => extraPermNames.Contains(p.Name))
                    .Select(p => p.Id)
                    .ToListAsync();

                var staleRolePermissions = await context.RolePermissions!
                    .Where(rp => rp.RoleId == role!.Id && extraPermIds.Contains(rp.PermissionId))
                    .ToListAsync();

                context.RolePermissions!.RemoveRange(staleRolePermissions);
            }
        }

        await context.SaveChangesAsync();
    }

    public static async Task SeedAllAsync(IServiceProvider serviceProvider)
    {
        await SeedPermissionsAsync(serviceProvider);
        await SeedRolePermissionsAsync(serviceProvider);
    }

    public static async Task<string> VerifyUatAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("===== UAT Verification: Permission Seeder =====");
        sb.AppendLine();

        // 1. Check total permissions seeded
        var totalPermissions = await context.Permissions!.CountAsync();
        var expectedPermissions = Permissions.All.Count;
        sb.AppendLine($"Permissions: {totalPermissions} seeded / {expectedPermissions} expected");
        if (totalPermissions != expectedPermissions)
            sb.AppendLine($"  >>> WARNING: Mismatch! Missing {expectedPermissions - totalPermissions} permissions.");

        // 2. List all permissions grouped
        sb.AppendLine();
        sb.AppendLine("--- All Permissions by Group ---");
        var groups = await context.Permissions!
            .OrderBy(p => p.Group)
            .ThenBy(p => p.Name)
            .ToListAsync();
        foreach (var group in groups.GroupBy(p => p.Group))
        {
            sb.AppendLine($"  [{group.Key}] ({group.Count()} permissions)");
            foreach (var perm in group)
                sb.AppendLine($"    - {perm.Name} ({perm.DisplayName})");
        }

        // 3. Check roles and their permissions
        sb.AppendLine();
        sb.AppendLine("--- Role-Permission Assignments ---");
        foreach (var (roleName, _) in Permissions.RolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            var count = 0;
            if (role != null)
                count = await context.RolePermissions!.CountAsync(rp => rp.RoleId == role.Id);

            var expected = Permissions.RolePermissions[roleName].Length;
            var status = count == expected ? "OK" : $"MISMATCH (got {count}, expected {expected})";
            sb.AppendLine($"  {roleName}: {count} permissions assigned [{status}]");
        }

        // 4. Specific role permission spot-checks
        sb.AppendLine();
        sb.AppendLine("--- Spot Checks ---");
        foreach (var (roleName, permissionNames) in Permissions.RolePermissions)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                sb.AppendLine($"  {roleName}: Role NOT FOUND in database");
                continue;
            }

            var assigned = await context.RolePermissions!
                .Where(rp => rp.RoleId == role.Id)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var permNames = await context.Permissions!
                .Where(p => assigned.Contains(p.Id))
                .Select(p => p.Name)
                .ToListAsync();

            var missing = permissionNames.Except(permNames).ToList();
            var extra = permNames.Except(permissionNames).ToList();

            if (missing.Count == 0 && extra.Count == 0)
            {
                sb.AppendLine($"  {roleName}: All permissions match exactly [PASS]");
            }
            else
            {
                if (missing.Count > 0)
                    sb.AppendLine($"  {roleName}: MISSING permissions [{string.Join(", ", missing)}]");
                if (extra.Count > 0)
                    sb.AppendLine($"  {roleName}: EXTRA permissions [{string.Join(", ", extra)}]");
            }
        }

        // 5. Summary
        sb.AppendLine();
        var allOk = totalPermissions == expectedPermissions;
        sb.AppendLine(allOk ? ">>> UAT: ALL CHECKS PASSED <<<" : ">>> UAT: SOME CHECKS FAILED <<<");
        sb.AppendLine("===========================================");

        return sb.ToString();
    }
}
