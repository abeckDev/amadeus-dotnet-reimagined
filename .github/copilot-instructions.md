# Amadeus .NET SDK (Reimagined)

A modern, pipeline-based .NET 8 SDK for the Amadeus Self-Service travel APIs. This SDK provides a robust, extensible HTTP processing pipeline with authentication, retry policies, logging, and telemetry.

**ALWAYS reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Prerequisites
- .NET 8.0 SDK (8.0.119 or later)
- Visual Studio 2022, VS Code, or any .NET-compatible IDE
- No additional external dependencies or tools required

### Bootstrap, Build, and Test the Repository
```bash
# 1. Restore NuGet packages - takes ~1s (cached) or ~25s (fresh). NEVER CANCEL.
dotnet restore

# 2. Build the solution - takes ~1.5 seconds after restore. NEVER CANCEL.
dotnet build

# 3. Run all tests - takes ~4 seconds, runs 30 tests. NEVER CANCEL.
dotnet test

# 4. Verify code formatting - takes ~8 seconds. NEVER CANCEL.
dotnet format --verify-no-changes
```

**CRITICAL TIMING**: Set timeouts of 60+ seconds for restore operations and 30+ seconds for other commands. NEVER CANCEL builds or tests.

### Fix Code Formatting
```bash
# Apply automatic formatting fixes
dotnet format
```

### Development Workflow
- ALWAYS run `dotnet restore` first after cloning or when package references change
- ALWAYS run `dotnet build` to verify compilation before making changes
- ALWAYS run `dotnet test` to ensure existing functionality works
- ALWAYS run `dotnet format --verify-no-changes` before committing changes

## Validation

### Manual Testing Scenarios
After making changes, ALWAYS validate with a test application:
```bash
# Create test console app
mkdir /tmp/test-app && cd /tmp/test-app
dotnet new console -f net8.0
dotnet add reference /path/to/src/AbeckDev.Amadeus/AbeckDev.Amadeus.csproj
dotnet add package Microsoft.Extensions.Logging.Console

# Test basic SDK instantiation
cat > Program.cs << 'EOF'
using AbeckDev.Amadeus;
using AbeckDev.Amadeus.Configuration;
using Microsoft.Extensions.Logging;

Console.WriteLine("Testing Amadeus .NET SDK...");
var options = new AmadeusClientOptions(new Uri("https://api.amadeus.com"));
using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<Program>();
using var client = new AmadeusClient(options, logger);
Console.WriteLine("✓ SDK working correctly!");
EOF

dotnet run
```

The test should output: "✓ SDK working correctly!" without errors.

### Required Validation Steps
- ALWAYS verify all 30 tests pass after changes: `dotnet test`
- ALWAYS validate code formatting: `dotnet format --verify-no-changes`
- ALWAYS test basic SDK instantiation with the scenario above
- ALWAYS build solution without warnings: `dotnet build`

## Architecture Overview

### Core Projects
- **`src/AbeckDev.Amadeus/`** - Main SDK library
- **`src/AbeckDev.Amadeus.Test/`** - Unit tests (xUnit with Moq)

### Key Components
- **Configuration**: `AmadeusClientOptions` - Client setup and configuration
- **Authentication**: `BearerTokenProvider` - OAuth2 token management  
- **Pipeline**: HTTP processing pipeline with pluggable policies
  - `AuthPolicy` - Authentication handling
  - `RetryPolicy` - Configurable retry logic
  - `LoggingPolicy` - Request/response logging
  - `TelemetryPolicy` - Performance metrics
- **Abstractions**: Core interfaces (`IAmadeusClient`, `ITokenProvider`)
- **Exceptions**: `AmadeusException` for SDK-specific errors

### Pipeline Architecture
The SDK uses a pipeline-based architecture where HTTP requests flow through configurable policies:
1. Request enters the pipeline
2. Each policy processes the request in order
3. Request reaches the HTTP transport
4. Response flows back through policies in reverse order
5. Final response returned to client

