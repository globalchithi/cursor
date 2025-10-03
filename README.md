# API Test Framework for C#

A comprehensive, feature-rich API testing framework built in C# that provides everything you need to create robust, maintainable API tests.

## 🚀 Features

- **HTTP Client Wrapper**: Full-featured HTTP client with authentication, retry logic, and comprehensive logging
- **Multiple Authentication Types**: Support for API Key, Bearer Token, Basic Auth, and OAuth2
- **Fluent Assertions**: Intuitive, readable assertions for API responses using FluentAssertions
- **Response Validation**: Built-in validators for common API response patterns
- **Configuration Management**: Environment-based configuration with JSON and environment variable support
- **Test Data Management**: Flexible test data handling with JSON files and automatic generation
- **Comprehensive Logging**: Structured logging with Serilog integration
- **Performance Tracking**: Built-in performance monitoring and reporting
- **Test Reporting**: HTML, JSON, and CSV report generation
- **Retry Logic**: Configurable retry mechanisms with exponential backoff
- **Base Test Classes**: Ready-to-use base classes that handle setup and teardown

## 📦 Installation

### Using the Framework

1. Clone this repository:
```bash
git clone <repository-url>
cd ApiTestFramework
```

2. Build the solution:
```bash
dotnet build
```

3. Run the sample tests:
```bash
dotnet test samples/SampleApiTests
```

### Adding to Your Project

Reference the framework in your test project:

```xml
<ProjectReference Include="path/to/ApiTestFramework/src/ApiTestFramework/ApiTestFramework.csproj" />
```

## 🏗️ Project Structure

```
ApiTestFramework/
├── src/
│   └── ApiTestFramework/           # Main framework library
│       ├── Core/                   # Core HTTP client and interfaces
│       ├── Models/                 # Data models and configuration
│       ├── Assertions/             # Fluent assertion extensions
│       ├── Validation/             # Response validators
│       ├── Configuration/          # Configuration management
│       ├── TestData/              # Test data management
│       ├── Utilities/             # Base classes and utilities
│       ├── Logging/               # Logging infrastructure
│       └── Reporting/             # Test reporting
├── tests/
│   └── ApiTestFramework.Tests/    # Framework unit tests
├── samples/
│   └── SampleApiTests/            # Example API tests
└── README.md
```

## 🚀 Quick Start

### 1. Create a Test Class

```csharp
using ApiTestFramework.Utilities;
using ApiTestFramework.Assertions;
using NUnit.Framework;

[TestFixture]
public class MyApiTests : ApiTestBase
{
    [Test]
    public async Task GetUsers_ShouldReturnUserList()
    {
        // Act
        var response = await ApiClient.GetAsync<List<User>>("/api/users");

        // Assert
        response.Should()
            .BeSuccessful()
            .And.HaveStatusCode(HttpStatusCode.OK)
            .And.HaveResponseTimeWithin(TimeSpan.FromSeconds(2))
            .And.HaveData();
    }
}
```

### 2. Configure Your Environment

Create `appsettings.json` or `testsettings.json`:

```json
{
  "Environments": {
    "Development": {
      "BaseUrl": "https://api.example.com",
      "TimeoutSeconds": 30,
      "Authentication": {
        "Type": "ApiKey",
        "ApiKey": "your-api-key",
        "ApiKeyHeader": "X-API-Key"
      }
    }
  }
}
```

### 3. Run Your Tests

```bash
dotnet test --environment Development
```

For complete documentation, examples, and advanced usage, see the [Documentation](docs/) folder.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License.