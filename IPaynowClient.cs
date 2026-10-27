namespace Furrow.Payments;

using Furrow.Payments.Models;

/// <summary>
/// Client interface for interacting with Paynow Zimbabwe payment APIs.
/// </summary>
public interface IPaynowClient
{
    /// <summary>
    /// Current Paynow configuration options.
    /// </summary>
    PaynowOptions Options { get; }

    /// <summary>
    /// Initiates a standard web redirection payment.
    /// Returns the browser checkout URL where customer completes payment.
    /// </summary>
    Task<PaynowInitResult> InitiateWebPaymentAsync(
        PaynowWebPaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates a direct mobile push payment prompt (EcoCash / OneMoney).
    /// </summary>
    Task<PaynowMobileInitResult> InitiateMobilePaymentAsync(
        PaynowMobilePaymentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience method to initiate an EcoCash mobile USSD payment prompt.
    /// </summary>
    Task<PaynowMobileInitResult> InitiateEcoCashPaymentAsync(
        string reference,
        decimal amount,
        string phone,
        string? authEmail = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience method to initiate a NetOne OneMoney mobile USSD payment prompt.
    /// </summary>
    Task<PaynowMobileInitResult> InitiateOneMoneyPaymentAsync(
        string reference,
        decimal amount,
        string phone,
        string? authEmail = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience method to initiate a standard redirection web payment.
    /// </summary>
    Task<PaynowInitResult> InitiateWebPaymentAsync(
        string reference,
        decimal amount,
        string? authEmail = null,
        string? returnUrl = null,
        string? resultUrl = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls Paynow for the current status of an initiated transaction.
    /// </summary>
    Task<PaynowStatusResult> PollTransactionStatusAsync(
        string pollUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the authenticity of an incoming Paynow webhook notification and extracts the payload.
    /// </summary>
    bool VerifyWebhook(
        string rawFormBody,
        out PaynowWebhookPayload? payload);

    /// <summary>
    /// Verifies the authenticity of incoming form fields and extracts the payload.
    /// </summary>
    bool VerifyWebhook(
        IEnumerable<KeyValuePair<string, string>> formFields,
        out PaynowWebhookPayload? payload);
}
