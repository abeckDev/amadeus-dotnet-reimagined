using System;
 
namespace AbeckDev.Amadeus.Abstractions
{
    public readonly record struct AccessToken(string Token, DateTimeOffset ExpiresOn)
    {
        public bool IsExpired(TimeSpan? earlyRefreshWindow = null)
        {
            var window = earlyRefreshWindow ?? TimeSpan.FromMinutes(2);
            return DateTimeOffset.UtcNow >= ExpiresOn - window;
        }
    }
}
