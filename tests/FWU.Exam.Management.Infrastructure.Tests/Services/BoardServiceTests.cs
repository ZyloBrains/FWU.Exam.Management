using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class BoardServiceTests : TestBase
{
    private async Task<Country> SeedCountryAsync(AppDbContext context)
    {
        var country = new Country { CountryName = "Nepal", IsActive = true };
        context.Set<Country>().Add(country);
        await context.SaveChangesAsync();
        return country;
    }

    [Fact]
    public async Task CreateBoard_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var country = await SeedCountryAsync(context);
        var service = new BoardService(context);

        var board = new Board { BoardName = "NEB", CountryId = country.Id, Remarks = "National Exam Board", IsActive = true };
        await service.CreateBoardAsync(board);

        var result = await service.GetBoardByIdAsync(board.Id);
        result.Should().NotBeNull();
        result!.BoardName.Should().Be("NEB");
    }

    [Fact]
    public async Task GetBoards_ShouldReturnPagedResults()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var country = await SeedCountryAsync(context);

        context.Set<Board>().AddRange(
            new Board { BoardName = "Board A", CountryId = country.Id, Remarks = "", IsActive = true },
            new Board { BoardName = "Board B", CountryId = country.Id, Remarks = "", IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new BoardService(context);
        var (items, totalCount) = await service.GetBoardsAsync(1, 10, null, "boardname", "asc");

        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBoards_WithSearch_ShouldFilter()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var country = await SeedCountryAsync(context);

        context.Set<Board>().AddRange(
            new Board { BoardName = "NEB", CountryId = country.Id, Remarks = "National", IsActive = true },
            new Board { BoardName = "HSEB", CountryId = country.Id, Remarks = "Higher Secondary", IsActive = true }
        );
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new BoardService(context);
        var (items, totalCount) = await service.GetBoardsAsync(1, 10, "NEB", "boardname", "asc");

        totalCount.Should().Be(1);
        items[0].BoardName.Should().Be("NEB");
    }

    [Fact]
    public async Task UpdateBoard_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var country = await SeedCountryAsync(context);

        var board = new Board { BoardName = "Original", CountryId = country.Id, Remarks = "", IsActive = true };
        context.Set<Board>().Add(board);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new BoardService(context);

        var existing = await service.GetBoardByIdAsync(board.Id);
        existing!.BoardName = "Updated";
        await service.UpdateBoardAsync(existing);

        context.ChangeTracker.Clear();
        var updated = await service.GetBoardByIdAsync(board.Id);
        updated!.BoardName.Should().Be("Updated");
    }

    [Fact]
    public async Task DeleteBoard_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var country = await SeedCountryAsync(context);

        var board = new Board { BoardName = "Delete Me", CountryId = country.Id, Remarks = "", IsActive = true };
        context.Set<Board>().Add(board);
        await context.SaveChangesAsync();

        var service = new BoardService(context);
        await service.DeleteBoardAsync(board.Id);

        var exists = await service.BoardExistsAsync(board.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task BoardExists_ShouldReturnTrue_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var country = await SeedCountryAsync(context);

        var board = new Board { BoardName = "Exists", CountryId = country.Id, Remarks = "", IsActive = true };
        context.Set<Board>().Add(board);
        await context.SaveChangesAsync();

        var service = new BoardService(context);
        var exists = await service.BoardExistsAsync(board.Id);
        exists.Should().BeTrue();
    }
}
