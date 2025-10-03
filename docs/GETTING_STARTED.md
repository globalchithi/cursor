# Getting Started with API Test Framework

This guide will help you get up and running with the API Test Framework quickly.

## Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or any C# IDE
- Basic knowledge of C# and API testing concepts

## Step 1: Set Up Your Test Project

1. Create a new test project:
```bash
dotnet new nunit -n MyApiTests
cd MyApiTests
```

2. Add the framework reference:
```bash
dotnet add reference path/to/ApiTestFramework/src/ApiTestFramework/ApiTestFramework.csproj
```

3. Install additional packages if needed:
```bash
dotnet add package FluentAssertions
dotnet add package Microsoft.Extensions.Configuration.Json
```

## Step 2: Create Your First Test

Create a test class that inherits from `ApiTestBase`:

```csharp
using System.Net;
using ApiTestFramework.Assertions;
using ApiTestFramework.Utilities;
using NUnit.Framework;

namespace MyApiTests;

[TestFixture]
public class UserApiTests : ApiTestBase
{
    [Test]
    public async Task GetUsers_ShouldReturnSuccess()
    {
        // Arrange
        SetTestContext("Get Users Test");

        // Act
        var response = await ApiClient.GetAsync<object>("/users");

        // Assert
        response.Should()
            .BeSuccessful("API should return users successfully")
            .And.HaveStatusCode(HttpStatusCode.OK)
            .And.HaveResponseTimeWithin(TimeSpan.FromSeconds(5));

        LogInfo("Test completed successfully");
    }
}
```

## Step 3: Configure Your API Settings

Create `testsettings.json` in your test project:

```json
{
  "Environments": {
    "Development": {
      "BaseUrl": "https://jsonplaceholder.typicode.com",
      "TimeoutSeconds": 30,
      "DefaultHeaders": {
        "Accept": "application/json",
        "User-Agent": "MyApiTests/1.0"
      },
      "Logging": {
        "LogRequests": true,
        "LogResponses": true,
        "LogLevel": "Information"
      }
    }
  }
}
```

## Step 4: Run Your Tests

```bash
dotnet test
```

## Next Steps

- [Authentication Guide](AUTHENTICATION.md) - Learn about different authentication methods
- [Assertions Guide](ASSERTIONS.md) - Master the assertion capabilities
- [Configuration Guide](CONFIGURATION.md) - Advanced configuration options
- [Test Data Guide](TEST_DATA.md) - Managing test data effectively
- [Examples](../samples/) - Browse sample test implementations

## Common Patterns

### Testing CRUD Operations

```csharp
[Test]
public async Task UserCrudOperations_ShouldWorkEndToEnd()
{
    // Create
    var newUser = new { name = "John Doe", email = "john@example.com" };
    var createResponse = await ApiClient.PostAsync<User>("/users", newUser);
    createResponse.Should().BeSuccessful();
    var userId = createResponse.Data.Id;

    // Read
    var getResponse = await ApiClient.GetAsync<User>($"/users/{userId}");
    getResponse.Should().BeSuccessful();

    // Update
    var updateData = new { name = "Jane Doe" };
    var updateResponse = await ApiClient.PutAsync<User>($"/users/{userId}", updateData);
    updateResponse.Should().BeSuccessful();

    // Delete
    var deleteResponse = await ApiClient.DeleteAsync<object>($"/users/{userId}");
    deleteResponse.Should().HaveStatusCode(HttpStatusCode.NoContent);
}
```

### Error Handling Tests

```csharp
[Test]
public async Task GetUser_WithInvalidId_ShouldReturn404()
{
    var response = await ApiClient.GetAsync<object>("/users/999999");
    
    response.Should()
        .HaveStatusCode(HttpStatusCode.NotFound)
        .And.HaveContent();
}
```

### Performance Testing

```csharp
[Test]
public async Task GetUsers_ShouldRespondQuickly()
{
    var response = await ApiClient.GetAsync<object>("/users");
    
    response.Should()
        .BeSuccessful()
        .And.HaveResponseTimeWithin(TimeSpan.FromSeconds(2));
}
```

## Troubleshooting

### Common Issues

1. **Configuration not found**: Ensure `testsettings.json` is copied to output directory
2. **Authentication failures**: Check your API keys and authentication configuration
3. **Timeout errors**: Increase timeout values in configuration
4. **SSL/TLS errors**: Configure HttpClient to accept certificates if needed

### Debug Tips

- Enable detailed logging by setting `LogLevel` to `Debug`
- Use `LogInfo()`, `LogWarning()`, and `LogError()` methods in your tests
- Check the generated test reports for detailed information
- Use breakpoints and step through your test code

## Best Practices

1. **Use descriptive test names** that explain what is being tested
2. **Follow the AAA pattern** (Arrange, Act, Assert)
3. **Clean up test data** in teardown methods
4. **Use test categories** to organize your tests
5. **Keep tests independent** - don't rely on test execution order
6. **Use configuration files** for environment-specific settings
7. **Log important information** for debugging and reporting

