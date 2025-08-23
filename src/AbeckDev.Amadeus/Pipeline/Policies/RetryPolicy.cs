using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AbeckDev.Amadeus.Configuration;

namespace AbeckDev.Amadeus.Pipeline.Policies;

public sealed class RetryPolicy : IHttpPipelinePolicy
{
    private readonly RetryOptions _options;
    private readonly Random _rng = new();

    public RetryPolicy(RetryOptions options) => _options = options;

    public async Task<HttpResponseMessage> ProcessAsync(PipelineContext context, HttpRequestMessage request, PipelineCall next, CancellationToken cancellationToken)
    {
        // Clone logic: HttpRequestMessage is single-use if content is streamed.
        // For simplicity assume either no content or buffered content.
        HttpContent? bufferedContent = null;
        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            bufferedContent = new ByteArrayContent(bytes);
            foreach (var h in request.Content.Headers)
                bufferedContent.Headers.TryAddWithoutValidation(h.Key, string.Join(",", h.Value));
        }

        for (int attempt = 1; attempt <= _options.MaxAttempts; attempt++)
        {
            context.Attempt = attempt;

            var req = CloneRequest(request, bufferedContent);

            HttpResponseMessage? response = null;
            Exception? failure = null;
            try
            {
                response = await next(context, req, cancellationToken).ConfigureAwait(false);

                if (!ShouldRetry(response.StatusCode))
                    return response;
            }
            catch (Exception ex) when (IsTransient(ex))
            {
                failure = ex;
            }

            response?.Dispose();

            if (attempt == _options.MaxAttempts)
            {
                if (failure != null) throw failure;
                // Fall through: final response considered non-transient => unreachable due to earlier return.
                throw new HttpRequestException("Exceeded retry attempts.");
            }

            var delay = ComputeDelay(attempt);
            await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException("Retry loop exited unexpectedly.");
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage original, HttpContent? bufferedContent)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri)
        {
            Version = original.Version
        };

        foreach (var h in original.Headers)
            clone.Headers.TryAddWithoutValidation(h.Key, h.Value);

        if (bufferedContent != null)
            clone.Content = bufferedContent; // Reuse buffered content

        return clone;
    }

    private bool ShouldRetry(HttpStatusCode status)
        => (int)status is 408 or >= 500; // basic set (expand as needed)

    private bool IsTransient(Exception ex)
        => ex is HttpRequestException || (_options.RetryOnTimeouts && ex is TaskCanceledException);

    private TimeSpan ComputeDelay(int attempt)
    {
        var exp = Math.Pow(2, attempt - 1);
        var raw = _options.BaseDelay.TotalMilliseconds * exp;
        var jitter = _rng.NextDouble() * _options.BaseDelay.TotalMilliseconds;
        var total = Math.Min(raw + jitter, _options.MaxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(total);
    }
}
