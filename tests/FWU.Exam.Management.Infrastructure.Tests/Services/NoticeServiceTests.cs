using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class NoticeServiceTests : TestBase
{
    [Fact]
    public async Task GetAllNotices_ShouldReturnPaged()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        for (var i = 1; i <= 5; i++)
        {
            context.Set<Notice>().Add(new Notice
            {
                TenantId = TestTenantId,
                NoticeTitle = $"Notice {i}",
                NoticePreview = $"Preview {i}",
                NoticeContent = $"Content {i}",
                PublishedDate = DateTime.UtcNow.AddDays(-i)
            });
        }
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new NoticeService(context);

        var (items, totalCount) = await service.GetNoticesAsync(1, 3, null, "publisheddate", "desc");

        totalCount.Should().Be(5);
        items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetNoticeById_ShouldReturn_WhenExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var notice = new Notice { TenantId = TestTenantId, NoticeTitle = "Test", NoticePreview = "Preview", NoticeContent = "Content", PublishedDate = DateTime.UtcNow };
        context.Set<Notice>().Add(notice);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new NoticeService(context);

        var result = await service.GetNoticeByIdAsync(notice.Id);

        result.Should().NotBeNull();
        result!.NoticeTitle.Should().Be("Test");
    }

    [Fact]
    public async Task CreateNotice_ShouldPersist()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var service = new NoticeService(context);

        var notice = new Notice { TenantId = TestTenantId, NoticeTitle = "New", NoticePreview = "Preview", NoticeContent = "Content", PublishedDate = DateTime.UtcNow };
        await service.CreateNoticeAsync(notice);

        notice.Id.Should().BeGreaterThan(0);
        context.ChangeTracker.Clear();

        var saved = await service.GetNoticeByIdAsync(notice.Id);
        saved.Should().NotBeNull();
        saved!.NoticeTitle.Should().Be("New");
        saved.NoticeContent.Should().Be("Content");
    }

    [Fact]
    public async Task UpdateNotice_ShouldModify()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var notice = new Notice { TenantId = TestTenantId, NoticeTitle = "Original", NoticePreview = "Preview", NoticeContent = "Content", PublishedDate = DateTime.UtcNow };
        context.Set<Notice>().Add(notice);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new NoticeService(context);

        var existing = await service.GetNoticeByIdAsync(notice.Id);
        existing!.NoticeTitle = "Updated Title";
        await service.UpdateNoticeAsync(existing);

        context.ChangeTracker.Clear();
        var updated = await service.GetNoticeByIdAsync(notice.Id);
        updated!.NoticeTitle.Should().Be("Updated Title");
    }

    [Fact]
    public async Task DeleteNotice_ShouldRemove()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var notice = new Notice { TenantId = TestTenantId, NoticeTitle = "To Delete", NoticePreview = "Preview", NoticeContent = "Content", PublishedDate = DateTime.UtcNow };
        context.Set<Notice>().Add(notice);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = new NoticeService(context);

        await service.DeleteNoticeAsync(notice.Id);

        var exists = await service.NoticeExistsAsync(notice.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetNoticeById_ShouldReturnNull_WhenNotExists()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        var service = new NoticeService(context);

        var result = await service.GetNoticeByIdAsync(999);

        result.Should().BeNull();
    }
}
