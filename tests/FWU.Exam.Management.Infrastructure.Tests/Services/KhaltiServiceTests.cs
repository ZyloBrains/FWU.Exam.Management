using System.Net;
using System.Net.Http;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class KhaltiServiceTests
{
    private static IConfiguration CreateConfig()
    {
        var config = Substitute.For<IConfiguration>();
        config["Khalti:BaseUrl"].Returns("https://dev.khalti.com/api/v2");
        config["Khalti:SecretKey"].Returns("test-secret-key");
        config["Khalti:WebsiteUrl"].Returns("https://example.com");
        return config;
    }

    private static ILogger<KhaltiService> CreateLogger() =>
        Substitute.For<ILogger<KhaltiService>>();

    [Fact]
    public async Task InitiatePaymentAsync_WithSuccessfulResponse_ShouldReturnResult()
    {
        var config = CreateConfig();
        var logger = CreateLogger();
        var handler = new MockHttpMessageHandler(request =>
        {
            request.RequestUri.Should().NotBeNull();
            request.RequestUri!.AbsolutePath.Should().EndWith("/initiate/");
            request.Headers.Authorization.Should().NotBeNull();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "pidx": "pxYZ12345",
                        "payment_url": "https://dev.khalti.com/api/v2/epayment/initiate/",
                        "expires_at": "2026-07-29T12:00:00Z",
                        "expires_in": 1800
                    }
                    """)
            };
        });
        using var httpClient = new HttpClient(handler);
        var service = new KhaltiService(httpClient, config, logger);

        var request = new KhaltiInitiateRequest
        {
            ReturnUrl = "https://example.com/success",
            Amount = 100000,
            PurchaseOrderId = "PO-001",
            PurchaseOrderName = "Test Payment",
            CustomerInfo = new KhaltiCustomerInfo
            {
                Name = "Test User",
                Email = "test@example.com",
                Phone = "9800000000"
            }
        };

        var result = await service.InitiatePaymentAsync(request);

        result.Should().NotBeNull();
        result!.Pidx.Should().Be("pxYZ12345");
        result.PaymentUrl.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InitiatePaymentAsync_WithHttpError_ShouldThrow()
    {
        var config = CreateConfig();
        var logger = CreateLogger();
        var handler = new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("""{"error":"invalid_request"}""")
            }
        );
        using var httpClient = new HttpClient(handler);
        var service = new KhaltiService(httpClient, config, logger);

        var request = new KhaltiInitiateRequest
        {
            ReturnUrl = "https://example.com/success",
            Amount = 100000,
            PurchaseOrderId = "PO-001",
            PurchaseOrderName = "Test"
        };

        var act = () => service.InitiatePaymentAsync(request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Khalti API returned 400*");
    }

    [Fact]
    public async Task LookupPaymentAsync_WithSuccessfulResponse_ShouldReturnResult()
    {
        var config = CreateConfig();
        var logger = CreateLogger();
        var handler = new MockHttpMessageHandler(request =>
        {
            request.RequestUri.Should().NotBeNull();
            request.RequestUri!.AbsolutePath.Should().EndWith("/lookup/");
            request.Headers.Authorization.Should().NotBeNull();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "pidx": "pxYZ12345",
                        "total_amount": 100000,
                        "status": "Completed",
                        "transaction_id": "TX123",
                        "fee": 1000,
                        "refunded": false
                    }
                    """)
            };
        });
        using var httpClient = new HttpClient(handler);
        var service = new KhaltiService(httpClient, config, logger);

        var result = await service.LookupPaymentAsync("pxYZ12345");

        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.TotalAmount.Should().Be(100000);
        result.TransactionId.Should().Be("TX123");
    }

    [Fact]
    public async Task LookupPaymentAsync_WithHttpError_ShouldThrow()
    {
        var config = CreateConfig();
        var logger = CreateLogger();
        var handler = new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("""{"error":"not_found"}""")
            }
        );
        using var httpClient = new HttpClient(handler);
        var service = new KhaltiService(httpClient, config, logger);

        var act = () => service.LookupPaymentAsync("invalid-pidx");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Khalti lookup API returned 404*");
    }

    [Fact]
    public async Task InitiatePaymentAsync_ShouldSendCorrectHeaders()
    {
        var config = CreateConfig();
        var logger = CreateLogger();
        string? authHeader = null;
        var handler = new MockHttpMessageHandler(request =>
        {
            authHeader = request.Headers.Authorization?.ToString();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"pidx":"px1","payment_url":"http://example.com"}""")
            };
        });
        using var httpClient = new HttpClient(handler);
        var service = new KhaltiService(httpClient, config, logger);

        await service.InitiatePaymentAsync(new KhaltiInitiateRequest
        {
            ReturnUrl = "https://example.com/success",
            Amount = 1000,
            PurchaseOrderId = "PO-001",
            PurchaseOrderName = "Test"
        });

        authHeader.Should().Be("Key test-secret-key");
    }

    private sealed class MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(sendAsync(request));
        }
    }
}
