namespace Furrow.Payments.Models;

/// <summary>
/// Parameters for initiating a standard redirection web payment.
/// </summary>
public sealed class PaynowWebPaymentRequest
{
    /// <summary>
    /// Merchant transaction reference (e.g. "INV-10042", "SUB-20261026-901").
    /// </summary>
    public required string Reference { get; set; }

    /// <summary>
    /// Amount to pay.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Customer's email address (for Paynow payment receipts and verification).
    /// </summary>
    public string? AuthEmail { get; set; }

    /// <summary>
    /// Return URL after web checkout. Overrides <see cref="PaynowOptions.ReturnUrl"/> if set.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Result / Webhook URL for status notifications. Overrides <see cref="PaynowOptions.ResultUrl"/> if set.
    /// </summary>
    public string? ResultUrl { get; set; }

    /// <summary>
    /// Optional additional notes or order description.
    /// </summary>
    public string? AdditionalInfo { get; set; }
}
