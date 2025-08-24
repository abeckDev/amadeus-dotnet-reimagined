using System;
using System.Net;
using System.Net.Http;
using AbeckDev.Amadeus.Exceptions;
using Xunit;

namespace AbeckDev.Amadeus.Test
{
    public class AmadeusRequestExceptionTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange
            const string message = "Test error message";
            const HttpStatusCode statusCode = HttpStatusCode.BadRequest;
            const string responseBody = "Error details from API";
            const string correlationId = "abc-123-xyz";
            var innerException = new InvalidOperationException("Inner exception");

            // Act
            var exception = new AmadeusRequestException(
                message,
                statusCode,
                responseBody,
                correlationId,
                innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(statusCode, exception.StatusCode);
            Assert.Equal(responseBody, exception.ResponseBody);
            Assert.Equal(correlationId, exception.CorrelationId);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void Constructor_WithNullOptionalParams_InitializesCorrectly()
        {
            // Arrange
            const string message = "Test error message";
            const HttpStatusCode statusCode = HttpStatusCode.Unauthorized;

            // Act
            var exception = new AmadeusRequestException(
                message,
                statusCode,
                null, // responseBody
                null, // correlationId
                null); // innerException

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Equal(statusCode, exception.StatusCode);
            Assert.Null(exception.ResponseBody);
            Assert.Null(exception.CorrelationId);
            Assert.Null(exception.InnerException);
        }

        [Fact]
        public void AmadeusRequestException_InheritsFromAmadeusException()
        {
            // Arrange & Act
            var exception = new AmadeusRequestException(
                "Test message",
                HttpStatusCode.InternalServerError,
                null,
                null);

            // Assert
            Assert.IsType<AmadeusRequestException>(exception);
            Assert.IsAssignableFrom<AmadeusException>(exception);
        }

        [Fact]
        public void FromResponse_CreatesExceptionWithCorrectProperties()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                ReasonPhrase = "Resource Not Found"
            };
            const string responseBody = "The requested resource was not found";
            const string correlationId = "request-123-correlation";

            // Act
            var exception = AmadeusRequestException.FromResponse(responseMessage, responseBody, correlationId);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal(responseBody, exception.ResponseBody);
            Assert.Equal(correlationId, exception.CorrelationId);
            Assert.Contains("404", exception.Message);
            Assert.Contains("Resource Not Found", exception.Message);
        }

        [Fact]
        public void FromResponse_WithNullOptionalParams_CreatesExceptionCorrectly()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                ReasonPhrase = "Bad Gateway"
            };

            // Act
            var exception = AmadeusRequestException.FromResponse(responseMessage, null, null);

            // Assert
            Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
            Assert.Null(exception.ResponseBody);
            Assert.Null(exception.CorrelationId);
            Assert.Contains("502", exception.Message);
            Assert.Contains("Bad Gateway", exception.Message);
        }

        [Fact]
        public void FromResponse_MessageContainsStatusCodeAndReason()
        {
            // Arrange
            var responseMessage = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                ReasonPhrase = "Service Unavailable"
            };

            // Act
            var exception = AmadeusRequestException.FromResponse(responseMessage, "Error body", "correlation-xyz");

            // Assert
            Assert.Contains("503", exception.Message);
            Assert.Contains("Service Unavailable", exception.Message);
            Assert.Matches("Request failed with status 503 \\(Service Unavailable\\)\\.", exception.Message);
        }

        [Fact]
        public void ProductAuthenticationException_InitializesProperties()
        {
            // Arrange
            const string message = "Authentication failed";
            var innerException = new InvalidOperationException("Inner exception");

            // Act
            var exception = new ProductAuthenticationException(message, innerException);

            // Assert
            Assert.Equal(message, exception.Message);
            Assert.Same(innerException, exception.InnerException);
        }

        [Fact]
        public void ProductAuthenticationException_InheritsFromAmadeusException()
        {
            // Arrange & Act
            var exception = new ProductAuthenticationException("Authentication failed");

            // Assert
            Assert.IsType<ProductAuthenticationException>(exception);
            Assert.IsAssignableFrom<AmadeusException>(exception);
        }
    }
}