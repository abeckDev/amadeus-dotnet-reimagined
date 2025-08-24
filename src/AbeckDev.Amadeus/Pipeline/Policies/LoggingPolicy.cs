using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AbeckDev.Amadeus.Pipeline.Policies;

/// <summary>
/// A pipeline policy that logs HTTP requests and responses with sensitive data redaction.
/// </summary>
/// <remarks>
/// This policy provides structured logging for all HTTP traffic, including request details,
/// response status codes, and timing information. Sensitive headers like Authorization
/// are automatically redacted for security.
/// </remarks>
public sealed class LoggingPolicy : IHttpPipelinePolicy
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingPolicy"/> class.
    /// </summary>
    /// <param name="logger">The logger to use for writing request and response information.</param>
    public LoggingPolicy(ILogger logger) => _logger = logger;

    /// <summary>
    /// Processes the HTTP request by logging request and response details.
    /// </summary>
    /// <param name="context">The pipeline context containing request metadata.</param>
    /// <param name="request">The HTTP request message to process.</param>
    /// <param name="next">A delegate to call the next policy in the pipeline chain.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that yields an HTTP response message.</returns>
    public async Task<HttpResponseMessage> ProcessAsync(PipelineContext context, HttpRequestMessage request, PipelineCall next, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request {RequestId} {Method} {Uri}", context.RequestId, request.Method, request.RequestUri);
        RedactSensitive(request);

        var start = DateTimeOffset.UtcNow;
        var response = await next(context, request, cancellationToken).ConfigureAwait(false);
        var elapsed = DateTimeOffset.UtcNow - start;

        _logger.LogInformation("Response {RequestId} {StatusCode} in {Elapsed}ms",
            context.RequestId, (int)response.StatusCode, elapsed.TotalMilliseconds);

        return response;
    }

    private static void RedactSensitive(HttpRequestMessage request)
    {
        if (request.Headers.Contains("Authorization"))
        {
            request.Headers.Remove("Authorization");
            request.Headers.TryAddWithoutValidation("Authorization", "Bearer ***REDACTED***");
        }
    }
}
