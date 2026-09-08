using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// HMAC-SHA256 signed state token to secure the Khalti callback. The token is
/// {@code base64url("logId|expiresUnixSeconds") "." base64url(hmac)}. The HMAC key is the
/// tenant's Khalti gateway <see cref="Domain.Entities.Payments.KhaltiConfiguration.AuthorizationKey"/>
/// (kept in the database, never in client code or appsettings) so no extra configuration is
/// needed; it is never logged or returned to the client.
/// </summary>
public class PaymentStateTokenService(
    AppDbContext context,
    ILogger<PaymentStateTokenService> logger) : IPaymentStateTokenService
{
    private const int MaxKeyLengthBytes = 64;
    private byte[]? _cachedKey;

    public string Generate(int logId, TimeSpan validFor)
    {
        var key = GetKey();
        var expiresUnix = DateTimeOffset.UtcNow.Add(validFor).ToUnixTimeSeconds();
        var payload = $"{logId}|{expiresUnix}";
        var payloadB64 = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        var signature = Sign(key, payloadB64);
        return $"{payloadB64}.{signature}";
    }

    public int? Verify(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var key = GetKey();
        var parts = token.Split('.');
        if (parts.Length != 2)
        {
            logger.LogWarning("PaymentStateToken: malformed token (expected payload.signature).");
            return null;
        }

        var payloadB64 = parts[0];
        var signature = parts[1];

        byte[] payloadBytes;
        try
        {
            payloadBytes = Base64UrlDecode(payloadB64);
        }
        catch (FormatException)
        {
            logger.LogWarning("PaymentStateToken: payload is not valid base64url.");
            return null;
        }

        var expected = Sign(key, payloadB64);
        if (!FixedTimeEquals(expected, signature))
        {
            logger.LogWarning("PaymentStateToken: signature mismatch (tampered or wrong key).");
            return null;
        }

        var payload = Encoding.UTF8.GetString(payloadBytes);
        var idx = payload.IndexOf('|');
        if (idx <= 0)
        {
            logger.LogWarning("PaymentStateToken: payload missing separator.");
            return null;
        }

        if (!int.TryParse(payload[..idx], NumberStyles.None, CultureInfo.InvariantCulture, out var logId))
        {
            logger.LogWarning("PaymentStateToken: payload logId is not a valid integer.");
            return null;
        }

        if (!long.TryParse(payload[(idx + 1)..], NumberStyles.None, CultureInfo.InvariantCulture, out var expiresUnix))
        {
            logger.LogWarning("PaymentStateToken: payload expiry is not a valid unix timestamp.");
            return null;
        }

        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiresUnix)
        {
            logger.LogInformation("PaymentStateToken: token expired for logId={LogId}.", logId);
            return null;
        }

        return logId;
    }

    private byte[] GetKey()
    {
        if (_cachedKey != null)
            return _cachedKey;

        var config = context.KhaltiConfigurations
            .AsNoTracking()
            .FirstOrDefault();

        var raw = KhaltiAuthorizationKey.Normalize(config?.AuthorizationKey);
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException(
                "Khalti gateway configuration is not set up for this tenant, so the payment callback " +
                "state token cannot be signed. Configure KhaltiConfigurations (Authorization Key) first.");

        var bytes = Encoding.UTF8.GetBytes(raw);
        if (bytes.Length < 32)
        {
            // Pad short dev keys rather than silently failing, but flag it so the
            // production misconfiguration is obvious. Prod should use ≥ 32 bytes.
            logger.LogWarning("Khalti AuthorizationKey is shorter than 32 bytes; the callback state-signing key is weak.");
            _cachedKey = bytes;
            return bytes;
        }
        if (bytes.Length > MaxKeyLengthBytes)
            bytes = bytes[..MaxKeyLengthBytes];

        _cachedKey = bytes;
        return bytes;
    }

    private string Sign(byte[] key, string payloadB64)
    {
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadB64));
        return Base64UrlEncode(hash);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        var aBytes = Encoding.UTF8.GetBytes(a);
        var bBytes = Encoding.UTF8.GetBytes(b);
        return CryptographicOperations.FixedTimeEquals(aBytes, bBytes);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
