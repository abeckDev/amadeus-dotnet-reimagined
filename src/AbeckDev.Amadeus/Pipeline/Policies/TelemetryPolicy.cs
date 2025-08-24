using System;

namespace AbeckDev.Amadeus.Pipeline.Policies;

/// <summary>
/// A pipeline policy that adds telemetry headers to HTTP requests.
/// </summary>
/// <remarks>
/// This policy adds User-Agent and request tracking headers to help with API analytics and debugging.
/// The headers include SDK version information and unique request identifiers.
/// </remarks>
public sealed class TelemetryPolicy : IHttpPipelinePolicy
{
    private readonly string _userAgent;

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryPolicy"/> class.
    /// </summary>
    /// <param name="productName">The name of the product using the SDK.</param>
    /// <param name="version">The version of the SDK.</param>
    /// <remarks>
    /// The User-Agent header will be formatted as: "{productName}/{version} (.NET {runtime-version})"
    /// </remarks>
    public TelemetryPolicy(string productName, string version)
    {
        _userAgent = $"{productName}/{version} (.NET {System.Environment.Version})";
    }

    /// <summary>
    /// Processes the HTTP request by adding telemetry headers.
    /// </summary>
    /// <param name="context">The pipeline context containing request metadata.</param>
    /// <param name="request">The HTTP request message to process.</param>
    /// <param name="next">A delegate to call the next policy in the pipeline chain.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that yields an HTTP response message.</returns>
    /// <remarks>
    /// This policy adds the following headers:
    /// - User-Agent: Contains product name, version, and .NET runtime version
    /// - x-sdk-request-id: Contains the unique request identifier for correlation
    /// </remarks>
    public Task<HttpResponseMessage> ProcessAsync(PipelineContext context, HttpRequestMessage request, PipelineCall next, CancellationToken cancellationToken)
    {
        request.Headers.UserAgent.ParseAdd(_userAgent);
        request.Headers.Add("x-sdk-request-id", context.RequestId);
        return next(context, request, cancellationToken);
    }
}
