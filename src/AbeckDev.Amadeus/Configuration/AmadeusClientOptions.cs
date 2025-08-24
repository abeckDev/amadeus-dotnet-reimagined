using System;
using System.Collections.Generic;
using System.Net.Http;
using AbeckDev.Amadeus.Abstractions;
using AbeckDev.Amadeus.Pipeline;

namespace AbeckDev.Amadeus.Configuration;

/// <summary>
/// Configuration options for the <see cref="AmadeusClient"/>.
/// </summary>
/// <remarks>
/// This class provides a fluent configuration API for setting up the Amadeus client,
/// including authentication, retry policies, and HTTP pipeline customization.
/// </remarks>
public sealed class AmadeusClientOptions
{
    /// <summary>
    /// Gets the base endpoint URI for the Amadeus API.
    /// </summary>
    /// <value>The API endpoint URI. This is typically https://api.amadeus.com for production.</value>
    public Uri Endpoint { get; init; }

    /// <summary>
    /// Gets or sets the token provider for authentication.
    /// </summary>
    /// <value>
    /// The token provider instance, or <c>null</c> if no authentication is required.
    /// When set, the authentication policy will be added to the HTTP pipeline.
    /// </value>
    public ITokenProvider? TokenProvider { get; init; }

    /// <summary>
    /// Gets or sets the default OAuth scopes to request when authenticating.
    /// </summary>
    /// <value>An array of scope strings. Defaults to an empty array.</value>
    public string[] DefaultScopes { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the retry configuration for failed requests.
    /// </summary>
    /// <value>The retry options. A default configuration is used if not specified.</value>
    public RetryOptions Retry { get; init; } = new();

    /// <summary>
    /// Gets or sets the default timeout for HTTP requests.
    /// </summary>
    /// <value>The default timeout. Defaults to 100 seconds.</value>
    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Gets or sets whether to enable telemetry headers in requests.
    /// </summary>
    /// <value><c>true</c> to add telemetry headers; otherwise, <c>false</c>. Defaults to <c>true</c>.</value>
    /// <remarks>
    /// When enabled, the client will add User-Agent and other telemetry headers to help with API analytics.
    /// </remarks>
    public bool EnableTelemetry { get; init; } = true;

    /// <summary>
    /// Gets or sets a custom HTTP message handler for the transport layer.
    /// </summary>
    /// <value>
    /// A custom HTTP message handler, or <c>null</c> to use the default <see cref="HttpClientHandler"/>.
    /// When a custom handler is provided, it will not be disposed by the client.
    /// </value>
    public HttpMessageHandler? TransportHandler { get; init; }

    /// <summary>
    /// Gets the list of additional HTTP pipeline policies.
    /// </summary>
    /// <value>An internal list of custom policies added via <see cref="AddPolicy"/>.</value>
    internal List<IHttpPipelinePolicy> AdditionalPolicies { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AmadeusClientOptions"/> class.
    /// </summary>
    /// <param name="endpoint">The base API endpoint URI.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="endpoint"/> is <c>null</c>.</exception>
    public AmadeusClientOptions(Uri endpoint)
        => Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    /// <summary>
    /// Adds a custom HTTP pipeline policy to the client configuration.
    /// </summary>
    /// <param name="policy">The policy to add to the pipeline.</param>
    /// <returns>This <see cref="AmadeusClientOptions"/> instance for fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="policy"/> is <c>null</c>.</exception>
    /// <remarks>
    /// Custom policies are added after the built-in policies (telemetry, logging, auth, retry)
    /// but before the final HTTP transport.
    /// </remarks>
    public AmadeusClientOptions AddPolicy(IHttpPipelinePolicy policy)
    {
        if (policy == null) throw new ArgumentNullException(nameof(policy));
        AdditionalPolicies.Add(policy);
        return this;
    }
}

/// <summary>
/// Configuration options for HTTP request retry behavior.
/// </summary>
/// <remarks>
/// The retry policy uses exponential backoff with jitter to avoid thundering herd problems.
/// It will retry on server errors (5xx), timeouts, and network errors.
/// </remarks>
public sealed class RetryOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    /// <value>The maximum retry attempts. Defaults to 5.</value>
    /// <remarks>
    /// This includes the initial request, so a value of 5 means up to 4 retries after the first failure.
    /// </remarks>
    public int MaxAttempts { get; init; } = 5;

    /// <summary>
    /// Gets or sets the base delay between retry attempts.
    /// </summary>
    /// <value>The base delay. Defaults to 200 milliseconds.</value>
    /// <remarks>
    /// The actual delay uses exponential backoff: base_delay * (2 ^ attempt) + random_jitter.
    /// </remarks>
    public TimeSpan BaseDelay { get; init; } = TimeSpan.FromMilliseconds(200);

    /// <summary>
    /// Gets or sets the maximum delay between retry attempts.
    /// </summary>
    /// <value>The maximum delay. Defaults to 5 seconds.</value>
    /// <remarks>
    /// This caps the exponential backoff to prevent excessively long delays.
    /// </remarks>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets whether to retry on timeout exceptions.
    /// </summary>
    /// <value><c>true</c> to retry on timeouts; otherwise, <c>false</c>. Defaults to <c>true</c>.</value>
    /// <remarks>
    /// When enabled, <see cref="TaskCanceledException"/> (which includes timeouts) will trigger retries.
    /// </remarks>
    public bool RetryOnTimeouts { get; init; } = true;
}
