using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AbeckDev.Amadeus.Abstractions;
using AbeckDev.Amadeus.Exceptions;

namespace AbeckDev.Amadeus.Pipeline.Policies;

/// <summary>
/// A pipeline policy that handles OAuth 2.0 authentication for HTTP requests.
/// </summary>
/// <remarks>
/// This policy automatically adds authorization headers to requests and handles
/// token refresh when requests fail with 401 Unauthorized responses.
/// </remarks>
public sealed class AuthPolicy : IHttpPipelinePolicy
{
    private readonly ITokenProvider _tokenProvider;
    private readonly string[] _scopes;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthPolicy"/> class.
    /// </summary>
    /// <param name="tokenProvider">The token provider to use for obtaining access tokens.</param>
    /// <param name="scopes">The OAuth scopes to request when obtaining tokens.</param>
    public AuthPolicy(ITokenProvider tokenProvider, string[] scopes)
    {
        _tokenProvider = tokenProvider;
        _scopes = scopes;
    }

    /// <summary>
    /// Processes the HTTP request by adding authentication and handling token refresh on 401 responses.
    /// </summary>
    /// <param name="context">The pipeline context containing request metadata.</param>
    /// <param name="request">The HTTP request message to process.</param>
    /// <param name="next">A delegate to call the next policy in the pipeline chain.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation that yields an HTTP response message.</returns>
    /// <exception cref="ProductAuthenticationException">Thrown when authentication fails after retry.</exception>
    public async Task<HttpResponseMessage> ProcessAsync(PipelineContext context, HttpRequestMessage request, PipelineCall next, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync(new TokenRequestContext(_scopes), cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new("Bearer", token.Token);

        var response = await next(context, request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Attempt a single forced refresh
            response.Dispose();
            var refreshed = await _tokenProvider.GetTokenAsync(new TokenRequestContext(_scopes), cancellationToken).ConfigureAwait(false);
            request.Headers.Remove("Authorization");
            request.Headers.Authorization = new("Bearer", refreshed.Token);

            response = await next(context, request, cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var body = await TryReadBody(response, cancellationToken).ConfigureAwait(false);
                throw new ProductAuthenticationException($"Authentication failed (401). Body: {body}");
            }
        }

        return response;
    }

    private static async Task<string?> TryReadBody(HttpResponseMessage response, CancellationToken ct)
    {
        try { return (await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false))[..Math.Min(200, (await response.Content.ReadAsStringAsync(ct)).Length)]; }
        catch { return null; }
    }
}
