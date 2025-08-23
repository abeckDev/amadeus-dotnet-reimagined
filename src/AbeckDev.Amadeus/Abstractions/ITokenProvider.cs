using System;

namespace AbeckDev.Amadeus.Abstractions
{
    public interface ITokenProvider
    {
        ValueTask<AccessToken> GetTokenAsync(TokenRequestContext context, CancellationToken cancellationToken = default);
    }
}
