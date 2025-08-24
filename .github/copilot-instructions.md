# Amadeus .NET SDK (Reimagined)

A modern, pipeline-based .NET 8 SDK for the Amadeus Self-Service travel APIs. This SDK provides a robust, extensible HTTP processing pipeline with authentication, retry policies, logging, and telemetry. The repository includes comprehensive CI/CD workflows, automated testing, code coverage reporting, and quality gates.

**ALWAYS reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Prerequisites
- .NET 8.0 SDK (8.0.119 or later)
- Visual Studio 2022, VS Code, or any .NET-compatible IDE
- No additional external dependencies or tools required

### Bootstrap, Build, and Test the Repository
```bash
# 1. Restore NuGet packages - takes ~4s (fresh) or ~1s (cached). NEVER CANCEL.
dotnet restore

# 2. Build the solution - takes ~1.5 seconds after restore. NEVER CANCEL.
dotnet build

# 3. Run all tests - takes ~1.6 seconds, runs 30 tests across 6 test classes. NEVER CANCEL.
dotnet test

# 4. Verify code formatting - takes ~8 seconds. NEVER CANCEL.
dotnet format --verify-no-changes
```

**CRITICAL TIMING**: Set timeouts of 180+ seconds for restore operations and 60+ seconds for other commands. NEVER CANCEL builds or tests.

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
- GitHub Actions automatically validate PRs with comprehensive CI/CD pipeline
- Code coverage reports are generated automatically for all PRs

## GitHub Actions & CI/CD

### Automated Workflows
The repository includes comprehensive GitHub Actions workflows for quality assurance:

#### 1. CI/CD Pipeline (`.github/workflows/ci.yml`)
Triggers on: `push` to main, `pull_request` to main, `release` published
- **Multi-platform builds**: Ubuntu and Windows with .NET 8.0.x
- **Build and test**: Full solution build with comprehensive testing
- **Code coverage**: XPlat Code Coverage collection, Codecov integration
- **Security analysis**: CodeQL static analysis for vulnerability detection
- **Test reporting**: dorny/test-reporter for detailed test result visualization
- **Package creation**: Automated NuGet package generation on main branch
- **Artifact management**: Upload packages with 30-day retention

#### 2. Pull Request Validation (`.github/workflows/pr.yml`)  
Triggers on: PR opened, synchronized, or reopened
- **Validation pipeline**: Build, test, and quality checks
- **Coverage reporting**: ReportGenerator for HTML/Markdown coverage reports
- **PR comments**: Automated coverage summary comments on PRs
- **Quality gates**: Comprehensive quality validation before merge
- **Test artifacts**: Upload test results and coverage reports

#### 3. Dependabot Auto-merge (`.github/workflows/dependabot.yml`)
Triggers on: Dependabot PRs
- **Automated validation**: Build and test Dependabot dependency updates
- **Auto-merge**: Successful dependency PRs are automatically merged
- **Safety checks**: Full test suite validation before merge

### Workflow Commands for Local Development
```bash
# Simulate CI build locally
dotnet clean && dotnet restore && dotnet build --configuration Release --no-restore

# Run tests with coverage (matches CI)
dotnet test --configuration Release --no-build --collect:"XPlat Code Coverage" --results-directory ./coverage

# Local quality validation
dotnet build --configuration Release && dotnet test --configuration Release --no-build
```

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
- GitHub Actions will automatically validate PRs with full CI/CD pipeline
- Code coverage reports are automatically generated and posted to PRs
- Security analysis via CodeQL runs on all changes

### Test Suite Overview
The repository includes **30 comprehensive tests** across **6 test classes**:

#### Test Classes & Coverage
1. **`AmadeusClientTests.cs`** (5 tests)
   - Client instantiation and configuration validation
   - Constructor parameter validation
   - Disposal and resource management
   
2. **`ClientOptionTests.cs`** (10 tests)
   - Configuration property validation
   - Policy management (add/remove policies)
   - Retry options configuration
   - Default value verification

3. **`ExceptionTests.cs`** (8 tests)
   - Exception hierarchy validation
   - Custom exception properties
   - Error response handling
   - Authentication exception scenarios

