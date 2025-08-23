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
    public interface IAmadeusClient
{
    Task GetDemoAsync(string demoId, CancellationToken cancellationToken = default);
}

    public sealed class AmadeusClient : IAmadeusClient, IDisposable
    {
        private readonly AmadeusClientOptions _options;
        private readonly HttpPipeline _pipeline;
        private readonly JsonSerializerOptions _json;
        private readonly bool _disposeTransport;
        private readonly HttpMessageHandler _transport;

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

        public void Dispose()
        {
            if (_disposeTransport)
                _transport.Dispose();
        }
    }

    internal static class ThisAssembly
    {
        //ToDo: Read from MSBuild
        public const string Version = "0.1.0-preview";
    }
}
