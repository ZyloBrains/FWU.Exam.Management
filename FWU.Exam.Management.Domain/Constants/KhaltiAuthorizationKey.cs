namespace FWU.Exam.Management.Domain.Constants;

/// <summary>
/// Normalizes the Khalti gateway secret key. Admins frequently paste the header-style
/// value shown by Khalti's docs (e.g. "Key live_secret_...") into the AuthorizationKey
/// field; building "Key {value}" from that yields a token that contains spaces and Khalti
/// rejects every request with "Invalid token header. Token string should not contain spaces."
/// </summary>
public static class KhaltiAuthorizationKey
{
    public static string? Normalize(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        var value = key.Trim();

        // Strip a leading "Key " or "Bearer " scheme, which is a paste artifact and never
        // part of the secret itself. Only remove the FIRST occurrence.
        if (value.StartsWith("key ", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("bearer ", StringComparison.OrdinalIgnoreCase))
        {
            value = value[(value.IndexOf(' ') + 1)..].Trim();
        }

        return value;
    }
}