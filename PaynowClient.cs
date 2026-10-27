namespace Furrow.Payments;

using System.Globalization;
using Furrow.Payments.Infrastructure;
using Furrow.Payments.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

/// <summary>
/// Production-grade implementation of <see cref="IPaynowClient"/> for Paynow Zimbabwe.
/// Supports Paynow Web, EcoCash &amp; OneMoney direct USSD prompts, status polling, and webhook verification.
/// </summary>
public sealed class PaynowClient : IPaynowClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly ILogger<PaynowClient> _logger;
    private readonly PaynowOptions _options;

    public PaynowOptions Options => _options;

    /// <summary>
    /// Dependency Injection constructor.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public PaynowClient(
        HttpClient httpClient,
        IOptions<PaynowOptions> options,
        ILogger<PaynowClient>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<PaynowClient>.Instance;
        _ownsHttpClient = false;

        ConfigureHttpClient(_httpClient, _options);
    }

    /// <summary>
    /// Standalone constructor accepting explicit options.
    /// </summary>
    public PaynowClient(PaynowOptions options, HttpClient? httpClient = null, ILogger<PaynowClient>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<PaynowClient>.Instance;

        if (httpClient is null)
        {
            _httpClient = new HttpClient();
            _ownsHttpClient = true;
        }
        else
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }

        ConfigureHttpClient(_httpClient, _options);
    }

    /// <summary>
    /// Standalone constructor with minimal required credentials.
    /// </summary>
    public PaynowClient(
        string integrationId,
        string integrationKey,
        string? resultUrl = null,
        string? returnUrl = null,
        HttpClient? httpClient = null,
        ILogger<PaynowClient>? logger = null)
        : this(new PaynowOptions
        {
            IntegrationId = integrationId,
            IntegrationKey = integrationKey,
            ResultUrl = resultUrl,
            ReturnUrl = returnUrl
        }, httpClient, logger)
    {
    }

    private static void ConfigureHttpClient(HttpClient client, PaynowOptions options)
    {
        if (options.TimeoutSeconds > 0 && client.Timeout != TimeSpan.FromSeconds(options.TimeoutSeconds))
        {
            try
            {
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            }
            catch (InvalidOperationException)
            {
                // Timeout can only be set before the first request
            }
        }
    }

    /// <inheritdoc/>
    public async Task<PaynowInitResult> InitiateWebPaymentAsync(
        PaynowWebPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _options.Validate();

        var payload = PaynowPayloadBuilder.BuildWebInitiatePayload(request, _options);
        _logger.LogInformation("Initiating Paynow Web transaction for Ref: {Reference}, Amount: {Amount}",
            request.Reference, request.Amount);

        try
        {
            using var content = new FormUrlEncodedContent(payload);
            using var response = await _httpClient.PostAsync(_options.InitiateUrl, content, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            var pairs = PaynowQueryStringParser.ParseOrdered(responseString);
            var dict = PaynowQueryStringParser.ToDictionary(pairs);

            if (dict.TryGetValue("status", out var status) &&
                string.Equals(status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                var err = dict.TryGetValue("error", out var errorMsg) ? errorMsg : "Unknown Paynow initiation error";
                _logger.LogWarning("Paynow Web initiation returned error: {Error}", err);
                return PaynowInitResult.Failed(err, dict);
            }

            if (!dict.TryGetValue("hash", out var receivedHash) ||
                !PaynowHasher.VerifyHash(pairs, _options.IntegrationKey, receivedHash))
            {
                _logger.LogError("Paynow response hash verification failed for Reference: {Reference}", request.Reference);
                return PaynowInitResult.Failed("Response hash verification failed", dict);
            }

            dict.TryGetValue("browserurl", out var browserUrl);
            dict.TryGetValue("pollurl", out var pollUrl);

            _logger.LogInformation("Paynow Web initiation successful. PollUrl: {PollUrl}", pollUrl);
            return PaynowInitResult.Succeeded(browserUrl ?? string.Empty, pollUrl ?? string.Empty, dict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate Paynow Web payment for Reference: {Reference}", request.Reference);
            return PaynowInitResult.Failed(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<PaynowMobileInitResult> InitiateMobilePaymentAsync(
        PaynowMobilePaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        _options.Validate();

        var payload = PaynowPayloadBuilder.BuildMobileInitiatePayload(request, _options);
        _logger.LogInformation("Initiating Paynow Mobile prompt ({Method}) for Ref: {Reference}, Phone: {Phone}, Amount: {Amount}",
            request.Method, request.Reference, request.Phone, request.Amount);

        try
        {
            using var content = new FormUrlEncodedContent(payload);
            using var response = await _httpClient.PostAsync(_options.MobileUrl, content, cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            var pairs = PaynowQueryStringParser.ParseOrdered(responseString);
            var dict = PaynowQueryStringParser.ToDictionary(pairs);

            if (dict.TryGetValue("status", out var status) &&
                string.Equals(status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                var err = dict.TryGetValue("error", out var errorMsg) ? errorMsg : "Unknown Paynow mobile error";
                _logger.LogWarning("Paynow Mobile initiation returned error: {Error}", err);
                return PaynowMobileInitResult.Failed(err, dict);
            }

            if (!dict.TryGetValue("hash", out var receivedHash) ||
                !PaynowHasher.VerifyHash(pairs, _options.IntegrationKey, receivedHash))
            {
                _logger.LogError("Paynow mobile response hash verification failed for Reference: {Reference}", request.Reference);
                return PaynowMobileInitResult.Failed("Response hash verification failed", dict);
            }

            dict.TryGetValue("pollurl", out var pollUrl);
            dict.TryGetValue("instructions", out var instructions);

            _logger.LogInformation("Paynow Mobile initiation successful. PollUrl: {PollUrl}", pollUrl);
            return PaynowMobileInitResult.Succeeded(pollUrl ?? string.Empty, instructions, dict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initiate Paynow Mobile payment for Reference: {Reference}", request.Reference);
            return PaynowMobileInitResult.Failed(ex.Message);
        }
    }

    /// <inheritdoc/>
    public Task<PaynowMobileInitResult> InitiateEcoCashPaymentAsync(
        string reference,
        decimal amount,
        string phone,
        string? authEmail = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default)
    {
        return InitiateMobilePaymentAsync(new PaynowMobilePaymentRequest
        {
            Reference = reference,
            Amount = amount,
            Phone = phone,
            Method = PaynowPaymentMethod.EcoCash,
            AuthEmail = authEmail,
            AdditionalInfo = additionalInfo
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<PaynowMobileInitResult> InitiateOneMoneyPaymentAsync(
        string reference,
        decimal amount,
        string phone,
        string? authEmail = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default)
    {
        return InitiateMobilePaymentAsync(new PaynowMobilePaymentRequest
        {
            Reference = reference,
            Amount = amount,
            Phone = phone,
            Method = PaynowPaymentMethod.OneMoney,
            AuthEmail = authEmail,
            AdditionalInfo = additionalInfo
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<PaynowInitResult> InitiateWebPaymentAsync(
        string reference,
        decimal amount,
        string? authEmail = null,
        string? returnUrl = null,
        string? resultUrl = null,
        string? additionalInfo = null,
        CancellationToken cancellationToken = default)
    {
        return InitiateWebPaymentAsync(new PaynowWebPaymentRequest
        {
            Reference = reference,
            Amount = amount,
            AuthEmail = authEmail,
            ReturnUrl = returnUrl,
            ResultUrl = resultUrl,
            AdditionalInfo = additionalInfo
        }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<PaynowStatusResult> PollTransactionStatusAsync(
        string pollUrl,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pollUrl))
            throw new ArgumentException("PollUrl cannot be empty.", nameof(pollUrl));

        try
        {
            using var response = await _httpClient.GetAsync(pollUrl, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var pairs = PaynowQueryStringParser.ParseOrdered(responseString);
            var dict = PaynowQueryStringParser.ToDictionary(pairs);

            if (dict.TryGetValue("status", out var status) &&
                string.Equals(status, "Error", StringComparison.OrdinalIgnoreCase))
            {
                var err = dict.TryGetValue("error", out var errorMsg) ? errorMsg : "Unknown Paynow status error";
                return PaynowStatusResult.Failed(err, dict);
            }

            if (!dict.TryGetValue("hash", out var receivedHash) ||
                !PaynowHasher.VerifyHash(pairs, _options.IntegrationKey, receivedHash))
            {
                _logger.LogWarning("Paynow poll status hash verification failed for PollUrl: {PollUrl}", pollUrl);
                return PaynowStatusResult.Failed("Response hash verification failed", dict);
            }

            var rawStatus = dict.TryGetValue("status", out var s) ? s : string.Empty;
            dict.TryGetValue("reference", out var reference);
            dict.TryGetValue("paynowreference", out var paynowReference);

            decimal amount = 0;
            if (dict.TryGetValue("amount", out var amountStr))
            {
                decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);
            }

            dict.TryGetValue("pollurl", out var respPollUrl);

            return PaynowStatusResult.Succeeded(
                rawStatus,
                reference,
                paynowReference,
                amount,
                respPollUrl ?? pollUrl,
                dict);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to poll Paynow status from URL: {PollUrl}", pollUrl);
            return PaynowStatusResult.Failed(ex.Message);
        }
    }

    /// <inheritdoc/>
    public bool VerifyWebhook(
        string rawFormBody,
        out PaynowWebhookPayload? payload)
    {
        payload = null;
        if (string.IsNullOrWhiteSpace(rawFormBody))
            return false;

        var pairs = PaynowQueryStringParser.ParseOrdered(rawFormBody);
        return VerifyWebhook(pairs, out payload);
    }

    /// <inheritdoc/>
    public bool VerifyWebhook(
        IEnumerable<KeyValuePair<string, string>> formFields,
        out PaynowWebhookPayload? payload)
    {
        payload = null;
        var pairs = formFields as List<KeyValuePair<string, string>> ?? formFields.ToList();
        if (pairs.Count == 0)
            return false;

        var dict = PaynowQueryStringParser.ToDictionary(pairs);
        if (!dict.TryGetValue("hash", out var receivedHash))
            return false;

        if (!PaynowHasher.VerifyHash(pairs, _options.IntegrationKey, receivedHash))
        {
            _logger.LogWarning("Webhook hash verification failed. PaynowReference: {PaynowReference}",
                dict.GetValueOrDefault("paynowreference"));
            return false;
        }

        var rawStatus = dict.GetValueOrDefault("status", string.Empty);
        var reference = dict.GetValueOrDefault("reference", string.Empty);
        var paynowRef = dict.GetValueOrDefault("paynowreference", string.Empty);
        var pollUrl = dict.GetValueOrDefault("pollurl", string.Empty);

        decimal amount = 0;
        if (dict.TryGetValue("amount", out var amountStr))
        {
            decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);
        }

        payload = new PaynowWebhookPayload
        {
            Reference = reference,
            PaynowReference = paynowRef,
            Amount = amount,
            RawStatus = rawStatus,
            Status = PaynowPaymentStatusExtensions.ParsePaynowStatus(rawStatus),
            PollUrl = pollUrl,
            Hash = receivedHash,
            RawValues = dict
        };

        return true;
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
