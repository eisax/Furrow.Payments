namespace Furrow.Payments.Infrastructure;

/// <summary>
/// Parser for application/x-www-form-urlencoded payloads that strictly preserves
/// parameter sequence order. Required for Paynow SHA-512 hash verification.
/// </summary>
public static class PaynowQueryStringParser
{
    /// <summary>
    /// Parses an url-encoded string (query string or POST body) into an ordered list of key-value pairs.
    /// </summary>
    public static List<KeyValuePair<string, string>> ParseOrdered(string? text)
    {
        var list = new List<KeyValuePair<string, string>>();
        if (string.IsNullOrWhiteSpace(text))
            return list;

        // Strip leading '?' if present
        var span = text.AsSpan().Trim();
        if (span.StartsWith("?"))
            span = span.Slice(1);

        var rawString = span.ToString();
        var pairs = rawString.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            var eqIdx = pair.IndexOf('=');
            if (eqIdx < 0)
            {
                var k = Decode(pair);
                list.Add(new KeyValuePair<string, string>(k, string.Empty));
            }
            else
            {
                var k = Decode(pair.Substring(0, eqIdx));
                var v = Decode(pair.Substring(eqIdx + 1));
                list.Add(new KeyValuePair<string, string>(k, v));
            }
        }

        return list;
    }

    /// <summary>
    /// Converts ordered pairs into a case-insensitive lookup dictionary.
    /// Note: if duplicate keys exist, the last occurrence wins.
    /// </summary>
    public static Dictionary<string, string> ToDictionary(IEnumerable<KeyValuePair<string, string>> pairs)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in pairs)
        {
            dict[p.Key] = p.Value;
        }
        return dict;
    }

    private static string Decode(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        // Replace '+' with space first before unescaping, standard for application/x-www-form-urlencoded
        var normalized = value.Replace("+", " ");
        return Uri.UnescapeDataString(normalized);
    }
}
