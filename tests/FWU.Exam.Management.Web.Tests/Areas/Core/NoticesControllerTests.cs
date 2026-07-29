using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Web.Areas.Core.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Areas.Core;

public class NoticesControllerTests
{
    private readonly INoticeService _noticeService;
    private readonly NoticesController _controller;

    public NoticesControllerTests()
    {
        _noticeService = Substitute.For<INoticeService>();

        var httpContext = new DefaultHttpContext();
        _controller = new NoticesController(_noticeService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, Substitute.For<ITempDataProvider>())
        };
    }

    [Fact]
    public async Task Index_ShouldReturnView()
    {
        _noticeService.GetNoticesAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((new List<Notice>(), 0));

        var result = await _controller.Index();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Get_ShouldReturnView()
    {
        var result = _controller.Create();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Create_Post_WithValidModel_ShouldRedirectToIndex()
    {
        var notice = new Notice { NoticeTitle = "Test Notice" };

        _controller.ModelState.Clear();
        var result = await _controller.Create(notice);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task Create_Post_WithInvalidModel_ShouldReturnView()
    {
        var notice = new Notice();
        _controller.ModelState.AddModelError("NoticeTitle", "Required");

        var result = await _controller.Create(notice);

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task Details_WithNullId_ShouldReturnNotFound()
    {
        var result = await _controller.Details(null);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_WithInvalidId_ShouldReturnNotFound()
    {
        _noticeService.GetNoticeByIdAsync(999).Returns((Notice?)null);

        var result = await _controller.Details(999);

        result.Should().BeOfType<NotFoundResult>();
    }
}
