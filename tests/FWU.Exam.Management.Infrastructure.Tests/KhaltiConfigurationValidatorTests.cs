using FWU.Exam.Management.Domain.Entities.Payments;
using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class KhaltiConfigurationValidatorTests
{
    private static KhaltiConfiguration ValidConfig() => new()
    {
        ReturnUrl = "https://exam.example/Students/StudentDashboard/KhaltiCallback",
        WebsiteUrl = "https://exam.example",
        ProductName = "Exam Fee",
        AuthorizationKey = "test_secret_key_9e0f8a7b6c5d",
        PostUrl = "https://dev.khalti.com/api/v2/epayment/initiate/",
        VerifyUrl = "https://dev.khalti.com/api/v2/epayment/lookup/",
    };

    [Fact]
    public void Validate_ValidConfig_ReturnsNoErrors()
    {
        var errors = KhaltiConfigurationValidator.Validate(ValidConfig());
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullConfig_ReturnsMissingError()
    {
        var errors = KhaltiConfigurationValidator.Validate(null!);
        Assert.Contains(errors, e => e.Contains("missing", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_CallbackUrlAsVerifyUrl_IsFlagged()
    {
        var config = ValidConfig();
        config.VerifyUrl = "https://localhost:44333/Payment/KhaltiCallback";

        var errors = KhaltiConfigurationValidator.Validate(config);
        Assert.Contains(errors, e => e.Contains("VerifyUrl"));
    }

    [Fact]
    public void Validate_NonApiKhaltiDomainAsVerifyUrl_IsFlagged()
    {
        var config = ValidConfig();
        config.VerifyUrl = "https://dev.khalti.com/";

        var errors = KhaltiConfigurationValidator.Validate(config);
        Assert.Contains(errors, e => e.Contains("VerifyUrl"));
    }

    [Fact]
    public void Validate_ValidVerifyUrlApi_NotFlagged()
    {
        var config = ValidConfig();
        config.VerifyUrl = "https://khalti.com/api/v2/epayment/lookup/";

        var errors = KhaltiConfigurationValidator.Validate(config);
        Assert.DoesNotContain(errors, e => e.Contains("VerifyUrl"));
    }

    [Fact]
    public void Validate_PlaceholderAuthorizationKey_IsFlagged()
    {
        var config = ValidConfig();
        config.AuthorizationKey = "test_secret_key";

        var errors = KhaltiConfigurationValidator.Validate(config);
        Assert.Contains(errors, e => e.Contains("AuthorizationKey"));
    }

    [Fact]
    public void Validate_EmptyAuthorizationKey_IsFlagged()
    {
        var config = ValidConfig();
        config.AuthorizationKey = "";

        var errors = KhaltiConfigurationValidator.Validate(config);
        Assert.Contains(errors, e => e.Contains("AuthorizationKey"));
    }

    [Fact]
    public void Validate_EmptyVerifyUrl_IsFlagged()
    {
        var config = ValidConfig();
        config.VerifyUrl = "";

        var errors = KhaltiConfigurationValidator.Validate(config);
        Assert.Contains(errors, e => e.Contains("VerifyUrl"));
    }

    [Fact]
    public void Validate_RelativeVerifyUrl_IsFlagged()
    {
        var config = ValidConfig();
        config.VerifyUrl = "/api/v2/epayment/lookup/";

        var errors = KhaltiConfigurationValidator.Validate(config);
        Assert.Contains(errors, e => e.Contains("VerifyUrl"));
    }
}
