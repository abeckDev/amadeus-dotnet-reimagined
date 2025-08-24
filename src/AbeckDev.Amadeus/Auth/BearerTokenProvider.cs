using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AbeckDev.Amadeus.Abstractions;

namespace AbeckDev.Amadeus.Auth;

/// <summary>
/// A token provider implementation for OAuth 2.0 client credentials flow with token caching.
/// </summary>
/// <remarks>
/// This implementation handles automatic token acquisition, caching, and refresh for the Amadeus API.
/// It uses the OAuth 2.0 client credentials grant type and includes thread-safe token caching
/// to avoid unnecessary token requests.
/// </remarks>
public sealed class BearerTokenProvider : ITokenProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _tokenEndpoint;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string[] _defaultScopes;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private AccessToken _cachedToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="BearerTokenProvider"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for token requests. The provider does not dispose this client.</param>
    /// <param name="tokenEndpoint">The OAuth 2.0 token endpoint URL.</param>
    /// <param name="clientId">The OAuth 2.0 client identifier.</param>
    /// <param name="clientSecret">The OAuth 2.0 client secret.</param>
    /// <param name="defaultScopes">Optional default scopes to request when no specific scopes are provided.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is <c>null</c>.</exception>
    public BearerTokenProvider(
        HttpClient httpClient,
        string tokenEndpoint,
        string clientId,
        string clientSecret,
        string[]? defaultScopes = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tokenEndpoint = tokenEndpoint ?? throw new ArgumentNullException(nameof(tokenEndpoint));
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        _defaultScopes = defaultScopes ?? Array.Empty<string>();
    }

    /// <summary>
    /// Asynchronously retrieves an access token, using cached tokens when available and valid.
    /// </summary>
    /// <param name="context">The token request context containing the requested scopes.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A <see cref="ValueTask{AccessToken}"/> representing the asynchronous operation that yields an access token.</returns>
    /// <remarks>
    /// This method implements thread-safe token caching with automatic refresh. If a cached token
    /// exists and is not expired, it will be returned immediately. Otherwise, a new token will be
    /// requested using the OAuth 2.0 client credentials flow.
    /// </remarks>
    public async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext context, CancellationToken cancellationToken = default)
    {
        // Fast path if token is still present and not expired
        if (!_cachedToken.Token.IsNullOrEmpty() && !_cachedToken.IsExpired())
            return _cachedToken;

        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_cachedToken.Token.IsNullOrEmpty() && !_cachedToken.IsExpired())
                return _cachedToken;

            var scopes = context.Scopes.Length > 0 ? context.Scopes : _defaultScopes;
            var scopeStr = string.Join(' ', scopes);

            using var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string,string>("grant_type", "client_credentials"),
                new KeyValuePair<string,string>("client_id", _clientId),
                new KeyValuePair<string,string>("client_secret", _clientSecret),
                new KeyValuePair<string,string>("scope", scopeStr)
            });

            using var response = await _httpClient.PostAsync(_tokenEndpoint, form, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken).ConfigureAwait(false);
            var root = doc.RootElement;

            string token = root.GetProperty("access_token").GetString()!;
            int expiresIn = root.TryGetProperty("expires_in", out var e) ? e.GetInt32() : 3600;

            _cachedToken = new AccessToken(token, DateTimeOffset.UtcNow.AddSeconds(expiresIn));
            return _cachedToken;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="BearerTokenProvider"/>.
    /// </summary>
    /// <remarks>
    /// This method disposes internal synchronization primitives but does not dispose
    /// the HTTP client, which is assumed to be managed by the caller.
    /// </remarks>
    public void Dispose()
    {
        _gate.Dispose();
        // Do not dispose _httpClient if injected via DI (caller owns lifetime).
    }
}

internal static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? s) => string.IsNullOrEmpty(s);
}
