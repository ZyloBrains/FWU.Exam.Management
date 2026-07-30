using System.Net;
using System.Net.Http;
using System.Text.Json;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class ESewaServiceTests
{
    private static IESewaConfigurationService CreateConfigService()
    {
        var configService = Substitute.For<IESewaConfigurationService>();
        var config = new Domain.Entities.Payments.ESewaConfiguration
        {
            SecretKey = "8gBm/:&EnhH.1/q",
            PostUrl = "https://rc-epay.esewa.com.np/api/epay/main/v2/form",
            ProductCode = "EPAYTEST",
            ServiceChargeAmount = 0,
            VerifyUrl = "https://rc-epay.esewa.com.np/api/epay/transaction/status/"
        };
        configService.GetActiveAsync().Returns(config);
        return configService;
    }

    [Fact]
    public void GenerateTransactionUuid_ShouldReturnExpectedFormat()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var uuid = service.GenerateTransactionUuid();

        uuid.Should().MatchRegex(@"^\d{8}-[0-9a-f]{8}$");
    }

    [Fact]
    public void GenerateSignature_ShouldReturnBase64EncodedString()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var signature = service.GenerateSignature("test");

        signature.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(signature);
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateSignature_SameInput_ShouldProduceSameOutput()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);
        var message = "total_amount=1000,transaction_uuid=20240729-abc12345,product_code=EPAYTEST";

        var sig1 = service.GenerateSignature(message);
        var sig2 = service.GenerateSignature(message);

        sig1.Should().Be(sig2);
    }

    [Fact]
    public void GenerateSignature_DifferentInput_ShouldProduceDifferentOutput()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var sig1 = service.GenerateSignature("message1");
        var sig2 = service.GenerateSignature("message2");

        sig1.Should().NotBe(sig2);
    }

    [Fact]
    public void GeneratePaymentFormData_ShouldSetAllExpectedFields()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var result = service.GeneratePaymentFormData(1000m, "TXN-001", "https://example.com/success", "https://example.com/failure");

        result.PostUrl.Should().Be("https://rc-epay.esewa.com.np/api/epay/main/v2/form");
        result.Amount.Should().Be("1000");
        result.TotalAmount.Should().Be("1000");
        result.TaxAmount.Should().Be("0");
        result.TransactionUuid.Should().Be("TXN-001");
        result.ProductCode.Should().Be("EPAYTEST");
        result.ProductServiceCharge.Should().Be("0");
        result.ProductDeliveryCharge.Should().Be("0");
        result.SuccessUrl.Should().Be("https://example.com/success");
        result.FailureUrl.Should().Be("https://example.com/failure");
        result.SignedFieldNames.Should().Be("total_amount,transaction_uuid,product_code");
        result.Signature.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GeneratePaymentFormData_ShouldGenerateCorrectSignature()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var result = service.GeneratePaymentFormData(1000m, "TXN-001", "https://example.com/success", "https://example.com/failure");

        var expectedMessage = "total_amount=1000,transaction_uuid=TXN-001,product_code=EPAYTEST";
        var expectedSignature = service.GenerateSignature(expectedMessage);
        result.Signature.Should().Be(expectedSignature);
    }

    [Fact]
    public void GeneratePaymentFormData_ShouldRoundDecimalAmount()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var result = service.GeneratePaymentFormData(1000.75m, "TXN-001", "https://example.com/success", "https://example.com/failure");

        result.Amount.Should().Be("1001");
        result.TotalAmount.Should().Be("1001");
    }

    [Fact]
    public void VerifyResponseSignature_WithValidData_ShouldReturnTrue()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var totalAmount = 1000;
        var txnUuid = "TXN-001";
        var productCode = "EPAYTEST";
        var signedFieldNames = "total_amount,transaction_uuid,product_code";
        var message = $"total_amount={totalAmount},transaction_uuid={txnUuid},product_code={productCode}";
        var signature = service.GenerateSignature(message);

        var rawJson = $$"""
            {
                "total_amount": {{totalAmount}},
                "transaction_uuid": "{{txnUuid}}",
                "product_code": "{{productCode}}",
                "status": "COMPLETE",
                "transaction_code": "TRX001"
            }
            """;

        var response = new ESewaVerifyResponse
        {
            TotalAmount = totalAmount,
            TransactionUuid = txnUuid,
            ProductCode = productCode,
            Status = "COMPLETE",
            SignedFieldNames = signedFieldNames,
            Signature = signature
        };

        var result = service.VerifyResponseSignature(response, rawJson);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyResponseSignature_WithTamperedSignature_ShouldReturnFalse()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var rawJson = """
            {
                "total_amount": 1000,
                "transaction_uuid": "TXN-001",
                "product_code": "EPAYTEST",
                "status": "COMPLETE"
            }
            """;

        var response = new ESewaVerifyResponse
        {
            TotalAmount = 1000m,
            TransactionUuid = "TXN-001",
            ProductCode = "EPAYTEST",
            Status = "COMPLETE",
            SignedFieldNames = "total_amount,transaction_uuid,product_code",
            Signature = "dHJhbnNwb3NlZC1zaWduYXR1cmU="
        };

        var result = service.VerifyResponseSignature(response, rawJson);

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyResponseSignature_WithNullSignedFieldNames_ShouldReturnFalse()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var response = new ESewaVerifyResponse
        {
            SignedFieldNames = null,
            Signature = "anything"
        };

        var result = service.VerifyResponseSignature(response, "{}");

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyResponseSignature_WithNullSignature_ShouldReturnFalse()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var response = new ESewaVerifyResponse
        {
            SignedFieldNames = "total_amount",
            Signature = null
        };

        var result = service.VerifyResponseSignature(response, "{}");

        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyResponseSignature_WithMissingJsonField_ShouldUseEmptyValue()
    {
        var configService = CreateConfigService();
        using var httpClient = new HttpClient();
        var service = new ESewaService(configService, httpClient);

        var signature = service.GenerateSignature("total_amount=,transaction_uuid=TXN-001,product_code=EPAYTEST");

        var rawJson = """
            {
                "transaction_uuid": "TXN-001",
                "product_code": "EPAYTEST",
                "status": "COMPLETE"
            }
            """;

        var response = new ESewaVerifyResponse
        {
            TotalAmount = 0m,
            TransactionUuid = "TXN-001",
            ProductCode = "EPAYTEST",
            Status = "COMPLETE",
            SignedFieldNames = "total_amount,transaction_uuid,product_code",
            Signature = signature
        };

        var result = service.VerifyResponseSignature(response, rawJson);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyTransactionAsync_InDevelopment_ShouldReturnMockResponse()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        try
        {
            var configService = CreateConfigService();
            using var httpClient = new HttpClient();
            var service = new ESewaService(configService, httpClient);

            var result = await service.VerifyTransactionAsync("TXN-001", 1000m);

            result.Should().NotBeNull();
            result!.TransactionUuid.Should().Be("TXN-001");
            result.TotalAmount.Should().Be(1000m);
            result.Status.Should().Be("COMPLETE");
            result.ProductCode.Should().Be("EPAYTEST");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [Fact]
    public async Task VerifyTransactionAsync_WithSuccessfulResponse_ShouldReturnDeserializedResult()
    {
        var configService = CreateConfigService();
        var handler = new MockHttpMessageHandler(request =>
        {
            request.RequestUri.Should().NotBeNull();
            request.RequestUri!.AbsolutePath.Should().EndWith("/status/");
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""
                    {
                        "status": "COMPLETE",
                        "transaction_uuid": "TXN-001",
                        "total_amount": "1000",
                        "product_code": "EPAYTEST",
                        "transaction_code": "TRX001"
                    }
                    """)
            };
        });
        using var httpClient = new HttpClient(handler);
        var service = new ESewaService(configService, httpClient);

        var result = await service.VerifyTransactionAsync("TXN-001", 1000m);

        result.Should().NotBeNull();
        result!.Status.Should().Be("COMPLETE");
        result.TransactionUuid.Should().Be("TXN-001");
        result.TotalAmount.Should().Be(1000m);
        result.ProductCode.Should().Be("EPAYTEST");
    }

    [Fact]
    public async Task VerifyTransactionAsync_WithHttpError_ShouldReturnNull()
    {
        var configService = CreateConfigService();
        var handler = new MockHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadGateway)
        );
        using var httpClient = new HttpClient(handler);
        var service = new ESewaService(configService, httpClient);

        var result = await service.VerifyTransactionAsync("TXN-001", 1000m);

        result.Should().BeNull();
    }

    [Fact]
    public async Task VerifyTransactionAsync_ShouldBuildCorrectUrl()
    {
        var configService = CreateConfigService();
        string? capturedUrl = null;
        var handler = new MockHttpMessageHandler(request =>
        {
            capturedUrl = request.RequestUri?.ToString();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"status":"COMPLETE"}""")
            };
        });
        using var httpClient = new HttpClient(handler);
        var service = new ESewaService(configService, httpClient);

        await service.VerifyTransactionAsync("TXN-001", 1000m);

        capturedUrl.Should().Be("https://rc-epay.esewa.com.np/api/epay/transaction/status/?product_code=EPAYTEST&total_amount=1000&transaction_uuid=TXN-001");
    }

    private sealed class MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(sendAsync(request));
        }
    }
}
