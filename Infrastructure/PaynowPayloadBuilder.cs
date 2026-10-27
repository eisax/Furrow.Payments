namespace Furrow.Payments.Infrastructure;

using System.Globalization;
using Furrow.Payments.Models;

/// <summary>
/// Builds ordered HTTP payloads for Paynow API requests conforming to Paynow specifications.
/// </summary>
public static class PaynowPayloadBuilder
{
    /// <summary>
    /// Builds the ordered key-value payload for standard web redirection initiation.
    /// Encodes values (except returnurl and resulturl) before signing with SHA-512.
    /// </summary>
    public static List<KeyValuePair<string, string>> BuildWebInitiatePayload(
        PaynowWebPaymentRequest request,
        PaynowOptions options)
    {
        var resultUrl = !string.IsNullOrWhiteSpace(request.ResultUrl) ? request.ResultUrl! : options.ResultUrl ?? string.Empty;
        var returnUrl = !string.IsNullOrWhiteSpace(request.ReturnUrl) ? request.ReturnUrl! : options.ReturnUrl ?? string.Empty;
        var amountStr = request.Amount.ToString("F2", CultureInfo.InvariantCulture);

        var list = new List<KeyValuePair<string, string>>
        {
            new("resulturl", resultUrl),
            new("returnurl", returnUrl),
            new("reference", PaynowHasher.UrlEncodeValue(request.Reference)),
            new("amount", PaynowHasher.UrlEncodeValue(amountStr)),
            new("id", PaynowHasher.UrlEncodeValue(options.IntegrationId)),
            new("additionalinfo", PaynowHasher.UrlEncodeValue(request.AdditionalInfo ?? string.Empty)),
            new("authemail", PaynowHasher.UrlEncodeValue(request.AuthEmail ?? string.Empty)),
            new("status", PaynowHasher.UrlEncodeValue("Message"))
        };

        var hash = PaynowHasher.GenerateHash(list, options.IntegrationKey);
        list.Add(new KeyValuePair<string, string>("hash", hash));

        return list;
    }

    /// <summary>
    /// Builds the ordered key-value payload for direct mobile remote prompt initiation (EcoCash / OneMoney).
    /// Encodes values (except returnurl, resulturl, and authemail) before signing with SHA-512.
    /// </summary>
    public static List<KeyValuePair<string, string>> BuildMobileInitiatePayload(
        PaynowMobilePaymentRequest request,
        PaynowOptions options)
    {
        var resultUrl = !string.IsNullOrWhiteSpace(request.ResultUrl) ? request.ResultUrl! : options.ResultUrl ?? string.Empty;
        // Fallback to resultUrl if returnUrl not provided (required by Paynow remote API)
        var returnUrl = !string.IsNullOrWhiteSpace(request.ReturnUrl)
            ? request.ReturnUrl!
            : (!string.IsNullOrWhiteSpace(options.ReturnUrl) ? options.ReturnUrl! : resultUrl);

        var amountStr = request.Amount.ToString("F2", CultureInfo.InvariantCulture);
        var methodStr = request.Method.ToApiMethodString();
        var authEmail = !string.IsNullOrWhiteSpace(request.AuthEmail)
            ? request.AuthEmail!
            : (!string.IsNullOrWhiteSpace(options.DefaultAuthEmail) ? options.DefaultAuthEmail! : "billing@havanoerp.com");

        var list = new List<KeyValuePair<string, string>>
        {
            new("resulturl", resultUrl),
            new("returnurl", returnUrl),
            new("reference", PaynowHasher.UrlEncodeValue(request.Reference)),
            new("amount", PaynowHasher.UrlEncodeValue(amountStr)),
            new("id", PaynowHasher.UrlEncodeValue(options.IntegrationId)),
            new("additionalinfo", PaynowHasher.UrlEncodeValue(request.AdditionalInfo ?? string.Empty)),
            new("authemail", authEmail), // Not pre-encoded per Paynow remote specification
            new("phone", PaynowHasher.UrlEncodeValue(request.Phone)),
            new("method", PaynowHasher.UrlEncodeValue(methodStr)),
            new("status", PaynowHasher.UrlEncodeValue("Message"))
        };

        var hash = PaynowHasher.GenerateHash(list, options.IntegrationKey);
        list.Add(new KeyValuePair<string, string>("hash", hash));

        return list;
    }
}
