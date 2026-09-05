using FWU.Exam.Management.Infrastructure.Services;
using Xunit;

namespace FWU.Exam.Management.Infrastructure.Tests;

public class KhaltiPaymentStatusTests
{
    [Theory]
    [InlineData("Expired", true)]
    [InlineData("User canceled", true)]
    [InlineData("Canceled", true)]
    [InlineData("Cancelled", true)]
    [InlineData("Failed", true)]
    [InlineData("USER CANCELED", true)]
    [InlineData("expired", true)]
    public void IsTerminalStatus_TerminalStatuses_ReturnsTrue(string status, bool expected)
    {
        Assert.Equal(expected, KhaltiPaymentStatus.IsTerminalStatus(status));
    }

    [Theory]
    [InlineData("Completed")]
    [InlineData("Initiated")]
    [InlineData("Pending")]
    [InlineData("null")]
    [InlineData("")]
    [InlineData(null)]
    public void IsTerminalStatus_NonTerminal_ReturnsFalse(string? status)
    {
        Assert.False(KhaltiPaymentStatus.IsTerminalStatus(status));
    }

    [Fact]
    public void GetVerificationFailureMessage_Expired_MentionsExpiryAndNoCharge()
    {
        var msg = KhaltiPaymentStatus.GetVerificationFailureMessage("Expired");
        Assert.Contains("expired", msg, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no amount was charged", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetVerificationFailureMessage_Canceled_MentionsCanceledAndNoCharge()
    {
        var msg = KhaltiPaymentStatus.GetVerificationFailureMessage("User canceled");
        Assert.Contains("canceled", msg, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no amount was charged", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetVerificationFailureMessage_Pending_MentionsNotCompleted()
    {
        var msg = KhaltiPaymentStatus.GetVerificationFailureMessage("Initiated");
        Assert.Contains("not completed", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetVerificationFailureMessage_Unknown_FallsBackToGeneric()
    {
        var msg = KhaltiPaymentStatus.GetVerificationFailureMessage("SomeRandomStatus");
        Assert.Contains("verification failed", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetVerificationFailureMessage_Null_FallsBackToGeneric()
    {
        Assert.Contains("verification failed", KhaltiPaymentStatus.GetVerificationFailureMessage(null), StringComparison.OrdinalIgnoreCase);
    }
}
