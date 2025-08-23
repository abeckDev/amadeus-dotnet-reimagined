using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AbeckDev.Amadeus.Abstractions;
using AbeckDev.Amadeus.Exceptions;

namespace AbeckDev.Amadeus.Pipeline.Policies;

public sealed class AuthPolicy : IHttpPipelinePolicy
{
    private readonly ITokenProvider _tokenProvider;
    private readonly string[] _scopes;

    public AuthPolicy(ITokenProvider tokenProvider, string[] scopes)
    {
        _tokenProvider = tokenProvider;
        _scopes = scopes;
    }

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
