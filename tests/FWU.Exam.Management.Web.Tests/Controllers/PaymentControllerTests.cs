using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Interfaces;
using FWU.Exam.Management.Infrastructure.Data.Models;
using FWU.Exam.Management.Web.Areas.Exams.Controllers;
using FWU.Exam.Management.Web.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Controllers;

public class PaymentControllerTests
{
    private readonly IEntranceExamApplicationService _service = Substitute.For<IEntranceExamApplicationService>();
    private readonly IExamScheduleService _examScheduleService = Substitute.For<IExamScheduleService>();
    private readonly IESewaService _esewaService = Substitute.For<IESewaService>();
    private readonly IKhaltiService _khaltiService = Substitute.For<IKhaltiService>();
    private readonly IFileUploadHelper _fileUploadHelper = Substitute.For<IFileUploadHelper>();
    private readonly UserManager<AppUser> _userManager;
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly ILogger<EntranceController> _logger = Substitute.For<ILogger<EntranceController>>();
    private readonly EntranceController _controller;

    public PaymentControllerTests()
    {
        _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(), null, null, null, null, null, null, null, null);

        _controller = new EntranceController(_service, _examScheduleService, _esewaService, _khaltiService, _fileUploadHelper, _userManager, _userContext, _logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            TempData = new TempDataDictionary(new DefaultHttpContext(), Substitute.For<ITempDataProvider>())
        };
    }

    [Fact]
    public void VerifyPayment_ShouldReturnView()
    {
        var result = _controller.VerifyPayment();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public async Task InitiatePayment_POST_ShouldRedirectToESewaPayment()
    {
        _service.HasExistingVoucherAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>()).Returns(false);
        _service.GetAvailableExamSchedulesAsync().Returns([
            new FWU.Exam.Management.Application.DTOs.AvailableScheduleDto { Id = 1, ExamFee = 1000 }
        ]);
        _esewaService.GenerateTransactionUuid().Returns("test-uuid");
        _service.CreateEsewaPaymentLogAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>()).Returns(1);

        var result = await _controller.InitiatePayment(1, "John Doe", "1234567890", 1);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("ESewaPayment");
    }

    [Fact]
    public void PaymentSuccess_ShouldReturnView()
    {
        var result = _controller.PaymentSuccess();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void ESewaFailure_ShouldRedirectToVerifyPayment()
    {
        var result = _controller.ESewaFailure();

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("VerifyPayment");
    }
}
