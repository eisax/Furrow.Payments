namespace Furrow.Payments.Models;

/// <summary>
/// Status of a Paynow transaction.
/// </summary>
public enum PaynowPaymentStatus
{
    /// <summary>Transaction created but not yet initiated.</summary>
    Created,

    /// <summary>Mobile prompt sent to user's phone, awaiting payment approval.</summary>
    Sent,

    /// <summary>Awaiting customer payment authorization.</summary>
    AwaitingPayment,

    /// <summary>Payment made and confirmed, awaiting merchant fulfillment/delivery.</summary>
    AwaitingDelivery,

    /// <summary>Transaction successfully paid and completed.</summary>
    Paid,

    /// <summary>Goods or service delivered.</summary>
    Delivered,

    /// <summary>Transaction was cancelled by user or expired.</summary>
    Cancelled,

    /// <summary>Transaction failed.</summary>
    Failed,

    /// <summary>Transaction disputed by customer.</summary>
    Disputed,

    /// <summary>Transaction has been refunded.</summary>
    Refunded,

    /// <summary>Unrecognized or custom status.</summary>
    Unknown
}

/// <summary>
/// Extension methods for <see cref="PaynowPaymentStatus"/>.
/// </summary>
public static class PaynowPaymentStatusExtensions
{
    /// <summary>
    /// Parses the raw string status returned by Paynow (e.g., "Paid", "Awaiting Delivery", "Sent").
    /// </summary>
    public static PaynowPaymentStatus ParsePaynowStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return PaynowPaymentStatus.Unknown;

        return status.Trim().ToLowerInvariant() switch
        {
            "paid" => PaynowPaymentStatus.Paid,
            "awaiting delivery" => PaynowPaymentStatus.AwaitingDelivery,
            "delivered" => PaynowPaymentStatus.Delivered,
            "sent" => PaynowPaymentStatus.Sent,
            "awaiting payment" => PaynowPaymentStatus.AwaitingPayment,
            "pending" => PaynowPaymentStatus.AwaitingPayment,
            "created" => PaynowPaymentStatus.Created,
            "cancelled" or "canceled" => PaynowPaymentStatus.Cancelled,
            "failed" => PaynowPaymentStatus.Failed,
            "disputed" => PaynowPaymentStatus.Disputed,
            "refunded" => PaynowPaymentStatus.Refunded,
            _ => PaynowPaymentStatus.Unknown
        };
    }

    /// <summary>
    /// Returns true if the status indicates funds have been collected (Paid or Awaiting Delivery).
    /// </summary>
    public static bool IsPaid(this PaynowPaymentStatus status) =>
        status is PaynowPaymentStatus.Paid or PaynowPaymentStatus.AwaitingDelivery or PaynowPaymentStatus.Delivered;

    /// <summary>
    /// Returns true if the transaction is pending customer PIN authorization or checkout.
    /// </summary>
    public static bool IsPending(this PaynowPaymentStatus status) =>
        status is PaynowPaymentStatus.Sent or PaynowPaymentStatus.AwaitingPayment or PaynowPaymentStatus.Created;

    /// <summary>
    /// Returns true if the transaction has reached an unsuccessful terminal state (Cancelled or Failed).
    /// </summary>
    public static bool IsFailed(this PaynowPaymentStatus status) =>
        status is PaynowPaymentStatus.Cancelled or PaynowPaymentStatus.Failed;

    /// <summary>
    /// Returns true if the transaction has completed (Paid, Delivered, Cancelled, Failed, Refunded).
    /// </summary>
    public static bool IsTerminal(this PaynowPaymentStatus status) =>
        status.IsPaid() || status.IsFailed() || status == PaynowPaymentStatus.Refunded;
}
