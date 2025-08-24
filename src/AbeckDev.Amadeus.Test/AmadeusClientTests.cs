using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AbeckDev.Amadeus.Configuration;
using AbeckDev.Amadeus.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace AbeckDev.Amadeus.Test
{

    public class AmadeusClientTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly AmadeusClientOptions _defaultOptions;

        public AmadeusClientTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            _defaultOptions = new AmadeusClientOptions(new Uri("https://api.amadeus.com/"))
            {
                TransportHandler = _mockHandler.Object
            };
        }

        [Fact]
        public void Constructor_WithValidOptions_ShouldCreateInstance()
        {
            // Arrange & Act
            using var client = new AmadeusClient(_defaultOptions);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new AmadeusClient(null!));
        }

        [Fact]
        public void Constructor_WithLogger_ShouldCreateInstance()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();

            // Act
            using var client = new AmadeusClient(_defaultOptions, mockLogger.Object);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void Dispose_WithDefaultTransport_ShouldDisposeTransport()
        {
            // Arrange
            var options = new AmadeusClientOptions(new Uri("https://api.amadeus.com/"))
            {
                // No custom transport handler - should create and dispose its own
            };

            // Act & Assert - Should not throw
            using var client = new AmadeusClient(options);
            client.Dispose();
        }

        public void Dispose()
        {
            _mockHandler?.Object?.Dispose();
        }
    }
}