namespace Furrow.Payments.Exceptions;

/// <summary>
/// Base exception for Paynow payment operations.
/// </summary>
public class PaynowException : Exception
{
    public PaynowException(string message) : base(message) { }
    public PaynowException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Thrown when Paynow's response or webhook SHA-512 signature does not match computed value.
/// </summary>
public sealed class PaynowHashMismatchException : PaynowException
{
    public string ExpectedHash { get; }
    public string ReceivedHash { get; }

    public PaynowHashMismatchException(string expectedHash, string receivedHash)
        : base($"Paynow hash verification failed. Expected: {expectedHash}, Received: {receivedHash}")
    {
        ExpectedHash = expectedHash;
        ReceivedHash = receivedHash;
    }
}

/// <summary>
/// Thrown when an HTTP error occurs while communicating with Paynow.
/// </summary>
public sealed class PaynowHttpException : PaynowException
{
    public int StatusCode { get; }
    public string? ResponseBody { get; }

    public PaynowHttpException(int statusCode, string? responseBody, string message)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }
}
