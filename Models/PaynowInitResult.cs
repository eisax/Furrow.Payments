namespace Furrow.Payments.Models;

/// <summary>
/// Result of initiating a standard Paynow web transaction.
/// </summary>
public sealed class PaynowInitResult
{
    /// <summary>
    /// True if Paynow accepted the initiation request and returned valid URLs.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// URL to which the customer's browser should be redirected to complete payment.
    /// </summary>
    public string? BrowserUrl { get; init; }

    /// <summary>
    /// URL used by the backend to poll for transaction updates.
    /// </summary>
    public string? PollUrl { get; init; }

    /// <summary>
    /// Error message if initiation failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Full key-value dictionary returned by Paynow.
    /// </summary>
    public IReadOnlyDictionary<string, string> RawResponse { get; init; } = new Dictionary<string, string>();

    public static PaynowInitResult Failed(string error, IReadOnlyDictionary<string, string>? raw = null) =>
        new() { Success = false, Error = error, RawResponse = raw ?? new Dictionary<string, string>() };

    public static PaynowInitResult Succeeded(string browserUrl, string pollUrl, IReadOnlyDictionary<string, string> raw) =>
        new() { Success = true, BrowserUrl = browserUrl, PollUrl = pollUrl, RawResponse = raw };
}
