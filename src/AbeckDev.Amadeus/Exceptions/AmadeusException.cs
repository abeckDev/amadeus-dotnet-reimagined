using System;
using System.Net;
using System.Net.Http;

namespace AbeckDev.Amadeus.Exceptions;

public class AmadeusException : Exception
{
        public AmadeusException(string message, Exception? inner = null) : base(message, inner) { }
}

public sealed class AmadeusRequestException : AmadeusException
{
    public HttpStatusCode StatusCode { get; }
    public string? ResponseBody { get; }
    public string? CorrelationId { get; }

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

    public static AmadeusRequestException FromResponse(HttpResponseMessage response, string? body, string? correlationId)
        => new($"Request failed with status {(int)response.StatusCode} ({response.ReasonPhrase}).",
            response.StatusCode, body, correlationId);
}

public sealed class ProductAuthenticationException : AmadeusException
{
    public ProductAuthenticationException(string message, Exception? inner = null) : base(message, inner) { }
}