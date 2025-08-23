using System;

namespace AbeckDev.Amadeus.Pipeline;

public sealed class HttpPipeline
{
    private readonly IReadOnlyList<IHttpPipelinePolicy> _policies;
    private readonly HttpMessageHandler _transport;

    public HttpPipeline(IEnumerable<IHttpPipelinePolicy> policies, HttpMessageHandler transport)
    {
        _policies = new List<IHttpPipelinePolicy>(policies);
        _transport = transport;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = new PipelineContext();
        PipelineCall terminal = async (_, req, ct) =>
        {
            var client = new HttpMessageInvoker(_transport, disposeHandler: false);
            return await client.SendAsync(req, ct).ConfigureAwait(false);
        };

        // Build chain in reverse order
        PipelineCall next = terminal;
        for (int i = _policies.Count - 1; i >= 0; i--)
        {
            var policy = _policies[i];
            var localNext = next;
            next = (c, r, ct) => policy.ProcessAsync(c, r, localNext, ct);
        }

        return await next(ctx, request, cancellationToken).ConfigureAwait(false);
    }
}
