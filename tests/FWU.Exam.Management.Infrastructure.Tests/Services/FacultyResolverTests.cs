using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class FacultyResolverTests : TestBase
{
    [Fact]
    public async Task ResolveFacultyAsync_WithSubdomainMatch_ShouldReturnFaculty()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Faculty>().Add(new Faculty
        {
            Name = "science", OfficeCode = "SCI", ShortName = "SCI",
            ContactNumber = "01-5550001", Address = "KTM", Email = "sci@fwu.edu.np",
            TenantId = TestTenantId
        });
        await context.SaveChangesAsync();

        var service = new FacultyResolver(context);

        var result = await service.ResolveFacultyAsync("science.fwu.edu.np");

        result.Should().NotBeNull();
        result!.Name.Should().Be("science");
    }

    [Fact]
    public async Task ResolveFacultyAsync_WithOfficeCodeMatch_ShouldReturnFaculty()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Faculty>().Add(new Faculty
        {
            Name = "Faculty of Management", OfficeCode = "mgt", ShortName = "MGT",
            ContactNumber = "01-5550002", Address = "KTM", Email = "mgt@fwu.edu.np",
            TenantId = TestTenantId
        });
        await context.SaveChangesAsync();

        var service = new FacultyResolver(context);

        var result = await service.ResolveFacultyAsync("mgt.fwu.edu.np");

        result.Should().NotBeNull();
        result!.OfficeCode.Should().Be("mgt");
    }

    [Fact]
    public async Task ResolveFacultyAsync_WithNoSubdomain_ShouldReturnNull()
    {
        using var context = await CreateContextAsync();
        var service = new FacultyResolver(context);

        var result = await service.ResolveFacultyAsync("fwu.edu.np");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ResolveFacultyByCodeAsync_ShouldReturnFaculty()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Faculty>().Add(new Faculty
        {
            Name = "Faculty of Education", OfficeCode = "EDU", ShortName = "EDU",
            ContactNumber = "01-5550003", Address = "KTM", Email = "edu@fwu.edu.np",
            TenantId = TestTenantId
        });
        await context.SaveChangesAsync();

        var service = new FacultyResolver(context);

        var result = await service.ResolveFacultyByCodeAsync("EDU");

        result.Should().NotBeNull();
        result!.Name.Should().Be("Faculty of Education");
    }

    [Fact]
    public async Task ResolveFacultyByCodeAsync_WithUnknownCode_ShouldReturnNull()
    {
        using var context = await CreateContextAsync();
        var service = new FacultyResolver(context);

        var result = await service.ResolveFacultyByCodeAsync("NONEXISTENT");

        result.Should().BeNull();
    }
}
