using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AbeckDev.Amadeus.Abstractions;

namespace AbeckDev.Amadeus.Auth;

public sealed class BearerTokenProvider : ITokenProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _tokenEndpoint;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string[] _defaultScopes;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private AccessToken _cachedToken;

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
