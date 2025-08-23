using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AbeckDev.Amadeus.Configuration;
using AbeckDev.Amadeus.Pipeline;
using AbeckDev.Amadeus.Pipeline.Policies;
using Xunit;

namespace AbeckDev.Amadeus.Test;

public class RetryPolicyTests
{
    [Fact]
    public async Task RetriesOn500()
    {
        int calls = 0;
        var transport = new StubHandler(_ =>
        {
            calls++;
            if (calls < 3)
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("OK") };
        });

        var pipeline = new HttpPipeline(new[] { new RetryPolicy(new RetryOptions { MaxAttempts = 5 }) }, transport);

        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
        var response = await pipeline.SendAsync(request, CancellationToken.None);

        //Check if the request was successful in the end (more than 3 calls)
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        //Check for exactly three calls. THe third one should work.
        Assert.Equal(3, calls);
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _func;
        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> func) => _func = func;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_func(request));
    }

}
