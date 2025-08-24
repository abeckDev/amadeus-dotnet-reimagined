# Amadeus .NET SDK (Reimagined)

A modern, reimagined .NET SDK for the Amadeus Self-Service travel APIs. This project is a complete reboot of the [amadeus4dev-examples/amadeus-dotnet](https://github.com/amadeus4dev-examples/amadeus-dotnet) library, designed with modern .NET practices and enhanced functionality.

## 🎯 Vision

This SDK aims to provide:
- **Modern Architecture**: Built with .NET 8, leveraging the latest language features and patterns
- **Pipeline-Based Design**: Extensible HTTP processing pipeline with pluggable policies
- **Developer Experience**: Intuitive APIs, comprehensive documentation, and tooling support
- **Enterprise Ready**: Robust error handling, retry policies, logging, and telemetry
- **Dependency Injection**: Support for Microsoft.Extensions.DependencyInjection
- **Performance**: Efficient HTTP handling with proper resource management

## 🚀 Quick Start

### Installation

```bash
dotnet add package AbeckDev.Amadeus --prerelease
```

### Basic Usage

```csharp
using AbeckDev.Amadeus;
using AbeckDev.Amadeus.Configuration;
using AbeckDev.Amadeus.Auth;

// Configure the client
var options = new AmadeusClientOptions(new Uri("https://api.amadeus.com"))
{
    TokenProvider = new BearerTokenProvider(
        httpClient,
        "https://api.amadeus.com/v1/security/oauth2/token",
        "your-client-id",
        "your-client-secret"
    ),
    DefaultScopes = new[] { "amadeus-api" },
    EnableTelemetry = true
};

// Create the client
using var client = new AmadeusClient(options, logger);

// Use the client (example - actual API methods will be added)
await client.GetDemoAsync("some-id");
```

### With Dependency Injection

```csharp
services.AddSingleton(sp => new AmadeusClientOptions(new Uri("https://api.amadeus.com"))
{
    TokenProvider = new BearerTokenProvider(/* ... */),
    DefaultScopes = new[] { "amadeus-api" }
});

services.AddSingleton<IAmadeusClient, AmadeusClient>();
```

## 🏗️ Architecture

### Core Components

- **AmadeusClient**: Main client implementing `IAmadeusClient`
- **Configuration**: `AmadeusClientOptions` for client setup and `RetryOptions` for retry behavior
- **Authentication**: `ITokenProvider` abstraction with `BearerTokenProvider` implementation
- **HTTP Pipeline**: Extensible pipeline with built-in policies

### HTTP Pipeline Policies

The SDK uses a pipeline-based approach for HTTP processing:

1. **TelemetryPolicy**: Adds User-Agent and telemetry headers
2. **LoggingPolicy**: Logs requests and responses (with sensitive data redaction)
3. **AuthPolicy**: Handles OAuth token injection and refresh
4. **RetryPolicy**: Implements exponential backoff with jitter
5. **Custom Policies**: Add your own policies via `AmadeusClientOptions.AddPolicy()`

### Built-in Features

- **Token Caching**: Automatic token refresh and caching
- **Retry Logic**: Configurable retry with exponential backoff
- **Logging**: Structured logging with Microsoft.Extensions.Logging
- **Telemetry**: Optional telemetry headers for API analytics
- **Error Handling**: Detailed exception types for different failure scenarios

## ⚙️ Configuration

### Retry Configuration

```csharp
var options = new AmadeusClientOptions(endpoint)
{
    Retry = new RetryOptions
    {
        MaxAttempts = 3,
        BaseDelay = TimeSpan.FromMilliseconds(100),
        MaxDelay = TimeSpan.FromSeconds(10),
        RetryOnTimeouts = true
    }
};
```

### Custom Policies

```csharp
var options = new AmadeusClientOptions(endpoint)
    .AddPolicy(new MyCustomPolicy())
    .AddPolicy(new AnotherPolicy());
```

## 🧪 Current Status

**Version**: 0.1.0-preview

This is an early preview focusing on the foundational architecture. The core infrastructure is complete and includes:

- ✅ HTTP pipeline with policies
- ✅ Authentication and token management
- ✅ Retry logic with exponential backoff
- ✅ Logging and telemetry
- ✅ Configuration system
- ✅ Unit tests for core components

**Upcoming**: API endpoint implementations for Amadeus travel services.

## 🛠️ Development

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code

### Building

```bash
git clone https://github.com/abeckDev/amadeus-dotnet-reimagined.git
cd amadeus-dotnet-reimagined
dotnet build
```

### Running Tests

```bash
dotnet test
```

## 📋 Roadmap

- [ ] Complete API endpoint implementations
- [ ] Add response model classes
- [ ] Implement request/response serialization
- [ ] Add more comprehensive documentation
- [ ] Performance optimizations
- [ ] Additional authentication methods

## 🤝 Contributing

We welcome contributions! This project aims to provide a modern, well-architected SDK for the Amadeus APIs.

### Guidelines

1. Follow existing code patterns and conventions
2. Add unit tests for new functionality
3. Update documentation for public APIs
4. Use conventional commit messages

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Related

- [Amadeus for Developers](https://developers.amadeus.com/)
- [Original amadeus-dotnet](https://github.com/amadeus4dev-examples/amadeus-dotnet)
- [Amadeus API Documentation](https://developers.amadeus.com/self-service)
