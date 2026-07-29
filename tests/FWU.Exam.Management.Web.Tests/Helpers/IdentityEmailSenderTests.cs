using FluentAssertions;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Web.Helpers;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Helpers;

public class IdentityEmailSenderTests
{
    private readonly IEmailService _emailService;
    private readonly IdentityEmailSender _sut;

    public IdentityEmailSenderTests()
    {
        _emailService = Substitute.For<IEmailService>();
        _sut = new IdentityEmailSender(_emailService);
    }

    [Fact]
    public async Task SendEmailAsync_DelegatesToEmailService()
    {
        await _sut.SendEmailAsync("test@example.com", "Subject", "<p>Body</p>");

        await _emailService.Received(1).SendEmailAsync(
            "test@example.com",
            "Subject",
            "<p>Body</p>",
            true);
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptySubject_StillDelegates()
    {
        await _sut.SendEmailAsync("user@test.com", "", "body");

        await _emailService.Received(1).SendEmailAsync(
            "user@test.com",
            "",
            "body",
            true);
    }

    [Fact]
    public async Task SendEmailAsync_WithoutDefaultAttachments()
    {
        await _sut.SendEmailAsync("a@b.com", "Sub", "Msg");

        await _emailService.Received(1).SendEmailAsync(
            Arg.Is<string>(x => x == "a@b.com"),
            Arg.Is<string>(x => x == "Sub"),
            Arg.Is<string>(x => x == "Msg"),
            Arg.Is<bool>(x => x));
    }
}
