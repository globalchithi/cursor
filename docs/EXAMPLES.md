# API Test Framework Examples

This document provides comprehensive examples of using the API Test Framework for various testing scenarios.

## Basic API Testing

### Simple GET Request

```csharp
[Test]
public async Task GetUsers_ShouldReturnUserList()
{
    var response = await ApiClient.GetAsync<List<User>>("/api/users");
    
    response.Should()
        .BeSuccessful()
        .And.HaveStatusCode(HttpStatusCode.OK)
        .And.HaveData();
        
    response.Data.Should().NotBeEmpty();
}
```

### POST Request with Body

```csharp
[Test]
public async Task CreateUser_WithValidData_ShouldReturnCreatedUser()
{
    var newUser = new User
    {
        Name = "John Doe",
        Email = "john.doe@example.com",
        IsActive = true
    };
    
    var response = await ApiClient.PostAsync<User>("/api/users", newUser);
    
    response.Should()
        .BeSuccessful()
        .And.HaveStatusCode(HttpStatusCode.Created)
        .And.HaveData();
        
    response.Data.Id.Should().BeGreaterThan(0);
    response.Data.Email.Should().Be(newUser.Email);
}
```

## Authentication Examples

### API Key Authentication

```csharp
public class ApiKeyTests : ApiTestBase
{
    protected override async Task OnInitialize()
    {
        var authConfig = new AuthenticationConfig
        {
            Type = AuthenticationType.ApiKey,
            ApiKey = "your-api-key-here",
            ApiKeyHeader = "X-API-Key"
        };
        
        await ApiClient.SetAuthenticationAsync(authConfig);
    }
    
    [Test]
    public async Task GetProtectedResource_WithValidApiKey_ShouldSucceed()
    {
        var response = await ApiClient.GetAsync<object>("/api/protected");
        response.Should().BeSuccessful();
    }
}
```

### Bearer Token Authentication

```csharp
[Test]
public async Task AccessProtectedEndpoint_WithBearerToken_ShouldSucceed()
{
    // Set bearer token
    var authConfig = new AuthenticationConfig
    {
        Type = AuthenticationType.Bearer,
        BearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    };
    
    await ApiClient.SetAuthenticationAsync(authConfig);
    
    var response = await ApiClient.GetAsync<object>("/api/user/profile");
    
    response.Should()
        .BeSuccessful()
        .And.HaveHeader("Content-Type", "application/json");
}
```

## Validation and Error Handling

### Input Validation Testing

```csharp
[Test]
public async Task CreateUser_WithInvalidEmail_ShouldReturnValidationError()
{
    var invalidUser = new User
    {
        Name = "Test User",
        Email = "invalid-email-format",
        IsActive = true
    };
    
    var response = await ApiClient.PostAsync<object>("/api/users", invalidUser);
    
    response.Should()
        .HaveStatusCode(HttpStatusCode.BadRequest)
        .And.HaveValidationError("Email");
}
```

### Error Response Testing

```csharp
[Test]
public async Task GetUser_WithNonExistentId_ShouldReturn404()
{
    var response = await ApiClient.GetAsync<object>("/api/users/999999");
    
    response.Should()
        .HaveStatusCode(HttpStatusCode.NotFound)
        .And.HaveContentContaining("User not found");
}
```

## Advanced Assertions

### JSON Property Validation

```csharp
[Test]
public async Task GetUser_ShouldHaveRequiredProperties()
{
    var response = await ApiClient.GetAsync<object>("/api/users/1");
    
    response.Should()
        .BeSuccessful()
        .And.HaveJsonProperty("id")
        .And.HaveJsonProperty("name")
        .And.HaveJsonProperty("email")
        .And.HaveJsonPropertyValue("id", 1);
}
```

### Array Response Validation

