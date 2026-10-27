namespace Furrow.Payments.Models;

/// <summary>
/// Verified payload received from Paynow result/webhook POST notification.
/// </summary>
public sealed class PaynowWebhookPayload
{
    /// <summary>Merchant reference provided during initiation.</summary>
    public string Reference { get; init; } = string.Empty;

    /// <summary>Paynow's internal transaction reference.</summary>
    public string PaynowReference { get; init; } = string.Empty;

    /// <summary>Transaction amount parsed from payload.</summary>
    public decimal Amount { get; init; }

    /// <summary>Status of the transaction (e.g. Paid, Cancelled, Failed).</summary>
    public PaynowPaymentStatus Status { get; init; }

    /// <summary>Original raw status string from Paynow.</summary>
    public string RawStatus { get; init; } = string.Empty;

    /// <summary>Poll URL for querying subsequent updates if needed.</summary>
    public string PollUrl { get; init; } = string.Empty;

    /// <summary>The SHA-512 signature transmitted by Paynow.</summary>
    public string Hash { get; init; } = string.Empty;

    /// <summary>True if transaction is Paid or Awaiting Delivery.</summary>
    public bool IsPaid => Status.IsPaid();

    /// <summary>All raw fields in their received order.</summary>
    public IReadOnlyDictionary<string, string> RawValues { get; init; } = new Dictionary<string, string>();
}
