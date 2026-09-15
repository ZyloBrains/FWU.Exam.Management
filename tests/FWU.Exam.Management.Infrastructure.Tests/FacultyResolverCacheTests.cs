using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class FacultyResolverCacheTests
{
    private static void SeedFaculty(AppDbContext ctx, int id, string name, string officeCode)
    {
        TestData.SeedBase(ctx);
        ctx.Faculties.Add(new Faculty
        {
            Id = id,
            TenantId = TestData.TenantId,
            Name = name,
            OfficeCode = officeCode
        });
    }

    [Fact]
    public async Task ResolveFacultyAsync_CachesResult_SecondCallHitsCache()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
            SeedFaculty(ctx, 99, "Seed Faculty", "SEED"));
        var cache = new MemoryCache(new MemoryCacheOptions());
        var resolver = new FacultyResolver(db.Context, cache);

        var first = await resolver.ResolveFacultyAsync("Seed.example.edu.np");
        Assert.NotNull(first);
        Assert.Equal(99, first!.Id);

        // Neutralise both matching columns behind the resolver so a cache miss
        // would now return null; only a cache hit can still return the faculty.
        var faculty = await db.Context.Faculties.SingleAsync();
        faculty.Name = "Renamed";
        faculty.OfficeCode = "XXYYZZ";
        await db.Context.SaveChangesAsync();

        var second = await resolver.ResolveFacultyAsync("Seed.example.edu.np");

        Assert.NotNull(second);
        Assert.Equal(99, second!.Id);
        Assert.Equal("Seed Faculty", second.Name);
        Assert.Equal("SEED", second.OfficeCode);
    }

    [Fact]
    public async Task ResolveFacultyByCodeAsync_CachesResult_SecondCallHitsCache()
    {
        using var db = new TestDb(TestTenantContext.Standard(), ctx =>
            SeedFaculty(ctx, 99, "Seed Faculty", "SEED"));
        var cache = new MemoryCache(new MemoryCacheOptions());
        var resolver = new FacultyResolver(db.Context, cache);

        var first = await resolver.ResolveFacultyByCodeAsync("SEED");
        Assert.NotNull(first);
        Assert.Equal(99, first!.Id);

        var faculty = await db.Context.Faculties.SingleAsync();
        faculty.OfficeCode = "XXYYZZ";
        await db.Context.SaveChangesAsync();

        var second = await resolver.ResolveFacultyByCodeAsync("SEED");

        Assert.NotNull(second);
        Assert.Equal(99, second!.Id);
    }
}