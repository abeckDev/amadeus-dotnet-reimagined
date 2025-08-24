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


namespace AbeckDev.Amadeus
{
    /// <summary>
    /// Defines the contract for the Amadeus API client.
    /// </summary>
    public interface IAmadeusClient
    {
        /// <summary>
        /// Gets demo data asynchronously. This is a placeholder method for the current preview version.
        /// </summary>
        /// <param name="demoId">The unique identifier for the demo data to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="demoId"/> is null or whitespace.</exception>
        Task GetDemoAsync(string demoId, CancellationToken cancellationToken = default);
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
        }

        /// <summary>
        /// Gets demo data asynchronously. This is a placeholder method for the current preview version.
        /// </summary>
        /// <param name="demoId">The unique identifier for the demo data to retrieve.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="demoId"/> is null or whitespace.</exception>
        /// <exception cref="AmadeusRequestException">Thrown when the API request fails.</exception>
        /// <remarks>
        /// This method demonstrates the HTTP pipeline in action. In the full version, this will be replaced
        /// with actual Amadeus API endpoint methods.
        /// </remarks>
        public async Task GetDemoAsync(string demoId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(demoId))
                throw new ArgumentException("demoId must be provided.", nameof(demoId));

            var uri = new Uri(_options.Endpoint, $"coolStuff/{demoId}");

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            using var response = await _pipeline.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
                await ThrowRequestException(response, cancellationToken).ConfigureAwait(false);

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

            //Return Stuff later
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
