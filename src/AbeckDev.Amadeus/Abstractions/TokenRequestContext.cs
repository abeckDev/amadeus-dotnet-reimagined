using System;

namespace AbeckDev.Amadeus.Abstractions
{
    /// <summary>
    /// Represents the context for a token request, including the requested scopes.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the information needed to request an access token,
    /// primarily the OAuth scopes that define the permissions being requested.
    /// </remarks>
    public sealed class TokenRequestContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRequestContext"/> class.
        /// </summary>
        /// <param name="scopes">The OAuth scopes to request for the token.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="scopes"/> is <c>null</c>.</exception>
        public TokenRequestContext(string[] scopes)
            => Scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));

        /// <summary>
        /// Gets the OAuth scopes requested for the token.
        /// </summary>
        /// <value>An array of scope strings that define the permissions being requested.</value>
        public string[] Scopes { get; }
    }
}
