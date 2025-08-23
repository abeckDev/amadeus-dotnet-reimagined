using System;
using System.Collections.Generic;
using System.Net.Http;
using AbeckDev.Amadeus.Abstractions;
using AbeckDev.Amadeus.Pipeline;

namespace AbeckDev.Amadeus.Configuration;

public sealed class AmadeusClientOptions
{
    public Uri Endpoint { get; init; }
    public ITokenProvider? TokenProvider { get; init; }
    public string[] DefaultScopes { get; init; } = Array.Empty<string>();
    public RetryOptions Retry { get; init; } = new();
    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(100);
    public bool EnableTelemetry { get; init; } = true;

    // Optional injection points:
    public HttpMessageHandler? TransportHandler { get; init; }

    internal List<IHttpPipelinePolicy> AdditionalPolicies { get; } = new();

    public AmadeusClientOptions(Uri endpoint)
        => Endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

    public AmadeusClientOptions AddPolicy(IHttpPipelinePolicy policy)
    {
        if (policy == null) throw new ArgumentNullException(nameof(policy));
        AdditionalPolicies.Add(policy);
        return this;
    }
}

public sealed class RetryOptions
{
    public int MaxAttempts { get; init; } = 5;
    public TimeSpan BaseDelay { get; init; } = TimeSpan.FromMilliseconds(200);
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(5);
    public bool RetryOnTimeouts { get; init; } = true;
}
