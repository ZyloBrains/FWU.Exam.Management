namespace FWU.Exam.Management.Application.Interfaces;

public enum NotificationEmailChannel
{
    /// <summary>Sends through SMTP unless an explicit override is supplied.</summary>
    Auto = 0,
    Smtp = 1,
    GumpNow = 2,
}

public record NotificationResult(
    bool EmailSent = false,
    bool SmsSent = false,
    string? EmailError = null,
    string? SmsError = null)
{
    public bool Success => EmailSent && SmsSent;
}

/// <summary>
/// Template-driven notification facade. Given a recipient's email and/or phone and a
/// template code, it renders the matching Email and Sms templates, sends them, and
/// returns per-channel results. Failures are logged and never thrown to the caller.
/// </summary>
public interface INotificationService
{
    Task<NotificationResult> SendAsync(
        string? email,
        string? phone,
        string templateCode,
        IReadOnlyDictionary<string, string> context,
        NotificationEmailChannel emailChannel = NotificationEmailChannel.Auto);
}
