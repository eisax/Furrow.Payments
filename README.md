# Furrow.Payments

A standalone, production-ready C# / .NET library for integrating **Paynow Zimbabwe** payments, featuring:

- **Paynow Web Checkout**: Standard redirection payments (Visa, MasterCard, ZimSwitch, EcoCash Web).
- **EcoCash Direct (USSD Push)**: Instant mobile payment prompts pushed straight to the customer's mobile device with live polling.
- **OneMoney Direct (USSD Push)**: NetOne OneMoney mobile prompt payments.
- **Transaction Status Polling**: Query and track payment statuses in real-time.
- **Webhook Signature Verification**: Tamper-proof SHA-512 verification for Paynow status notifications.
- **Multi-Framework Support**: Targets `.NET 10`, `.NET 8`, and `.NET Standard 2.0`.

---

## Installation

Add the library to your .NET project via NuGet or project reference:

```bash
dotnet add package Furrow.Payments
```

Or reference the project:

```xml
<ProjectReference Include="..\Furrow.Payments\Furrow.Payments.csproj" />
```

---

## Quick Start

### 1. Dependency Injection (ASP.NET Core / Blazor / Worker)

In `Program.cs`:

```csharp
using Furrow.Payments.Extensions;

builder.Services.AddFurrowPayments(options =>
{
    options.IntegrationId = builder.Configuration["Paynow:IntegrationId"] ?? "YOUR_INTEGRATION_ID";
    options.IntegrationKey = builder.Configuration["Paynow:IntegrationKey"] ?? "YOUR_INTEGRATION_KEY";
    options.ResultUrl = "https://yourdomain.com/api/payments/paynow/webhook";
    options.ReturnUrl = "https://yourdomain.com/billing/payment-return";
});
```

Or directly from configuration:

```csharp
builder.Services.AddFurrowPayments(builder.Configuration.GetSection("Paynow"));
```

---

### 2. Standalone / Console / Service Usage (Without DI)

```csharp
using Furrow.Payments;

var client = new PaynowClient(
    integrationId: "12345",
    integrationKey: "a1b2c3d4-e5f6-7890",
    resultUrl: "https://yourdomain.com/webhook",
    returnUrl: "https://yourdomain.com/return"
);
```

---

## Usage Examples

### A. Initiating EcoCash Direct (Mobile USSD Prompt)

```csharp
var result = await client.InitiateEcoCashPaymentAsync(
    reference: "INV-2026-001",
    amount: 15.00m,
    phone: "0771234567",
    authEmail: "customer@example.com",
    additionalInfo: "Subscription Renewal"
);

if (result.Success)
{
    Console.WriteLine($"Mobile prompt sent! Poll URL: {result.PollUrl}");
    Console.WriteLine($"Instructions: {result.Instructions}");
    
    // Periodically poll for confirmation
    var status = await client.PollTransactionStatusAsync(result.PollUrl!);
    if (status.IsPaid)
    {
        Console.WriteLine($"Payment successful! Paynow Ref: {status.PaynowReference}");
    }
}
else
{
    Console.WriteLine($"Failed to send EcoCash prompt: {result.Error}");
}
```

---

### B. Initiating Paynow Web Redirection Checkout

```csharp
var result = await client.InitiateWebPaymentAsync(
    reference: "INV-2026-002",
    amount: 50.00m,
    authEmail: "customer@example.com",
    additionalInfo: "Account Balance Top-Up"
);

if (result.Success)
{
    // Redirect customer to Paynow's hosted checkout page
    Console.WriteLine($"Redirect customer to: {result.BrowserUrl}");
    Console.WriteLine($"Poll URL: {result.PollUrl}");
}
```

---

### C. Polling Transaction Status

```csharp
var status = await client.PollTransactionStatusAsync(pollUrl);

if (status.Success)
{
    if (status.IsPaid)
    {
        // Status is "Paid" or "Awaiting Delivery"
        Console.WriteLine($"Payment completed for Ref: {status.Reference}");
    }
    else if (status.IsPending)
    {
        // Status is "Sent" or "Awaiting Payment"
        Console.WriteLine("Still waiting for customer PIN...");
    }
    else if (status.IsFailed)
    {
        // Status is "Cancelled" or "Failed"
        Console.WriteLine($"Payment failed: {status.RawStatus}");
    }
}
```

---

### D. Verifying Webhook Notifications (ASP.NET Core Minimal API)

```csharp
app.MapPost("/api/payments/paynow/webhook", async (
    HttpRequest request,
    IPaynowClient paynowClient) =>
{
    // Read raw urlencoded body from Paynow
    using var reader = new StreamReader(request.Body);
    var rawBody = await reader.ReadToEndAsync();

    if (!paynowClient.VerifyWebhook(rawBody, out var payload))
    {
        return Results.BadRequest("Invalid Paynow signature");
    }

    // Process verified transaction
    if (payload!.IsPaid)
    {
        // Mark order or invoice as paid in database
        await HandleSuccessfulPaymentAsync(payload.Reference, payload.PaynowReference, payload.Amount);
    }

    return Results.Ok("OK");
});
```

---

## Security

All payloads sent to and received from Paynow are signed using SHA-512 hashes. `Furrow.Payments` strictly enforces dictionary key ordering during hashing and response verification to prevent tampering or replay attacks.

## License
MIT License. Developed by KudahNdhlovu / Furrow.
