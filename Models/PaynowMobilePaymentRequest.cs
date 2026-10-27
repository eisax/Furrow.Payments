namespace Furrow.Payments.Models;

/// <summary>
/// Parameters for initiating a direct mobile prompt transaction (EcoCash / OneMoney).
/// </summary>
public sealed class PaynowMobilePaymentRequest
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
    /// Customer's mobile money phone number (e.g., "0771234567" or "263771234567").
    /// </summary>
    public required string Phone { get; set; }

    /// <summary>
    /// Mobile money method. Defaults to <see cref="PaynowPaymentMethod.EcoCash"/>.
    /// </summary>
    public PaynowPaymentMethod Method { get; set; } = PaynowPaymentMethod.EcoCash;

    /// <summary>
    /// Customer's email address.
    /// </summary>
    public string? AuthEmail { get; set; }

    /// <summary>
    /// Webhook URL for status notifications. Overrides <see cref="PaynowOptions.ResultUrl"/> if set.
    /// </summary>
    public string? ResultUrl { get; set; }

    /// <summary>
    /// Return URL. Paynow requires this field even on remote calls; defaults to <see cref="ResultUrl"/> if omitted.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Optional additional notes or description.
    /// </summary>
    public string? AdditionalInfo { get; set; }
}
