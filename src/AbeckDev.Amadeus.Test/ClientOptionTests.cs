using System;
using System.Linq;
using System.Net.Http;
using AbeckDev.Amadeus.Abstractions;
using AbeckDev.Amadeus.Pipeline;
using AbeckDev.Amadeus.Configuration;
using AbeckDev.Amadeus.Pipeline.Policies;
using Xunit;
using AbeckDev.Amadeus.Auth;

namespace AbeckDev.Amadeus.Test;

public class ClientOptionTests
{
    private readonly Uri _testEndpoint = new Uri("https://test.api.amadeus.com");

    [Fact]
    public void Constructor_WithValidEndpoint_SetsEndpointProperty()
    {
        // Arrange & Act
        var options = new AmadeusClientOptions(_testEndpoint);

        // Assert
        Assert.Equal(_testEndpoint, options.Endpoint);
    }

    [Fact]
    public void Constructor_WithNullEndpoint_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AmadeusClientOptions(null!));
    }

    [Fact]
    public void DefaultProperties_HaveExpectedValues()
    {
        // Arrange & Act
        var options = new AmadeusClientOptions(_testEndpoint);

        // Assert
        Assert.Null(options.TokenProvider);
        Assert.Empty(options.DefaultScopes);
        Assert.NotNull(options.Retry);
        Assert.Equal(TimeSpan.FromSeconds(100), options.DefaultTimeout);
        Assert.True(options.EnableTelemetry);
        Assert.Null(options.TransportHandler);
        Assert.Empty(options.AdditionalPolicies);
    }


    //Mock Token Provider
    class TestTokenProvider : ITokenProvider
    {
        public ValueTask<AccessToken> GetTokenAsync(TokenRequestContext context, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void CustomProperties_SetCorrectly()
    {

        // Define test data
        var tokenProvider = new TestTokenProvider();
        var scopes = new[] { "scope1", "scope2" };
        var retry = new RetryOptions { MaxAttempts = 10 };
        var timeout = TimeSpan.FromSeconds(200);
        var telemetry = false;
        var handler = new HttpClientHandler();

        // Create Options object
        var options = new AmadeusClientOptions(_testEndpoint)
        {
            TokenProvider = tokenProvider,
            DefaultScopes = scopes,
            Retry = retry,
            DefaultTimeout = timeout,
            EnableTelemetry = telemetry,
            TransportHandler = handler
        };

        // Check
        Assert.Same(tokenProvider, options.TokenProvider);
        Assert.Same(scopes, options.DefaultScopes);
        Assert.Same(retry, options.Retry);
        Assert.Equal(timeout, options.DefaultTimeout);
        Assert.Equal(telemetry, options.EnableTelemetry);
        Assert.Same(handler, options.TransportHandler);
    }

    class TestPipelinePolicy : IHttpPipelinePolicy
    {
        public async Task<HttpResponseMessage> ProcessAsync(PipelineContext context, HttpRequestMessage request, PipelineCall next, CancellationToken cancellationToken)
        {
            // Pass through to next policy without modification
            var response = await next(context, request, cancellationToken).ConfigureAwait(false);
            return response;

        }
    }

    [Fact]
    public void AddPolicy_WithValidPolicy_AddsPolicyToCollection()
    {
        // Arrange
        var options = new AmadeusClientOptions(_testEndpoint);
        var policy = new TestPipelinePolicy();

        // Act
        var result = options.AddPolicy(policy);

        // Assert
        Assert.Same(options, result); // Fluent return
        Assert.Single(options.AdditionalPolicies);
        Assert.Same(policy, options.AdditionalPolicies.First());
    }

    [Fact]
    public void AddPolicy_WithNullPolicy_ThrowsArgumentNullException()
    {
        // Arrange
        var options = new AmadeusClientOptions(_testEndpoint);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => options.AddPolicy(null!));
    }

    [Fact]
    public void AddPolicy_MultiplePolicies_AddsAllPolicies()
    {
        // Arrange
        var options = new AmadeusClientOptions(_testEndpoint);
        var policy1 = new TestPipelinePolicy();
        using (var httpClient = new HttpClient())
        {
            var policy2 = new AuthPolicy(
                new BearerTokenProvider(httpClient, "testClientId", "testClientSecret", "testTokenEndpoint"),
                null!);

            // Act
            options.AddPolicy(policy1).AddPolicy(policy2);

            // Assert
            Assert.Equal(2, options.AdditionalPolicies.Count);
            Assert.Contains(policy1, options.AdditionalPolicies);
            Assert.Contains(policy2, options.AdditionalPolicies);
        }
    }

    [Fact]
    public void RetryOptions_DefaultValues()
    {
        // Arrange & Act
        var retryOptions = new RetryOptions();

        // Assert
        Assert.Equal(5, retryOptions.MaxAttempts);
        Assert.Equal(TimeSpan.FromMilliseconds(200), retryOptions.BaseDelay);
        Assert.Equal(TimeSpan.FromSeconds(5), retryOptions.MaxDelay);
        Assert.True(retryOptions.RetryOnTimeouts);
    }

    [Fact]
    public void RetryOptions_CustomValues()
    {
        // Arrange & Act
        var retryOptions = new RetryOptions
        {
            MaxAttempts = 10,
            BaseDelay = TimeSpan.FromMilliseconds(500),
            MaxDelay = TimeSpan.FromSeconds(30),
            RetryOnTimeouts = false
        };

        // Assert
        Assert.Equal(10, retryOptions.MaxAttempts);
        Assert.Equal(TimeSpan.FromMilliseconds(500), retryOptions.BaseDelay);
        Assert.Equal(TimeSpan.FromSeconds(30), retryOptions.MaxDelay);
        Assert.False(retryOptions.RetryOnTimeouts);
    }

    
}
