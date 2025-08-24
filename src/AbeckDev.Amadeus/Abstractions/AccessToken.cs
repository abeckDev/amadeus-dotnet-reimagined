using System;
 
namespace AbeckDev.Amadeus.Abstractions
{
    /// <summary>
    /// Represents an access token with its expiration information.
    /// </summary>
    /// <param name="Token">The access token string.</param>
    /// <param name="ExpiresOn">The date and time when the token expires (in UTC).</param>
    /// <remarks>
    /// This is an immutable value type that provides methods to check token validity.
    /// The token should be considered expired slightly before its actual expiration to allow for network latency.
    /// </remarks>
    public readonly record struct AccessToken(string Token, DateTimeOffset ExpiresOn)
    {
        /// <summary>
        /// Determines whether the access token is expired or about to expire.
        /// </summary>
        /// <param name="earlyRefreshWindow">
        /// The time window before the actual expiration to consider the token expired.
        /// Defaults to 2 minutes if not specified.
        /// </param>
        /// <returns><c>true</c> if the token is expired or within the early refresh window; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// Using an early refresh window helps prevent authentication failures due to clock skew
        /// or network delays during API calls.
        /// </remarks>
        public bool IsExpired(TimeSpan? earlyRefreshWindow = null)
        {
            var window = earlyRefreshWindow ?? TimeSpan.FromMinutes(2);
            return DateTimeOffset.UtcNow >= ExpiresOn - window;
        }
    }
}
