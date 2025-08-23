using System;

namespace AbeckDev.Amadeus.Pipeline;

public sealed class PipelineContext
{
    public string RequestId { get; }
    public int Attempt { get; set; }
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    public PipelineContext(string? requestId = null)
    {
        RequestId = requestId ?? Guid.NewGuid().ToString("N");
    }
}