```csharp
[Test]
public async Task GetUsers_ShouldReturnPaginatedResults()
{
    var response = await ApiClient.GetAsync<object>("/api/users?page=1&limit=10");
    
    response.Should()
        .BeSuccessful()
        .And.HaveJsonArrayCount(10)
        .And.HaveJsonProperty("[0].id")
        .And.HaveJsonProperty("[0].name");
}
```

## Test Data Management

### Using JSON Test Data

```csharp
[Test]
public async Task CreateUser_WithTestData_ShouldSucceed()
{
    // Load test data from JSON file
    var userData = TestData.GetData<User>("validUser");
    
    var response = await ApiClient.PostAsync<User>("/api/users", userData);
    
    response.Should().BeSuccessful();
    response.Data.Email.Should().Be(userData.Email);
}
```

### Generated Test Data

```csharp
[Test]
public async Task CreateMultipleUsers_WithGeneratedData_ShouldSucceed()
{
    for (int i = 0; i < 5; i++)
    {
        var user = TestDataGenerator.Generate<User>();
        
        var response = await ApiClient.PostAsync<User>("/api/users", user);
        
        response.Should().BeSuccessful();
        
        // Clean up
        await ApiClient.DeleteAsync<object>($"/api/users/{response.Data.Id}");
    }
}
```

## Performance Testing

### Response Time Testing

```csharp
[Test]
[ApiTestCategory(TestCategories.Performance)]
public async Task GetUsers_ShouldRespondWithinTimeLimit()
{
    var response = await ApiClient.GetAsync<List<User>>("/api/users");
    
    response.Should()
        .BeSuccessful()
        .And.HaveResponseTimeWithin(TimeSpan.FromSeconds(2));
}
```

### Load Testing Simulation

```csharp
[Test]
public async Task GetUsers_ConcurrentRequests_ShouldHandleLoad()
{
    var tasks = new List<Task<ApiResponse<List<User>>>>();
    
    // Create 10 concurrent requests
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(ApiClient.GetAsync<List<User>>("/api/users"));
    }
    
    var responses = await Task.WhenAll(tasks);
    
    // All requests should succeed
    foreach (var response in responses)
    {
        response.Should().BeSuccessful();
    }
    
    // Average response time should be reasonable
    var avgResponseTime = responses.Average(r => r.ResponseTime.TotalMilliseconds);
    avgResponseTime.Should().BeLessThan(5000); // 5 seconds
}
```

## Integration Testing

### End-to-End Workflow

```csharp
[Test]
[ApiTestCategory(TestCategories.Integration)]
public async Task UserRegistrationWorkflow_ShouldWorkEndToEnd()
{
    // 1. Register new user
    var registrationData = new
    {
        Username = TestDataFactory.RandomString(10),
        Email = TestDataFactory.RandomEmail(),
        Password = "SecurePassword123!"
    };
    
    var registerResponse = await ApiClient.PostAsync<object>("/api/auth/register", registrationData);
    registerResponse.Should().BeSuccessful();
    
    // 2. Login with new user
    var loginData = new
    {
        Email = registrationData.Email,
        Password = registrationData.Password
    };
    
    var loginResponse = await ApiClient.PostAsync<LoginResult>("/api/auth/login", loginData);
    loginResponse.Should().BeSuccessful();
    
    // 3. Use token to access protected resource
    await ApiClient.SetAuthenticationAsync(new AuthenticationConfig
    {
        Type = AuthenticationType.Bearer,
        BearerToken = loginResponse.Data.Token
    });
    
    var profileResponse = await ApiClient.GetAsync<UserProfile>("/api/user/profile");
    profileResponse.Should().BeSuccessful();
    
    // 4. Logout
    var logoutResponse = await ApiClient.PostAsync<object>("/api/auth/logout");
    logoutResponse.Should().BeSuccessful();
}
```

## Custom Validation

### Business Rule Validation

