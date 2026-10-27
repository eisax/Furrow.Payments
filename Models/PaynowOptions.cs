namespace Furrow.Payments.Models;

/// <summary>
/// Configuration options for the Paynow client.
/// </summary>
public sealed class PaynowOptions
{
    /// <summary>
    /// Default initiation endpoint for standard web redirection payments.
    /// </summary>
    public const string DefaultInitiateUrl = "https://www.paynow.co.zw/interface/initiatetransaction";

    /// <summary>
    /// Default initiation endpoint for direct mobile USSD push payments (EcoCash / OneMoney).
    /// </summary>
    public const string DefaultMobileUrl = "https://www.paynow.co.zw/interface/remotetransaction";

    /// <summary>
    /// Your Paynow Integration ID (assigned in the Paynow Merchant portal).
    /// </summary>
    public string IntegrationId { get; set; } = string.Empty;

    /// <summary>
    /// Your Paynow Integration Key / Secret.
    /// </summary>
    public string IntegrationKey { get; set; } = string.Empty;

    /// <summary>
    /// Default Result URL (Webhook / Notification URL) where Paynow posts status updates.
    /// Can be overridden per request.
    /// </summary>
    public string? ResultUrl { get; set; }

    /// <summary>
    /// Default Return URL where user is redirected after standard web checkout.
    /// Can be overridden per request.
    /// </summary>
    public string? ReturnUrl { get; set; }

    /// <summary>
    /// Default customer receipt email used when AuthEmail is omitted on mobile remote requests.
    /// Paynow requires a valid authemail for remote transactions.
    /// </summary>
    public string? DefaultAuthEmail { get; set; } = "billing@havanoerp.com";

    /// <summary>
    /// URL to initiate standard web transactions.
    /// </summary>
    public string InitiateUrl { get; set; } = DefaultInitiateUrl;

    /// <summary>
    /// URL to initiate mobile remote transactions (EcoCash / OneMoney).
    /// </summary>
    public string MobileUrl { get; set; } = DefaultMobileUrl;

    /// <summary>
    /// HTTP request timeout in seconds (defaults to 30s).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Validates that required credentials have been configured.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(IntegrationId))
            throw new InvalidOperationException("Paynow IntegrationId must be configured.");

        if (string.IsNullOrWhiteSpace(IntegrationKey))
            throw new InvalidOperationException("Paynow IntegrationKey must be configured.");
    }
}
