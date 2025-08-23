using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AbeckDev.Amadeus.Pipeline.Policies;

public sealed class LoggingPolicy : IHttpPipelinePolicy
{
    private readonly ILogger _logger;

    public LoggingPolicy(ILogger logger) => _logger = logger;

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
