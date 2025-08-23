using System;

namespace AbeckDev.Amadeus.Abstractions
{
    public sealed class TokenRequestContext
    {
        public TokenRequestContext(string[] scopes)
            => Scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));

        public string[] Scopes { get; }
    }
}
