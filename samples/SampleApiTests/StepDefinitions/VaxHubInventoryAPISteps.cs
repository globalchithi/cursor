using System.Net;
using ApiTestFramework.Core;
using ApiTestFramework.Models;
using FluentAssertions;
using TechTalk.SpecFlow;

namespace SampleApiTests.StepDefinitions;

[Binding]
public class VaxHubInventoryAPISteps : SpecFlowTestBase
{
    private ApiResponse<VaxHubInventoryProductResponse> _apiResponse = null!;
    private string _endpoint = null!;
    private TimeSpan _responseTime;
    private Dictionary<string, string> _customHeaders = null!;
    private Dictionary<string, string> _vaxHubHeaders = null!;

    // Inherit logger from base class
    protected ILogger Logger => base.Logger;

    [Given(@"I have proxy configured for ""(.*)"" on port (\d+)")]
    public void GivenIHaveProxyConfiguredForOnPort(string host, int port)
    {
        // Proxy configuration is handled by the TestConfiguration
        // The proxy settings should be configured in appsettings.json or via environment variables
        LogInfo("Proxy configured for {0}:{1}", host, port);
    }

    [Given(@"I have VaxHub mobile headers configured for inventory API")]
    public void GivenIHaveVaxHubMobileHeadersConfiguredForInventoryAPI()
    {
        _vaxHubHeaders = new Dictionary<string, string>
        {
            ["X-VaxHub-Identifier"] = "eyJhbmRyb2lkU2RrIjoyOSwiYW5kcm9pZFZlcnNpb24iOiIxMCIsImFzc2V0VGFnIjotMSwiY2xpbmljSWQiOjg5NTM0LCJkZXZpY2VTZXJpYWxOdW1iZXIiOiJOT19QRVJNSVNTSU9OIiwicGFydG5lcklkIjoxNzg3NjQsInVzZXJJZCI6MCwidXNlck5hbWUiOiAiIiwidmVyc2lvbiI6MTQsInZlcnNpb25OYW1lIjoiMy4wLjAtMC1TVEciLCJtb2RlbFR5cGUiOiJNb2JpbGVIdWIifQ==",
            ["MobileData"] = "false",
            ["UserSessionId"] = "NO USER LOGGED IN",
            ["MessageSource"] = "VaxMobile",
            ["Host"] = "vhapistg.vaxcare.com",
            ["Connection"] = "Keep-Alive",
            ["User-Agent"] = "okhttp/4.12.0"
        };

        LogInfo("VaxHub mobile headers configured for inventory API");
    }

    [Given(@"I have the following request headers:")]
    public void GivenIHaveTheFollowingRequestHeaders(Table table)
    {
        _customHeaders = new Dictionary<string, string>();

        foreach (var row in table.Rows)
        {
            var headerName = row["HeaderName"];
            var headerValue = row["HeaderValue"];
            _customHeaders[headerName] = headerValue;
        }

        LogInfo("Custom request headers configured: {0}", string.Join(", ", _customHeaders.Keys));
    }

    [When(@"I send a GET request to ""(.*)""")]
    public async Task WhenISendAGetRequestTo(string endpoint)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            // Create a custom API client with VaxHub headers and proxy
            var customApiClient = CreateApiClientWithVaxHubHeadersAndProxy();

            // Combine VaxHub headers with custom headers
            var allHeaders = new Dictionary<string, string>(_vaxHubHeaders);
            if (_customHeaders != null)
            {
                foreach (var header in _customHeaders)
                {
                    allHeaders[header.Key] = header.Value;
                }
            }

            _apiResponse = await customApiClient.GetAsync<VaxHubInventoryProductResponse>(endpoint, allHeaders);
            _responseTime = DateTime.UtcNow - startTime;

            LogInfo("GET request sent to {0}", endpoint);
            LogInfo("Response Status: {0}", _apiResponse.StatusCode);
            LogInfo("Response Time: {0}ms", _responseTime.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _responseTime = DateTime.UtcNow - startTime;
            LogError("GET request failed: {0}", ex.Message);
            throw;
        }
    }

    [Then(@"the VaxHub response status should be (\d+) (.+)")]
    public void ThenTheVaxHubResponseStatusShouldBe(int expectedStatusCode, string expectedStatusName)
    {
        var expectedHttpStatusCode = (HttpStatusCode)expectedStatusCode;
        _apiResponse.StatusCode.Should().Be(expectedHttpStatusCode,
            $"expected status code {expectedStatusCode} ({expectedStatusName})");

        LogInfo("Response status verified: {0} {1}", expectedStatusCode, expectedStatusName);
    }

    [Then(@"the VaxHub response time should be less than (\d+) seconds")]
    public void ThenTheVaxHubResponseTimeShouldBeLessThanSeconds(int maxSeconds)
    {
        var maxTimeSpan = TimeSpan.FromSeconds(maxSeconds);
        _responseTime.Should().BeLessThan(maxTimeSpan,
            $"response time should be less than {maxSeconds} seconds");

        LogInfo("Response time verified: {0}ms (max: {1}ms)",
            _responseTime.TotalMilliseconds, maxTimeSpan.TotalMilliseconds);
    }

    [Then(@"the response should contain inventory product data")]
    public void ThenTheResponseShouldContainInventoryProductData()
    {
        _apiResponse.Data.Should().NotBeNull("response should contain inventory product data");

        LogInfo("Inventory product data verified in response");
    }

    // Helper methods
    private IApiClient CreateApiClientWithVaxHubHeadersAndProxy()
    {
        // Filter out Content-Type from headers as it should be set on HttpContent
        var filteredHeaders = new Dictionary<string, string>(_vaxHubHeaders);
        filteredHeaders.Remove("Content-Type");

        // Create a custom configuration with VaxHub headers and proxy
        var customConfig = new TestConfiguration
        {
            BaseUrl = Configuration.BaseUrl,
            TimeoutSeconds = Configuration.TimeoutSeconds,
            DefaultHeaders = filteredHeaders,
            Authentication = Configuration.Authentication,
            Retry = Configuration.Retry,
            Logging = Configuration.Logging,
            Proxy = Configuration.Proxy, // This should include the proxy configuration
            Environment = Configuration.Environment
        };

        // Create a properly typed logger for ApiClient using the inherited logger
        var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
        });

        var apiClientLogger = loggerFactory.CreateLogger<ApiClient>();

        return new ApiClient(customConfig, apiClientLogger);
    }
}

/// <summary>
/// VaxHub Inventory Product Response Model
/// </summary>
public class VaxHubInventoryProductResponse
{
    public List<VaxHubInventoryProduct>? Products { get; set; }
    public int TotalCount { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// VaxHub Inventory Product Model
/// </summary>
public class VaxHubInventoryProduct
{
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; }
}
