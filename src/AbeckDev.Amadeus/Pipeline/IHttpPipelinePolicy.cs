using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AbeckDev.Amadeus.Pipeline;

/// <summary>
/// Defines a contract for HTTP pipeline policies that can process requests and responses.
/// </summary>
/// <remarks>
/// Pipeline policies form a chain of responsibility pattern where each policy can:
/// - Modify the outgoing request
/// - Call the next policy in the chain
/// - Modify the incoming response
/// - Handle exceptions and retry logic
/// Common use cases include authentication, logging, retry logic, and telemetry.
/// </remarks>
public interface IHttpPipelinePolicy
{
    /// <summary>
    /// Processes an HTTP request through the pipeline.
    /// </summary>
    /// <param name="context">The pipeline context containing request metadata and state.</param>
    /// <param name="request">The HTTP request message to process.</param>
    /// <param name="next">A delegate to call the next policy in the pipeline chain.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that yields an HTTP response message.</returns>
    /// <remarks>
    /// Implementations should:
    /// 1. Optionally modify the request
    /// 2. Call <paramref name="next"/> to continue the pipeline
    /// 3. Optionally modify the response
    /// 4. Handle any exceptions as appropriate
    /// </remarks>
    Task<HttpResponseMessage> ProcessAsync(PipelineContext context, HttpRequestMessage request, PipelineCall next, CancellationToken cancellationToken);
}

/// <summary>
/// Represents a delegate for calling the next policy in the HTTP pipeline chain.
/// </summary>
/// <param name="context">The pipeline context containing request metadata and state.</param>
/// <param name="request">The HTTP request message to process.</param>
/// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
/// <returns>A task representing the asynchronous operation that yields an HTTP response message.</returns>
public delegate Task<HttpResponseMessage> PipelineCall(PipelineContext context, HttpRequestMessage request, CancellationToken cancellationToken);
