using System;

namespace AbeckDev.Amadeus.Pipeline;

/// <summary>
/// Represents the execution context for an HTTP request as it flows through the pipeline.
/// </summary>
/// <remarks>
/// The pipeline context provides a way to share state and metadata between pipeline policies.
/// Each request gets its own context instance that lives for the duration of the request.
/// </remarks>
public sealed class PipelineContext
{
    /// <summary>
    /// Gets the unique identifier for this request.
    /// </summary>
    /// <value>A unique string identifier that can be used for correlation and logging.</value>
    public string RequestId { get; }

    /// <summary>
    /// Gets or sets the current attempt number for this request.
    /// </summary>
    /// <value>The attempt number, starting from 1 for the initial request. Used by retry policies.</value>
    public int Attempt { get; set; }

    /// <summary>
    /// Gets a dictionary for storing arbitrary data associated with this request.
    /// </summary>
    /// <value>A dictionary that can be used by policies to store and retrieve state.</value>
    /// <remarks>
    /// This allows policies to communicate with each other by storing data that can be
    /// accessed later in the pipeline or on subsequent retry attempts.
    /// </remarks>
    public IDictionary<string, object?> Items { get; } = new Dictionary<string, object?>();

    /// <summary>
    /// Initializes a new instance of the <see cref="PipelineContext"/> class.
    /// </summary>
    /// <param name="requestId">
    /// An optional request identifier. If not provided, a new GUID will be generated.
    /// </param>
    public PipelineContext(string? requestId = null)
    {
        RequestId = requestId ?? Guid.NewGuid().ToString("N");
    }
}
