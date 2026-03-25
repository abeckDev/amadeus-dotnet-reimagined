# Amadeus .NET SDK (Reimagined)

> [!WARNING]
> **⚠️ This repository is archived and no longer maintained.**
>
> The Amadeus Self-Service APIs have been retired. As a result, this SDK is no longer functional and this repository has been sunset. No further development, bug fixes, or support will be provided.
>
> Thank you to everyone who contributed to and used this project.

~~[![CI/CD Pipeline](https://github.com/abeckDev/amadeus-dotnet-reimagined/actions/workflows/ci.yml/badge.svg)](https://github.com/abeckDev/amadeus-dotnet-reimagined/actions/workflows/ci.yml)~~
~~[![codecov](https://codecov.io/gh/abeckDev/amadeus-dotnet-reimagined/branch/main/graph/badge.svg)](https://codecov.io/gh/abeckDev/amadeus-dotnet-reimagined)~~

A modern, reimagined .NET SDK for the Amadeus Self-Service travel APIs. This project is a complete reboot of the [amadeus4dev-examples/amadeus-dotnet](https://github.com/amadeus4dev-examples/amadeus-dotnet) library, designed with modern .NET practices and enhanced functionality.

> **Note**: This project is preserved for historical and educational purposes only. The underlying Amadeus Self-Service APIs are no longer available.

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

## 🧪 Status

**Version**: 0.1.0-preview — **ARCHIVED**

> ⚠️ This project is no longer active. The Amadeus Self-Service APIs have been retired and this repository is preserved for reference only.

The core infrastructure that was implemented includes:

- ✅ HTTP pipeline with policies
- ✅ Authentication and token management
- ✅ Retry logic with exponential backoff
- ✅ Logging and telemetry
- ✅ Configuration system
- ✅ Unit tests for core components

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

### Running Tests with Coverage

```bash
dotnet test --collect:"XPlat Code Coverage" --results-directory ./test-results
```

### Creating NuGet Package

```bash
dotnet pack src/AbeckDev.Amadeus/AbeckDev.Amadeus.csproj --configuration Release --include-symbols --include-source
```

## 🔄 CI/CD Pipeline

This project uses GitHub Actions for automated build, test, and deployment workflows:

### Workflows

- **CI/CD Pipeline** (`.github/workflows/ci.yml`)
  - Builds and tests across multiple .NET versions and OS platforms
  - Generates code coverage reports
  - Performs security analysis with CodeQL
  - Creates NuGet packages (does not publish yet)
  - Runs on push to main and pull requests

- **Pull Request Validation** (`.github/workflows/pr.yml`)
  - Validates builds and tests on pull requests
  - Reports test coverage status directly in PR comments
  - Ensures quality gates are met before merging

### Required Secrets

To enable full CI/CD functionality, these repository secrets are available:

- `CODECOV_TOKEN`: Token for uploading coverage reports to Codecov (optional)

## 📋 ~~Roadmap~~ (Archived)

This project has been archived and the roadmap is no longer being pursued. The Amadeus Self-Service APIs have been retired.

## 🤝 Contributing

This repository is archived and no longer accepts contributions.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Related

- [Amadeus for Developers](https://developers.amadeus.com/)
- [Original amadeus-dotnet](https://github.com/amadeus4dev-examples/amadeus-dotnet)
- [Amadeus API Documentation](https://developers.amadeus.com/self-service)
