using System;
using System.Net.Http;
using System.Text.Json;
using AbeckDev.Amadeus.Configuration;
using AbeckDev.Amadeus.Abstractions;

namespace AbeckDev.Amadeus;

public sealed class AmadeusClientContext
{
    public AmadeusClientOptions Options { get; }
    public HttpClient HttpClient { get; }
    public ITokenProvider TokenProvider { get; }
    public JsonSerializerOptions Json { get; }


    public AmadeusClientContext(AmadeusClientOptions options)
    {
        Options = options;
    }
}
