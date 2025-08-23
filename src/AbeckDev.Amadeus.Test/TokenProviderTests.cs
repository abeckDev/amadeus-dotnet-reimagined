using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using AbeckDev.Amadeus.Abstractions;
using AbeckDev.Amadeus.Auth;

namespace AbeckDev.Amadeus.Test
{
    public class TokenProviderTests
    {
        [Fact]
        public async Task CachesToken()
        {
            var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":3600}")
            });
            var client = new HttpClient(handler);

            var provider = new BearerTokenProvider(client, "https://id/token", "id", "secret");
            var token1 = await provider.GetTokenAsync(new TokenRequestContext(Array.Empty<string>()));
            var token2 = await provider.GetTokenAsync(new TokenRequestContext(Array.Empty<string>()));

            //Is the token of the second request the same one than the one from the first request aka from cache
            Assert.Equal(token1.Token, token2.Token);
            //Has the handler called the Endpoint only once
            Assert.Equal(1, handler.Calls);
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _func;
            public int Calls { get; private set; }
            public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> func) => _func = func;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Calls++;
                return Task.FromResult(_func(request));
            }
        }
    }
}