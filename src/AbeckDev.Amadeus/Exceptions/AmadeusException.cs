using System;
using System.Net;
using System.Net.Http;

namespace AbeckDev.Amadeus.Exceptions;

/// <summary>
/// The base exception class for all Amadeus SDK-related exceptions.
/// </summary>
/// <remarks>
/// This serves as the base class for all exceptions thrown by the Amadeus SDK,
/// allowing consumers to catch all SDK-related exceptions with a single catch block.
/// </remarks>
public class AmadeusException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AmadeusException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
    public AmadeusException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>
/// An exception that is thrown when an HTTP request to the Amadeus API fails.
/// </summary>
/// <remarks>
/// This exception provides detailed information about the failed request, including
/// the HTTP status code, response body, and correlation ID for debugging purposes.
/// </remarks>
public sealed class AmadeusRequestException : AmadeusException
{
    /// <summary>
    /// Gets the HTTP status code of the failed request.
    /// </summary>
    /// <value>The HTTP status code returned by the API.</value>
    public HttpStatusCode StatusCode { get; }

    /// <summary>
    /// Gets the response body from the failed request, if available.
    /// </summary>
    /// <value>The response body content, or <c>null</c> if not available or readable.</value>
    /// <remarks>
    /// This may be truncated for very large response bodies to prevent memory issues.
    /// </remarks>
    public string? ResponseBody { get; }

    /// <summary>
    /// Gets the correlation ID from the failed request, if available.
    /// </summary>
    /// <value>The correlation ID header value, or <c>null</c> if not present.</value>
    /// <remarks>
    /// The correlation ID can be used when contacting Amadeus support to help
    /// diagnose issues with specific API requests.
    /// </remarks>
    public string? CorrelationId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AmadeusRequestException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="statusCode">The HTTP status code of the failed request.</param>
    /// <param name="responseBody">The response body from the failed request, if available.</param>
    /// <param name="correlationId">The correlation ID from the failed request, if available.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
    public AmadeusRequestException(
        string message,
        HttpStatusCode statusCode,
        string? responseBody,
        string? correlationId,
        Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        CorrelationId = correlationId;
    }

    /// <summary>
    /// Creates an <see cref="AmadeusRequestException"/> from an HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response message that failed.</param>
    /// <param name="body">The response body content, if available.</param>
    /// <param name="correlationId">The correlation ID from the response headers, if available.</param>
    /// <returns>A new <see cref="AmadeusRequestException"/> with details from the response.</returns>
    public static AmadeusRequestException FromResponse(HttpResponseMessage response, string? body, string? correlationId)
        => new($"Request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).",
            response.StatusCode, body, correlationId);
}

/// <summary>
/// An exception that is thrown when authentication with the Amadeus API fails.
/// </summary>
/// <remarks>
/// This exception is typically thrown by the authentication policy when token
/// acquisition or refresh fails, or when the API returns authentication errors.
/// </remarks>
public sealed class ProductAuthenticationException : AmadeusException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProductAuthenticationException"/> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
    public ProductAuthenticationException(string message, Exception? inner = null) : base(message, inner) { }
}