```csharp
[Test]
public async Task CreateOrder_ShouldValidateBusinessRules()
{
    var order = new Order
    {
        CustomerId = 1,
        Items = new List<OrderItem>
        {
            new() { ProductId = 1, Quantity = 2, Price = 10.00m },
            new() { ProductId = 2, Quantity = 1, Price = 15.00m }
        }
    };
    
    var response = await ApiClient.PostAsync<Order>("/api/orders", order);
    
    response.Should().BeSuccessful();
    
    // Custom validation
    response.Should().SatisfyData(data =>
    {
        data.TotalAmount.Should().Be(35.00m); // 2*10 + 1*15
        data.Status.Should().Be(OrderStatus.Pending);
        data.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    });
}
```

## Error Scenarios

### Network Error Simulation

```csharp
[Test]
public async Task ApiCall_WithNetworkError_ShouldRetryAndFail()
{
    // Configure to use invalid URL to simulate network error
    var originalBaseUrl = Configuration.BaseUrl;
    Configuration.BaseUrl = "https://invalid-url-that-does-not-exist.com";
    
    try
    {
        var response = await ApiClient.GetAsync<object>("/api/users");
        
        response.Should().NotHaveException("Framework should handle network errors gracefully");
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
    finally
    {
        Configuration.BaseUrl = originalBaseUrl;
    }
}
```

### Rate Limiting Testing

```csharp
[Test]
public async Task ApiCall_ExceedingRateLimit_ShouldHandle429()
{
    // Make rapid requests to trigger rate limiting
    var tasks = new List<Task<ApiResponse<object>>>();
    
    for (int i = 0; i < 100; i++)
    {
        tasks.Add(ApiClient.GetAsync<object>("/api/users"));
    }
    
    var responses = await Task.WhenAll(tasks);
    
    // Some responses might be rate limited
    var rateLimitedResponses = responses.Where(r => r.StatusCode == HttpStatusCode.TooManyRequests);
    
    if (rateLimitedResponses.Any())
    {
        LogInfo($"Rate limiting detected: {rateLimitedResponses.Count()} requests were throttled");
        
        foreach (var response in rateLimitedResponses)
        {
            response.Should().HaveHeader("Retry-After");
        }
    }
}
```

## Cleanup and Teardown

### Automatic Cleanup

```csharp
public class UserManagementTests : ApiTestBase
{
    private readonly List<int> _createdUserIds = new();
    
    [Test]
    public async Task CreateUser_ShouldTrackForCleanup()
    {
        var user = TestDataGenerator.Generate<User>();
        var response = await ApiClient.PostAsync<User>("/api/users", user);
        
        response.Should().BeSuccessful();
        
        // Track for cleanup
        _createdUserIds.Add(response.Data.Id);
    }
    
    protected override async Task OnTearDown()
    {
        // Clean up all created users
        foreach (var userId in _createdUserIds)
        {
            try
            {
                await ApiClient.DeleteAsync<object>($"/api/users/{userId}");
                LogInfo($"Cleaned up user {userId}");
            }
            catch (Exception ex)
            {
                LogWarning($"Failed to clean up user {userId}: {ex.Message}");
            }
        }
        
        _createdUserIds.Clear();
    }
}
```

## Configuration Examples

### Environment-Specific Tests

```csharp
[TestFixture]
[TestEnvironment("Production")]
public class ProductionSmokeTests : ApiTestBase
{
    [Test]
    [ApiTestCategory(TestCategories.Smoke)]
    public async Task HealthCheck_ShouldReturnHealthy()
    {
        var response = await ApiClient.GetAsync<HealthStatus>("/health");
        
        response.Should()
            .BeSuccessful()
            .And.HaveResponseTimeWithin(TimeSpan.FromSeconds(5));
            
        response.Data.Status.Should().Be("Healthy");
    }
}
```

These examples demonstrate the flexibility and power of the API Test Framework. You can combine these patterns to create comprehensive test suites that cover all aspects of your API testing needs.

