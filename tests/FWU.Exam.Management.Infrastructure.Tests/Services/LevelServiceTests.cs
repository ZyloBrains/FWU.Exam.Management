using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class LevelServiceTests : TestBase
{
    [Fact]
    public async Task CreateLevel_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new LevelService(context);

        var level = new Level
        {
            LevelCode = "BACH",
            LevelName = "Bachelor",
            LevelDisplayOrder = 1,
            IsActive = true
        };

        await service.CreateLevelAsync(level);

        var result = await service.GetLevelByIdAsync(level.Id);
        result.Should().NotBeNull();
        result!.LevelName.Should().Be("Bachelor");
    }

    [Fact]
    public async Task GetLevelsAsync_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new LevelService(context);

        context.Set<Level>().Add(new Level { LevelCode = "MSTR", LevelName = "Master", LevelDisplayOrder = 2, IsActive = true });
        context.Set<Level>().Add(new Level { LevelCode = "PHD", LevelName = "PhD", LevelDisplayOrder = 3, IsActive = true });
        await context.SaveChangesAsync();

        var (items, totalCount) = await service.GetLevelsAsync(1, 10, null, "levelname", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateLevel_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var level = new Level { LevelCode = "DIP", LevelName = "Diploma", IsActive = true };
        context.Set<Level>().Add(level);
        await context.SaveChangesAsync();

        context.ChangeTracker.Clear();
        var service = new LevelService(context);

        level.LevelName = "Advanced Diploma";
        await service.UpdateLevelAsync(level);

        context.ChangeTracker.Clear();
        var result = await service.GetLevelByIdAsync(level.Id);
        result!.LevelName.Should().Be("Advanced Diploma");
    }

    [Fact]
    public async Task DeleteLevel_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var level = new Level { LevelCode = "CERT", LevelName = "Certificate", IsActive = true };
        context.Set<Level>().Add(level);
        await context.SaveChangesAsync();

        var service = new LevelService(context);
        await service.DeleteLevelAsync(level.Id);

        var exists = await service.LevelExistsAsync(level.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task LevelExists_ShouldReturnFalse_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new LevelService(context);

        var exists = await service.LevelExistsAsync(999);

        exists.Should().BeFalse();
    }
}
