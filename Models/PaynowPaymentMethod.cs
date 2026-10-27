namespace Furrow.Payments.Models;

/// <summary>
/// Supported payment methods for Furrow / Paynow transactions.
/// </summary>
public enum PaynowPaymentMethod
{
    /// <summary>
    /// Standard redirection web checkout (Visa, MasterCard, ZimSwitch, EcoCash Web).
    /// </summary>
    PaynowWeb,

    /// <summary>
    /// Direct mobile prompt push to customer's EcoCash wallet.
    /// </summary>
    EcoCash,

    /// <summary>
    /// Direct mobile prompt push to customer's NetOne OneMoney wallet.
    /// </summary>
    OneMoney,

    /// <summary>
    /// InnBucks mobile payment flow.
    /// </summary>
    InnBucks
}

/// <summary>
/// Extension helpers for <see cref="PaynowPaymentMethod"/>.
/// </summary>
public static class PaynowPaymentMethodExtensions
{
    /// <summary>
    /// Converts the enum to the string identifier expected by Paynow's API.
    /// </summary>
    public static string ToApiMethodString(this PaynowPaymentMethod method) => method switch
    {
        PaynowPaymentMethod.EcoCash => "ecocash",
        PaynowPaymentMethod.OneMoney => "onemoney",
        PaynowPaymentMethod.InnBucks => "innbucks",
        _ => "paynow"
    };

    /// <summary>
    /// Parses an API method string (case-insensitive) into <see cref="PaynowPaymentMethod"/>.
    /// </summary>
    public static PaynowPaymentMethod FromApiMethodString(string? method)
    {
        if (string.IsNullOrWhiteSpace(method))
            return PaynowPaymentMethod.PaynowWeb;

        return method.Trim().ToLowerInvariant() switch
        {
            "ecocash" => PaynowPaymentMethod.EcoCash,
            "onemoney" => PaynowPaymentMethod.OneMoney,
            "innbucks" => PaynowPaymentMethod.InnBucks,
            _ => PaynowPaymentMethod.PaynowWeb
        };
    }
}
