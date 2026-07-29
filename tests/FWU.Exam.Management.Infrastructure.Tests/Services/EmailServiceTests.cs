using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class EmailServiceTests : TestBase
{
    [Fact]
    public async Task SendEmailAsync_ShouldReturnSilently_WhenNoSmtpConfig()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new EmailService(context);

        var act = () => service.SendEmailAsync("test@test.com", "Subject", "Body");

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendEmailAsync_ShouldThrow_WhenSmtpConfigIsInvalid()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<SmtpConfiguration>().Add(new SmtpConfiguration
        {
            Host = "invalid.smtp.test",
            Port = 25,
            From = "test@test.com",
            UserName = "user",
            Password = "pass",
            EnableSsl = false,
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new EmailService(context);

        var act = () => service.SendEmailAsync("to@test.com", "Subject", "Body");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*SMTP error*");
    }

    [Fact]
    public async Task SendEmailAsync_ShouldHandleNullAttachments()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);
        var service = new EmailService(context);

        var act = () => service.SendEmailAsync("test@test.com", "Sub", "Body", true, null);

        await act.Should().NotThrowAsync();
    }
}