### Common File Locations
```
src/AbeckDev.Amadeus/
├── AmadeusClient.cs           # Main client implementation
├── Configuration/             # Client configuration
│   └── AmadeusClientOptions.cs
├── Auth/                      # Authentication providers
│   └── BearerTokenProvider.cs
├── Pipeline/                  # HTTP pipeline infrastructure
│   ├── HttpPipeline.cs       # Core pipeline implementation
│   ├── PipelineContext.cs    # Request context
│   └── Policies/             # Pipeline policies
├── Abstractions/             # Core interfaces
└── Exceptions/               # Exception types
```

## Common Tasks

### Adding New Pipeline Policy
1. Implement `IHttpPipelinePolicy` interface
2. Add to `AmadeusClientOptions.AdditionalPolicies`
3. Write unit tests in `src/AbeckDev.Amadeus.Test/`
4. ALWAYS test with retry scenarios

### Modifying Authentication
- Primary authentication code in `src/AbeckDev.Amadeus/Auth/`
- Token provider interface in `src/AbeckDev.Amadeus/Abstractions/ITokenProvider.cs`
- ALWAYS test token renewal and error scenarios

### Adding Configuration Options
- Modify `AmadeusClientOptions` class
- Add corresponding unit tests in `ClientOptionTests.cs`
- Update documentation for public properties

### Working with Tests
- All tests use xUnit framework with Moq for mocking
- Test files follow pattern: `{ComponentName}Tests.cs`
- ALWAYS run full test suite after changes: `dotnet test`
- Tests cover: client options, pipeline policies, authentication, context management

## Build Information

### Solution Structure
```
amadeus-dotnet-reimagined.sln    # Main solution file
src/
├── AbeckDev.Amadeus/            # Main SDK project (.NET 8 library)
└── AbeckDev.Amadeus.Test/       # Test project (xUnit)
```

### Dependencies
- **Main SDK**: Microsoft.Extensions.Logging 9.0.8
- **Tests**: xUnit 2.5.3, Moq 4.20.72, Microsoft.NET.Test.Sdk 17.8.0

### Build Outputs
- Debug builds: `src/{Project}/bin/Debug/net8.0/`
- Release builds: `src/{Project}/bin/Release/net8.0/`

### Development Container
The repository includes `.devcontainer/devcontainer.json` for consistent development environment with:
- .NET 8.0 runtime
- Azure CLI
- VS Code with C# DevKit and GitHub Copilot extensions

## Troubleshooting

### Common Issues
1. **Build fails**: Run `dotnet restore` first
2. **Format errors**: Run `dotnet format` to auto-fix
3. **Test failures**: Check for missing packages or dependency issues
4. **Authentication errors**: Verify token provider configuration

### Performance Notes
- Restore: ~1 second (when packages cached), ~25 seconds (fresh downloads)
- Build: ~1.5 seconds (after restore)  
- Test: ~4 seconds (30 tests)
- Format: ~8 seconds

NEVER CANCEL these operations - they complete reliably within expected timeframes.

## Quick Reference Commands

### Essential Development Commands
```bash
# Complete development workflow
dotnet restore && dotnet build && dotnet test && dotnet format --verify-no-changes

# Clean build from scratch
dotnet clean && dotnet restore && dotnet build

# Run specific test class
dotnet test --filter "AmadeusClientTests"

# Build in Release mode
dotnet build -c Release
```

### Repository Overview
```
.
├── .devcontainer/           # VS Code dev container configuration
├── .github/                 # GitHub workflows and templates
├── src/
│   ├── AbeckDev.Amadeus/    # 📦 Main SDK library (20 source files)
│   └── AbeckDev.Amadeus.Test/ # 🧪 Unit tests (6 test classes, 30 tests)
├── amadeus-dotnet-reimagined.sln # Solution file
└── README.md               # Project documentation
```

### Common File Quick Access
These are frequently modified files during development:
- `src/AbeckDev.Amadeus/AmadeusClient.cs` - Main client implementation
- `src/AbeckDev.Amadeus/Configuration/AmadeusClientOptions.cs` - Client configuration
- `src/AbeckDev.Amadeus/Pipeline/Policies/` - HTTP pipeline policies
- `src/AbeckDev.Amadeus.Test/` - All unit tests

The SDK is currently in preview status and implements the core pipeline infrastructure with authentication, retry, logging, and telemetry policies.