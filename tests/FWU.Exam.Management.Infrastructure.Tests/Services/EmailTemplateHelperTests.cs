using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class EmailTemplateHelperTests
{
    [Fact]
    public void ConfirmEmail_ShouldContainUserNameAndCallbackUrl()
    {
        var result = EmailTemplateHelper.ConfirmEmail("Ram Sharma", "https://example.com/confirm?token=abc");

        result.Should().Contain("Ram Sharma");
        result.Should().Contain("https://example.com/confirm?token=abc");
        result.Should().Contain("Confirm Email Address");
        result.Should().Contain("Far-Western University");
    }

    [Fact]
    public void ChangeEmail_ShouldContainCallbackUrl()
    {
        var result = EmailTemplateHelper.ChangeEmail("Sita Gurung", "https://example.com/change-email?token=xyz");

        result.Should().Contain("Sita Gurung");
        result.Should().Contain("https://example.com/change-email?token=xyz");
        result.Should().Contain("Confirm Email Change");
    }

    [Fact]
    public void ResetPassword_ShouldContainCallbackUrl()
    {
        var result = EmailTemplateHelper.ResetPassword("Hari KC", "https://example.com/reset?token=123");

        result.Should().Contain("Hari KC");
        result.Should().Contain("https://example.com/reset?token=123");
        result.Should().Contain("Reset Password");
    }

    [Fact]
    public void EntranceApplicationSubmitted_ShouldContainAllDetails()
    {
        var result = EmailTemplateHelper.EntranceApplicationSubmitted(
            "Ram Sharma", "FWU College", "BCA", 1001, "2081/01/15");

        result.Should().Contain("Ram Sharma");
        result.Should().Contain("FWU College");
        result.Should().Contain("BCA");
        result.Should().Contain("1001");
        result.Should().Contain("2081/01/15");
        result.Should().Contain("Application Submitted Successfully");
    }

    [Fact]
    public void StudentRegistrationCredentials_ShouldContainCredentials()
    {
        var result = EmailTemplateHelper.StudentRegistrationCredentials(
            "Ram Sharma", "REG-001", "FWU College", "BCA", "ram@test.com", "TempPass123!");

        result.Should().Contain("Ram Sharma");
        result.Should().Contain("REG-001");
        result.Should().Contain("FWU College");
        result.Should().Contain("BCA");
        result.Should().Contain("ram@test.com");
        result.Should().Contain("TempPass123!");
        result.Should().Contain("Registration Successful");
    }

    [Fact]
    public void TenantAccountCreated_ShouldContainTenantDetails()
    {
        var result = EmailTemplateHelper.TenantAccountCreated(
            "Admin User", "Far Western University", "FWU001", "admin@fwu.edu.np");

        result.Should().Contain("Admin User");
        result.Should().Contain("Far Western University");
        result.Should().Contain("FWU001");
        result.Should().Contain("admin@fwu.edu.np");
        result.Should().Contain("Tenant Account Created");
    }

    [Fact]
    public void LogoUrl_WhenSet_ShouldRenderImageTag()
    {
        var original = EmailTemplateHelper.LogoUrl;
        try
        {
            EmailTemplateHelper.LogoUrl = "https://example.com/logo.png";

            var result = EmailTemplateHelper.ConfirmEmail("Test", "https://example.com/cb");

            result.Should().Contain("src=");
        }
        finally
        {
            EmailTemplateHelper.LogoUrl = original;
        }
    }
}
