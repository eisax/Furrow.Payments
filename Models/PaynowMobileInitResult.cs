namespace Furrow.Payments.Models;

/// <summary>
/// Result of initiating a mobile payment prompt (EcoCash / OneMoney).
/// </summary>
public sealed class PaynowMobileInitResult
{
    /// <summary>
    /// True if Paynow dispatched the USSD prompt to customer's mobile phone.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Poll URL to check if the user entered their PIN and approved payment.
    /// </summary>
    public string? PollUrl { get; init; }

    /// <summary>
    /// User instructions returned by Paynow (e.g. "Please check your phone and enter PIN").
    /// </summary>
    public string? Instructions { get; init; }

    /// <summary>
    /// Error message if initiation failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Full key-value dictionary returned by Paynow.
    /// </summary>
    public IReadOnlyDictionary<string, string> RawResponse { get; init; } = new Dictionary<string, string>();

    public static PaynowMobileInitResult Failed(string error, IReadOnlyDictionary<string, string>? raw = null) =>
        new() { Success = false, Error = error, RawResponse = raw ?? new Dictionary<string, string>() };

    public static PaynowMobileInitResult Succeeded(string pollUrl, string? instructions, IReadOnlyDictionary<string, string> raw) =>
        new() { Success = true, PollUrl = pollUrl, Instructions = instructions, RawResponse = raw };
}
