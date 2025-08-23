using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AbeckDev.Amadeus.Pipeline;

public interface IHttpPipelinePolicy
{
    Task<HttpResponseMessage> ProcessAsync(PipelineContext context, HttpRequestMessage request, PipelineCall next, CancellationToken cancellationToken);
}

public delegate Task<HttpResponseMessage> PipelineCall(PipelineContext context, HttpRequestMessage request, CancellationToken cancellationToken);
