namespace Furrow.Payments.Models;

/// <summary>
/// Status result returned by polling Paynow or parsing status callbacks.
/// </summary>
public sealed class PaynowStatusResult
{
    /// <summary>True if poll call completed and signature verified successfully.</summary>
    public bool Success { get; init; }

    /// <summary>Strongly typed status enum.</summary>
    public PaynowPaymentStatus Status { get; init; }

    /// <summary>Raw status string from Paynow (e.g. "Paid", "Awaiting Delivery", "Sent").</summary>
    public string RawStatus { get; init; } = string.Empty;

    /// <summary>Merchant reference.</summary>
    public string? Reference { get; init; }

    /// <summary>Paynow reference number.</summary>
    public string? PaynowReference { get; init; }

    /// <summary>Amount paid or billed.</summary>
    public decimal Amount { get; init; }

    /// <summary>Poll URL for subsequent polls if still pending.</summary>
    public string? PollUrl { get; init; }

    /// <summary>True if transaction is Paid or Awaiting Delivery.</summary>
    public bool IsPaid => Status.IsPaid();

    /// <summary>True if transaction is still pending customer approval.</summary>
    public bool IsPending => Status.IsPending();

    /// <summary>True if cancelled or failed.</summary>
    public bool IsFailed => Status.IsFailed();

    /// <summary>Error message if status query or verification failed.</summary>
    public string? Error { get; init; }

    /// <summary>Raw response dictionary.</summary>
    public IReadOnlyDictionary<string, string> RawResponse { get; init; } = new Dictionary<string, string>();

    public static PaynowStatusResult Failed(string error, IReadOnlyDictionary<string, string>? raw = null) =>
        new()
        {
            Success = false,
            Status = PaynowPaymentStatus.Unknown,
            Error = error,
            RawResponse = raw ?? new Dictionary<string, string>()
        };

    public static PaynowStatusResult Succeeded(
        string rawStatus,
        string? reference,
        string? paynowReference,
        decimal amount,
        string? pollUrl,
        IReadOnlyDictionary<string, string> raw) =>
        new()
        {
            Success = true,
            RawStatus = rawStatus,
            Status = PaynowPaymentStatusExtensions.ParsePaynowStatus(rawStatus),
            Reference = reference,
            PaynowReference = paynowReference,
            Amount = amount,
            PollUrl = pollUrl,
            RawResponse = raw
        };
}
