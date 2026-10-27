namespace Furrow.Payments.Infrastructure;

using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Cryptographic hashing and verification utilities for Paynow Zimbabwe SHA-512 signatures.
/// </summary>
public static class PaynowHasher
{
    /// <summary>
    /// Computes the SHA-512 hash for a sequence of key-value pairs.
    /// Concatenates values in the provided sequence (excluding the 'hash' key),
    /// appends the integration key, and computes SHA-512 in uppercase hexadecimal.
    /// </summary>
    public static string GenerateHash(IEnumerable<KeyValuePair<string, string>> values, string integrationKey)
    {
        var sb = new StringBuilder();
        foreach (var pair in values)
        {
            if (string.Equals(pair.Key, "hash", StringComparison.OrdinalIgnoreCase))
                continue;

            sb.Append(pair.Value ?? string.Empty);
        }

        sb.Append(integrationKey ?? string.Empty);

        var payloadBytes = Encoding.UTF8.GetBytes(sb.ToString());
#if NETSTANDARD2_0
        using var sha512 = SHA512.Create();
        var hashBytes = sha512.ComputeHash(payloadBytes);
        return BitConverter.ToString(hashBytes).Replace("-", string.Empty).ToUpperInvariant();
#else
        var hashBytes = SHA512.HashData(payloadBytes);
        return Convert.ToHexString(hashBytes).ToUpperInvariant();
#endif
    }

    /// <summary>
    /// Verifies that the provided expected hash matches the computed hash for the given values.
    /// </summary>
    public static bool VerifyHash(IEnumerable<KeyValuePair<string, string>> values, string integrationKey, string? expectedHash)
    {
        if (string.IsNullOrWhiteSpace(expectedHash))
            return false;

        var computed = GenerateHash(values, integrationKey);
        return string.Equals(computed, expectedHash.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// URL-encodes a value adhering to Python's quote_plus behavior (spaces encoded as '+', special characters percent-encoded).
    /// </summary>
    public static string UrlEncodeValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Uri.EscapeDataString(value).Replace("%20", "+");
    }
}
