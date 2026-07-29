using System.Net;
using FWU.Exam.Management.Domain.Entities;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class SmsServiceTests : TestBase
{
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public MockHttpMessageHandler(HttpResponseMessage response) => _response = response;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }

    private static HttpClient CreateHttpClient(HttpStatusCode status) =>
        new(new MockHttpMessageHandler(new HttpResponseMessage(status)));

    [Fact]
    public async Task SendSmsAsync_ShouldThrow_WhenNoConfig()
    {
        using var context = await CreateContextAsync();
        var service = new SmsService(context, CreateHttpClient(HttpStatusCode.OK));

        await service.Invoking(s => s.SendSmsAsync("+9779800000000", "Test"))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendSmsAsync_ShouldThrow_WhenApiFails()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<SmsConfiguration>().Add(new SmsConfiguration
        {
            ApiUrl = "https://api.gumpnow.com/sms",
            ApiKey = "test-key",
            Mode = "prod",
            Tags = "entrance",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new SmsService(context, CreateHttpClient(HttpStatusCode.InternalServerError));

        await service.Invoking(s => s.SendSmsAsync("+9779800000000", "Test"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("SMS sending failed*");
    }

    [Fact]
    public async Task SendSmsAsync_ShouldSendSuccessfully()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<SmsConfiguration>().Add(new SmsConfiguration
        {
            ApiUrl = "https://api.gumpnow.com/sms",
            ApiKey = "test-key",
            Mode = "prod",
            Tags = "entrance,exam",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new SmsService(context, CreateHttpClient(HttpStatusCode.OK));

        await service.Invoking(s => s.SendSmsAsync("+9779800000000", "Test message"))
            .Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendSmsAsync_ShouldUseActiveConfig()
    {
        using var context = await CreateContextAsync();
        await SeedTenantAsync(context);

        context.Set<SmsConfiguration>().Add(new SmsConfiguration
        {
            ApiUrl = "https://api.gumpnow.com/sms",
            ApiKey = "inactive-key",
            Mode = "prod",
            Tags = "test",
            IsActive = false
        });
        context.Set<SmsConfiguration>().Add(new SmsConfiguration
        {
            ApiUrl = "https://api.gumpnow.com/sms",
            ApiKey = "active-key",
            Mode = "prod",
            Tags = "test",
            IsActive = true
        });
        await context.SaveChangesAsync();

        var service = new SmsService(context, CreateHttpClient(HttpStatusCode.OK));

        await service.Invoking(s => s.SendSmsAsync("+9779800000000", "Test"))
            .Should().NotThrowAsync();
    }
}
