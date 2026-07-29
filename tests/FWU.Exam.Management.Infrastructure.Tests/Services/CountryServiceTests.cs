using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class CountryServiceTests : TestBase
{
    [Fact]
    public async Task CreateCountry_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new CountryService(context);

        var country = new Country { CountryName = "Nepal", IsActive = true };
        await service.CreateCountryAsync(country);

        var result = await service.GetCountryByIdAsync(country.Id);
        result.Should().NotBeNull();
        result!.CountryName.Should().Be("Nepal");
    }

    [Fact]
    public async Task CreateFromName_ShouldReturnCreated()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new CountryService(context);

        var country = await service.CreateAsync("India");

        country.Should().NotBeNull();
        country.CountryName.Should().Be("India");
        country.IsActive.Should().BeTrue();
        country.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAll_ShouldReturnActiveOnly()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Country>().AddRange(
            new Country { CountryName = "Nepal", IsActive = true },
            new Country { CountryName = "India", IsActive = true },
            new Country { CountryName = "China", IsActive = false }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CountryService(context);
        var countries = await service.GetAllAsync();

        countries.Should().HaveCount(2);
        countries.Should().Contain(c => c.CountryName == "Nepal");
        countries.Should().NotContain(c => c.CountryName == "China");
    }

    [Fact]
    public async Task FindByName_ShouldReturn_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Country>().Add(new Country { CountryName = "Nepal", IsActive = true });
        await context.SaveChangesAsync();

        var service = new CountryService(context);
        var result = await service.FindByNameAsync("Nepal");

        result.Should().NotBeNull();
        result!.CountryName.Should().Be("Nepal");
    }

    [Fact]
    public async Task GetCountries_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Country>().AddRange(
            new Country { CountryName = "Nepal", IsActive = true },
            new Country { CountryName = "India", IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CountryService(context);
        var (items, totalCount) = await service.GetCountriesAsync(1, 10, null, "countryname", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateCountry_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var country = new Country { CountryName = "OldName", IsActive = true };
        context.Set<Country>().Add(country);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new CountryService(context);

        var existing = await service.GetCountryByIdAsync(country.Id);
        existing!.CountryName = "UpdatedName";
        await service.UpdateCountryAsync(existing);

        context.ChangeTracker.Clear();
        var updated = await service.GetCountryByIdAsync(country.Id);
        updated!.CountryName.Should().Be("UpdatedName");
    }

    [Fact]
    public async Task DeleteCountry_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var country = new Country { CountryName = "Delete Me", IsActive = true };
        context.Set<Country>().Add(country);
        await context.SaveChangesAsync();

        var service = new CountryService(context);
        await service.DeleteCountryAsync(country.Id);

        var exists = await service.CountryExistsAsync(country.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CountryExists_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<Country>().Add(new Country { CountryName = "Exists", IsActive = true });
        await context.SaveChangesAsync();

        var service = new CountryService(context);
        var result = await service.CountryExistsAsync(1);
        result.Should().BeTrue();
    }
}
