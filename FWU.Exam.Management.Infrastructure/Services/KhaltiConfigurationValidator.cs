using FWU.Exam.Management.Domain.Entities.Payments;

namespace FWU.Exam.Management.Infrastructure.Services;

/// <summary>
/// Lightweight sanity checks for the Khalti gateway configuration stored in
/// <see cref="KhaltiConfiguration"/>. The goal is to catch clear-cut, impossible
/// configurations (a callback/website URL used as the lookup endpoint, a blank
/// or placeholder authorization key) that otherwise surface as a confusing
/// "KPG payment verification failed" instead of "your Khalti config is wrong".
/// The checks are intentionally conservative so valid live/test setups are not
/// rejected.
/// </summary>
public static class KhaltiConfigurationValidator
{
    /// <summary>
    /// Returns a list of human-readable problems. Empty list means the config is usable.
    /// </summary>
    public static List<string> Validate(KhaltiConfiguration config)
    {
        var errors = new List<string>();

        if (config == null)
        {
            errors.Add("Khalti configuration is missing.");
            return errors;
        }

        ValidateVerifyUrl(config, errors);
        ValidatePostUrl(config, errors);
        ValidateAuthorizationKey(config, errors);

        return errors;
    }

    private static void ValidateVerifyUrl(KhaltiConfiguration config, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(config.VerifyUrl))
        {
            errors.Add("Khalti VerifyUrl (lookup endpoint) is empty.");
            return;
        }

        var trimmed = config.VerifyUrl.Trim();
        if (!TryParseAbsoluteHttpUrl(trimmed))
        {
            errors.Add($"Khalti VerifyUrl must be an absolute http(s) URL, got: '{config.VerifyUrl}'.");
            return;
        }

        // The exact misconfiguration we encountered in production: a callback route
        // or website URL was stored in the VerifyUrl field, so lookups hit a page
        // instead of the Khalti lookup API and always failed.
        if (LooksLikeCallbackOrWebsiteUrl(trimmed))
        {
            errors.Add(
                $"Khalti VerifyUrl looks like a callback/website URL, not a Khalti API lookup endpoint: '{config.VerifyUrl}'. " +
                "It should be e.g. 'https://dev.khalti.com/api/v2/epayment/lookup/' (test) or 'https://khalti.com/api/v2/epayment/lookup/' (live).");
        }
    }

    private static void ValidatePostUrl(KhaltiConfiguration config, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(config.PostUrl))
        {
            errors.Add("Khalti PostUrl (initiate endpoint) is empty.");
            return;
        }

        var trimmed = config.PostUrl.Trim();
        if (!TryParseAbsoluteHttpUrl(trimmed))
        {
            errors.Add($"Khalti PostUrl must be an absolute http(s) URL, got: '{config.PostUrl}'.");
        }
    }

    private static void ValidateAuthorizationKey(KhaltiConfiguration config, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(config.AuthorizationKey))
        {
            errors.Add("Khalti AuthorizationKey is empty.");
            return;
        }

        var key = config.AuthorizationKey.Trim();
        // The seeder writes this placeholder when no real key is configured; it can
        // never authenticate a lookup, so flag it explicitly.
        if (string.Equals(key, "test_secret_key", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(key, "live_secret_key", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(
                "Khalti AuthorizationKey is still the placeholder value. Replace it with the real Secret Key " +
                "from test-admin.khalti.com (test) or admin.khalti.com (live).");
        }
    }

    private static bool TryParseAbsoluteHttpUrl(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
               !string.IsNullOrWhiteSpace(uri.Host);
    }

    private static bool LooksLikeCallbackOrWebsiteUrl(string url)
    {
        var lower = url.ToLowerInvariant();

        // Khalti API endpoints contain a path segment; a bare host domain is not an API.
        if (lower.Contains("khalti.com") && !lower.Contains("/api/"))
            return true;

        // Common app callback / portal routes that are definitely not Khalti APIs.
        if (lower.Contains("khalti") && (lower.Contains("callback") || lower.Contains("/payment/")))
            return true;

        if (lower.Contains("/payment/khalti") || lower.Contains("khalti/khalti") ||
            lower.Contains("khalti/payment") || lower.Contains("khalti-callback"))
            return true;

        return false;
    }
}
