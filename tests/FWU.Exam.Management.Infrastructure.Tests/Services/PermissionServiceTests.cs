using FluentAssertions;
using FWU.Exam.Management.Domain.Entities.Permissions;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Infrastructure.Services.Permissions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class PermissionServiceTests : TestBase
{
    [Fact]
    public async Task HasPermission_ShouldReturnTrue_WhenUserHasPermission()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var user = new AppUser { Id = "user1" };
        context.Users.Add(user);

        var role = new IdentityRole("TestRole") { Id = "role1" };
        context.Roles.Add(role);

        var permission = new Permission { Name = "users.view", Group = "users", IsActive = true };
        context.Set<Permission>().Add(permission);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var result = await service.HasPermissionAsync("user1", "users.view");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermission_ShouldReturnFalse_WhenUserLacksPermission()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var user = new AppUser { Id = "user1" };
        context.Users.Add(user);

        var role = new IdentityRole("TestRole") { Id = "role1" };
        context.Roles.Add(role);

        var permission = new Permission { Name = "users.view", Group = "users", IsActive = true };
        context.Set<Permission>().Add(permission);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var result = await service.HasPermissionAsync("user1", "roles.view");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermission_ShouldReturnFalse_ForUnknownUserId()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var role = new IdentityRole("TestRole") { Id = "role1" };
        context.Roles.Add(role);

        var permission = new Permission { Name = "users.view", Group = "users", IsActive = true };
        context.Set<Permission>().Add(permission);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var result = await service.HasPermissionAsync("nonexistent", "users.view");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPermissions_ShouldReturnAllPermissions_ForUser()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var user = new AppUser { Id = "user1" };
        context.Users.Add(user);

        var role1 = new IdentityRole("Role1") { Id = "role1" };
        var role2 = new IdentityRole("Role2") { Id = "role2" };
        context.Roles.AddRange(role1, role2);

        var perm1 = new Permission { Name = "users.view", Group = "users", IsActive = true };
        var perm2 = new Permission { Name = "users.create", Group = "users", IsActive = true };
        var perm3 = new Permission { Name = "roles.view", Group = "roles", IsActive = true };
        context.Set<Permission>().AddRange(perm1, perm2, perm3);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().AddRange(
            new RolePermission { RoleId = role1.Id, PermissionId = perm1.Id },
            new RolePermission { RoleId = role1.Id, PermissionId = perm2.Id },
            new RolePermission { RoleId = role2.Id, PermissionId = perm3.Id }
        );

        context.UserRoles.AddRange(
            new IdentityUserRole<string> { UserId = user.Id, RoleId = role1.Id },
            new IdentityUserRole<string> { UserId = user.Id, RoleId = role2.Id }
        );

        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var permissions = await service.GetUserPermissionsAsync("user1");

        permissions.Should().HaveCount(3);
        permissions.Should().Contain(["users.view", "users.create", "roles.view"]);
    }

    [Fact]
    public async Task GetUserPermissions_ShouldCacheResult()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var user = new AppUser { Id = "user1" };
        context.Users.Add(user);

        var role = new IdentityRole("TestRole") { Id = "role1" };
        context.Roles.Add(role);

        var perm1 = new Permission { Name = "users.view", Group = "users", IsActive = true };
        context.Set<Permission>().Add(perm1);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = role.Id, PermissionId = perm1.Id });
        context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var firstResult = await service.GetUserPermissionsAsync("user1");
        firstResult.Should().Contain("users.view");

        var perm2 = new Permission { Name = "roles.view", Group = "roles", IsActive = true };
        context.Set<Permission>().Add(perm2);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = role.Id, PermissionId = perm2.Id });
        await context.SaveChangesAsync();

        var secondResult = await service.GetUserPermissionsAsync("user1");

        secondResult.Should().HaveCount(1);
        secondResult.Should().Contain("users.view");
        secondResult.Should().NotContain("roles.view");
    }

    [Fact]
    public async Task InvalidateCache_ShouldForceRequery()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var user = new AppUser { Id = "user1" };
        context.Users.Add(user);

        var role = new IdentityRole("TestRole") { Id = "role1" };
        context.Roles.Add(role);

        var perm1 = new Permission { Name = "users.view", Group = "users", IsActive = true };
        context.Set<Permission>().Add(perm1);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = role.Id, PermissionId = perm1.Id });
        context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var firstResult = await service.GetUserPermissionsAsync("user1");
        firstResult.Should().Contain("users.view");

        var perm2 = new Permission { Name = "roles.view", Group = "roles", IsActive = true };
        context.Set<Permission>().Add(perm2);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = role.Id, PermissionId = perm2.Id });
        await context.SaveChangesAsync();

        await service.InvalidateCacheAsync("user1");

        var secondResult = await service.GetUserPermissionsAsync("user1");

        secondResult.Should().HaveCount(2);
        secondResult.Should().Contain(["users.view", "roles.view"]);
    }

    [Fact]
    public async Task GetUserPermissions_WithGroup_ShouldFilterByPrefix()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var user = new AppUser { Id = "user1" };
        context.Users.Add(user);

        var role = new IdentityRole("TestRole") { Id = "role1" };
        context.Roles.Add(role);

        var perm1 = new Permission { Name = "Users.Read", Group = "Users", IsActive = true };
        var perm2 = new Permission { Name = "Users.Write", Group = "Users", IsActive = true };
        var perm3 = new Permission { Name = "Roles.Read", Group = "Roles", IsActive = true };
        context.Set<Permission>().AddRange(perm1, perm2, perm3);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().AddRange(
            new RolePermission { RoleId = role.Id, PermissionId = perm1.Id },
            new RolePermission { RoleId = role.Id, PermissionId = perm2.Id },
            new RolePermission { RoleId = role.Id, PermissionId = perm3.Id }
        );

        context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var permissions = await service.GetUserPermissionsAsync("user1", "Users");

        permissions.Should().HaveCount(2);
        permissions.Should().Contain(["Users.Read", "Users.Write"]);
        permissions.Should().NotContain("Roles.Read");
    }

    [Fact]
    public async Task HasAnyPermission_ShouldReturnTrue_WhenUserHasAnyOf()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var user = new AppUser { Id = "user1" };
        context.Users.Add(user);

        var role = new IdentityRole("TestRole") { Id = "role1" };
        context.Roles.Add(role);

        var permission = new Permission { Name = "users.view", Group = "users", IsActive = true };
        context.Set<Permission>().Add(permission);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var result = await service.HasAnyPermissionAsync("user1", "roles.view", "users.view", "settings.view");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasAnyPermission_ShouldReturnFalse_WhenUserHasNone()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var user = new AppUser { Id = "user1" };
        context.Users.Add(user);

        var role = new IdentityRole("TestRole") { Id = "role1" };
        context.Roles.Add(role);

        var permission = new Permission { Name = "users.view", Group = "users", IsActive = true };
        context.Set<Permission>().Add(permission);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = role.Id, PermissionId = permission.Id });
        context.UserRoles.Add(new IdentityUserRole<string> { UserId = user.Id, RoleId = role.Id });
        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var result = await service.HasAnyPermissionAsync("user1", "roles.view", "settings.view");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllRolesWithPermission_ShouldReturnMatchingRoles()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        using var context = await CreateContextAsync();

        var roleWithPerm = new IdentityRole("RoleWithPerm") { Id = "role1" };
        var roleWithoutPerm = new IdentityRole("RoleWithoutPerm") { Id = "role2" };
        context.Roles.AddRange(roleWithPerm, roleWithoutPerm);

        var permission = new Permission { Name = "users.view", Group = "users", IsActive = true };
        context.Set<Permission>().Add(permission);
        await context.SaveChangesAsync();

        context.Set<RolePermission>().Add(new RolePermission { RoleId = roleWithPerm.Id, PermissionId = permission.Id });
        await context.SaveChangesAsync();

        var service = new PermissionService(context, cache);
        var roles = await service.GetAllRolesWithPermissionAsync("users.view");

        roles.Should().HaveCount(1);
        roles[0].RoleId.Should().Be("role1");
        roles[0].RoleName.Should().Be("RoleWithPerm");
    }
}
