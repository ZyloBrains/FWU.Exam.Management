using FWU.Exam.Management.Domain.Entities;
using FluentAssertions;

namespace FWU.Exam.Management.Domain.Tests.Entities;

public class NoticeTests
{
    [Fact]
    public void CreateNotice_ShouldSetRequiredProperties()
    {
        var notice = new Notice
        {
            Id = 1,
            TenantId = 1,
            NoticeTitle = "Exam Schedule Published",
            NoticePreview = "The exam schedule for 2081 has been published",
            NoticeContent = "Detailed exam schedule content here...",
            PublishedDate = new DateTime(2026, 7, 15)
        };

        notice.Id.Should().Be(1);
        notice.TenantId.Should().Be(1);
        notice.NoticeTitle.Should().Be("Exam Schedule Published");
        notice.NoticePreview.Should().Be("The exam schedule for 2081 has been published");
        notice.NoticeContent.Should().Be("Detailed exam schedule content here...");
        notice.PublishedDate.Should().Be(new DateTime(2026, 7, 15));
    }

    [Fact]
    public void Notice_Implements_ITenantScoped()
    {
        var notice = new Notice();
        notice.Should().BeAssignableTo<FWU.Exam.Management.Domain.Interfaces.ITenantScoped>();
    }

    [Fact]
    public void Notice_PublishedDate_ShouldBeNullable()
    {
        var notice = new Notice();
        notice.PublishedDate.Should().BeNull();
    }
}