4. **`PipelineContextTests.cs`** (6 tests)
   - Request context management
   - Request ID generation and assignment
   - Item storage and retrieval
   - Attempt counter functionality

5. **`RetryPolicyTests.cs`** (1 test)
   - HTTP 500 retry behavior validation
   - Retry policy execution and timing

6. **`TokenProviderTests.cs`** (1 test)
   - OAuth2 token caching behavior
   - Token provider lifecycle management

#### Running Specific Test Categories
```bash
# Run all tests in a specific class
dotnet test --filter "AmadeusClientTests"
dotnet test --filter "ClientOptionTests"
dotnet test --filter "ExceptionTests"

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage collection
dotnet test --collect:"XPlat Code Coverage"
```

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
- All tests use **xUnit 2.9.3** framework with **Moq 4.20.72** for mocking
- Test files follow pattern: `{ComponentName}Tests.cs`
- **30 tests total** across 6 test classes with comprehensive coverage
- ALWAYS run full test suite after changes: `dotnet test`
- Tests cover: client options, pipeline policies, authentication, context management, exceptions, and retry behavior
- GitHub Actions automatically run tests on multiple platforms (Ubuntu/Windows)
- Code coverage is collected using XPlat Code Coverage and reported via Codecov
- Test results are automatically published to PRs via dorny/test-reporter

### Test Timing & Performance
- **Full test suite**: ~1.6 seconds (30 tests)
- **Individual test classes**: 10-860ms depending on complexity
- **Retry policy tests**: Longest running (~860ms) due to retry simulation
- **Exception tests**: Fastest (typically <1ms each)
- All tests are designed to be deterministic and reliable in CI environments

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
- **Tests**: xUnit 2.9.3, Moq 4.20.72, Microsoft.NET.Test.Sdk 17.8.0
- **CI/CD Tools**: 
  - actions/checkout@v5, actions/setup-dotnet@v4
  - codecov/codecov-action@v5, dorny/test-reporter@v2
  - danielpalme/ReportGenerator-GitHub-Action@5.4.12
  - github/codeql-action@v3

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
- **Restore**: ~4 seconds (fresh downloads), ~1 second (when packages cached)
- **Build**: ~1.5 seconds (after restore)  
- **Test**: ~1.6 seconds (30 tests across 6 classes)
- **Format**: ~8 seconds
- **CI Pipeline**: ~2-4 minutes for full multi-platform validation
- **Coverage Generation**: ~10-15 seconds additional for report generation

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

# Build in Release mode (matches CI)
dotnet build -c Release

# Run tests with coverage (matches CI pipeline)
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Validate PR readiness locally
dotnet clean && dotnet restore && dotnet build -c Release && dotnet test -c Release --no-build
```

### Repository Overview
```
.
├── .devcontainer/           # VS Code dev container configuration
├── .github/                 # GitHub workflows and Copilot instructions
│   ├── workflows/           # CI/CD automation
│   │   ├── ci.yml          # 🔄 Main CI/CD pipeline (build, test, security, package)
│   │   ├── pr.yml          # 🔍 PR validation (coverage, quality gates)
│   │   └── dependabot.yml  # 🤖 Automated dependency management
│   ├── copilot-instructions.md # 📋 This file - comprehensive development guide
│   └── dependabot.yml      # Dependabot configuration
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
- `src/AbeckDev.Amadeus.Test/` - All unit tests (6 classes, 30 tests)
- `.github/workflows/` - CI/CD pipeline configurations
- `.github/copilot-instructions.md` - This comprehensive development guide

### GitHub Actions Integration
The repository leverages GitHub Actions for:
- **Automated Testing**: Multi-platform test execution on every PR
- **Code Coverage**: Automatic coverage reporting with Codecov integration
- **Security Analysis**: CodeQL static analysis for vulnerability detection  
- **Quality Gates**: Comprehensive validation before merge approval
- **Dependency Management**: Automated Dependabot updates with validation
- **Package Management**: Automated NuGet package creation and versioning

The SDK is currently in preview status and implements the core pipeline infrastructure with authentication, retry, logging, and telemetry policies, backed by comprehensive automated testing and quality assurance.