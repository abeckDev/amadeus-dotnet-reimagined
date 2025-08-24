using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AbeckDev.Amadeus.Abstractions;
using AbeckDev.Amadeus.Configuration;
using AbeckDev.Amadeus.Exceptions;
using AbeckDev.Amadeus.Pipeline;
using AbeckDev.Amadeus.Pipeline.Policies;
using System.Collections.Generic;
using AbeckDev.Amadeus.Api.Airlines;


namespace AbeckDev.Amadeus
{
    /// <summary>
    /// Defines the contract for the Amadeus API client.
    /// </summary>
    public interface IAmadeusClient
    {
        Task<HttpResponseMessage> SendApiRequestAsync(HttpRequestMessage message, CancellationToken cancellationToken);

    }

    /// <summary>
    /// A modern, pipeline-based client for the Amadeus Self-Service travel APIs.
    /// This client supports authentication, retry policies, logging, and telemetry through a configurable HTTP pipeline.
    /// </summary>
    /// <remarks>
    /// The client is designed to be thread-safe and can be used as a singleton in dependency injection scenarios.
    /// It implements <see cref="IDisposable"/> to properly manage HTTP resources.
    /// </remarks>
    public sealed class AmadeusClient : IAmadeusClient, IDisposable
    {
        private readonly AmadeusClientOptions _options;
        private readonly HttpPipeline _pipeline;
        private readonly JsonSerializerOptions _json;
        private readonly bool _disposeTransport;
        private readonly HttpMessageHandler _transport;
        public readonly AmadeusClientContext clientContext;

        public Airlines airlines;


        /// <summary>
        /// Initializes a new instance of the <see cref="AmadeusClient"/> class.
        /// </summary>
        /// <param name="options">The configuration options for the client.</param>
        /// <param name="logger">Optional logger for request/response logging. When provided, enables detailed logging with sensitive data redaction.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> is null.</exception>
        /// <remarks>
        /// The constructor builds an HTTP pipeline with the following policies (in order):
        /// 1. Telemetry policy (if enabled)
        /// 2. Logging policy (if logger provided)
        /// 3. Authentication policy (if token provider configured)
        /// 4. Retry policy
        /// 5. Any additional custom policies
        /// </remarks>
        public AmadeusClient(AmadeusClientOptions options, ILogger? logger = null)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            _transport = options.TransportHandler ?? new HttpClientHandler();
            _disposeTransport = options.TransportHandler is null;

            var policies = new System.Collections.Generic.List<IHttpPipelinePolicy>();

            if (options.EnableTelemetry)
                policies.Add(new TelemetryPolicy("YourOrg.Product", ThisAssembly.Version));

            if (logger != null)
                policies.Add(new LoggingPolicy(logger));

            if (options.TokenProvider is not null)
                policies.Add(new AuthPolicy(options.TokenProvider, options.DefaultScopes));

            policies.Add(new RetryPolicy(options.Retry));

            policies.AddRange(options.AdditionalPolicies);

            _pipeline = new HttpPipeline(policies, _transport);

            clientContext = new AmadeusClientContext(_options);

            airlines = new Airlines(this);

        }

        public async Task<HttpResponseMessage> SendApiRequestAsync(HttpRequestMessage message, CancellationToken cancellationToken)
        {
            return await _pipeline.SendAsync(message, cancellationToken);
        }

        /// <summary>
        /// Creates and throws an appropriate exception based on the HTTP response.
        /// </summary>
        /// <param name="response">The failed HTTP response.</param>
        /// <param name="ct">Cancellation token for reading the response body.</param>
        /// <returns>This method never returns; it always throws an exception.</returns>
        /// <exception cref="AmadeusRequestException">Always thrown with details from the response.</exception>
        private static async Task ThrowRequestException(HttpResponseMessage response, CancellationToken ct)
        {
            string? body = null;
            try
            {
                body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                if (body.Length > 500) body = body[..500];
            }
            catch { /* ignore */ }

            throw AmadeusRequestException.FromResponse(response, body, response.Headers.Contains("x-correlation-id")
                ? string.Join(",", response.Headers.GetValues("x-correlation-id"))
                : null);
        }

        /// <summary>
        /// Releases all resources used by the <see cref="AmadeusClient"/>.
        /// </summary>
        /// <remarks>
        /// This method disposes the underlying HTTP transport if it was created by the client.
        /// If a custom transport was provided via <see cref="AmadeusClientOptions.TransportHandler"/>,
        /// it will not be disposed.
        /// </remarks>
        public void Dispose()
        {
            if (_disposeTransport)
                _transport.Dispose();
        }
    }

    /// <summary>
    /// Provides assembly-level version information for the Amadeus SDK.
    /// </summary>
    internal static class ThisAssembly
    {
        /// <summary>
        /// The current version of the Amadeus SDK.
        /// </summary>
        /// <remarks>
        /// TODO: This should be automatically populated from MSBuild properties in future versions.
        /// </remarks>
        public const string Version = "0.1.0-preview";
    }
}
