using System;

namespace AbeckDev.Amadeus.Abstractions
{
    /// <summary>
    /// Defines a contract for providing access tokens for API authentication.
    /// </summary>
    /// <remarks>
    /// Implementations should handle token caching, refresh logic, and thread safety.
    /// The provider will be called by the authentication pipeline policy to obtain tokens as needed.
    /// </remarks>
    public interface ITokenProvider
    {
        /// <summary>
        /// Asynchronously retrieves an access token for the specified context.
        /// </summary>
        /// <param name="context">The token request context containing scopes and other authentication details.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A <see cref="ValueTask{AccessToken}"/> representing the asynchronous operation that yields an access token.</returns>
        /// <remarks>
        /// Implementations should return cached tokens when they are still valid, and automatically
        /// refresh tokens when they are expired or near expiration.
        /// </remarks>
        ValueTask<AccessToken> GetTokenAsync(TokenRequestContext context, CancellationToken cancellationToken = default);
    }
}
