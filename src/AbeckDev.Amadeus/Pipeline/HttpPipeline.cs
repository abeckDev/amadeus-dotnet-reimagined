using System;

namespace AbeckDev.Amadeus.Pipeline;

/// <summary>
/// Represents an HTTP processing pipeline that applies a series of policies to outgoing requests.
/// </summary>
/// <remarks>
/// The pipeline implements a chain of responsibility pattern where each policy can process
/// the request and response. Policies are executed in the order they were provided to the constructor,
/// with each policy having the opportunity to modify the request before it reaches the transport
/// and modify the response on the way back.
/// </remarks>
public sealed class HttpPipeline
{
    private readonly IReadOnlyList<IHttpPipelinePolicy> _policies;
    private readonly HttpMessageHandler _transport;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpPipeline"/> class.
    /// </summary>
    /// <param name="policies">The sequence of policies to apply to requests, in execution order.</param>
    /// <param name="transport">The HTTP message handler that will send the final request.</param>
    /// <remarks>
    /// Policies are executed in the order provided. For example, if you provide [A, B, C],
    /// the execution order will be: A → B → C → Transport → C → B → A.
    /// </remarks>
    public HttpPipeline(IEnumerable<IHttpPipelinePolicy> policies, HttpMessageHandler transport)
    {
        _policies = new List<IHttpPipelinePolicy>(policies);
        _transport = transport;
    }

    /// <summary>
    /// Sends an HTTP request through the pipeline asynchronously.
    /// </summary>
    /// <param name="request">The HTTP request message to send.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that yields an HTTP response message.</returns>
    /// <remarks>
    /// This method creates a new pipeline context for the request and builds the policy chain
    /// dynamically. Each policy in the chain gets the opportunity to process the request and response.
    /// </remarks>
    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var ctx = new PipelineContext();

        // Define the terminal policy that actually sends the HTTP request
        PipelineCall terminal = async (_, req, ct) =>
        {
            var client = new HttpMessageInvoker(_transport, disposeHandler: false);
            return await client.SendAsync(req, ct).ConfigureAwait(false);
        };

        // Build the policy chain in reverse order so policies execute in the correct sequence
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
