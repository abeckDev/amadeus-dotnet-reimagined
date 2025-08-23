using System;

namespace AbeckDev.Amadeus.Pipeline.Policies;

public sealed class TelemetryPolicy : IHttpPipelinePolicy
{
    private readonly string _userAgent;

    public TelemetryPolicy(string productName, string version)
    {
        _userAgent = $"{productName}/{version} (.NET {System.Environment.Version})";
    }

    public Task<HttpResponseMessage> ProcessAsync(PipelineContext context, HttpRequestMessage request, PipelineCall next, CancellationToken cancellationToken)
    {
        request.Headers.UserAgent.ParseAdd(_userAgent);
        request.Headers.Add("x-sdk-request-id", context.RequestId);
        return next(context, request, cancellationToken);
    }
}
