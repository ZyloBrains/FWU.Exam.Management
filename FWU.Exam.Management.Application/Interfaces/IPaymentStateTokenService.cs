namespace FWU.Exam.Management.Application.Interfaces;

/// <summary>
/// Generates and verifies a short-lived, signed state token embedded in the Khalti
/// return URL. It binds a browser-initiated payment to its local PaymentRequestLog so
/// the callback can (a) resolve the log without trusting the session flag and (b) reject
/// forged/tampered callback URLs. The token is an HMAC-signed payload; the signing key is
/// supplied via configuration (environment variable) and is never exposed to the client.
/// </summary>
public interface IPaymentStateTokenService
{
    /// <summary>
    /// Produces a token that encodes <paramref name="logId"/> and is valid for
    /// <paramref name="validFor"/> from now.
    /// </summary>
    string Generate(int logId, TimeSpan validFor);

    /// <summary>
    /// Verifies a token's signature and expiry, returning the encoded log id.
    /// Returns <c>null</c> when the token is malformed, tampered, or expired.
    /// </summary>
    int? Verify(string? token);
}
