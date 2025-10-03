using System.Net;
using ApiTestFramework.Models;
using FluentAssertions;
using Newtonsoft.Json.Linq;

namespace ApiTestFramework.Assertions;

/// <summary>
/// Fluent assertions for API responses
/// </summary>
public static class ApiAssertions
{
    /// <summary>
    /// Creates assertion context for an API response
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="response">API response</param>
    /// <returns>Response assertions</returns>
    public static ResponseAssertions<T> Should<T>(this ApiResponse<T> response)
    {
        return new ResponseAssertions<T>(response);
    }
}

/// <summary>
/// Assertion methods for API responses
/// </summary>
/// <typeparam name="T">Response type</typeparam>
public class ResponseAssertions<T>
{
    private readonly ApiResponse<T> _response;

    public ResponseAssertions(ApiResponse<T> response)
    {
        _response = response ?? throw new ArgumentNullException(nameof(response));
    }

    /// <summary>
    /// Allows chaining of assertions
    /// </summary>
    public ResponseAssertions<T> And => this;

    /// <summary>
    /// Asserts that the response has a successful status code (2xx)
    /// </summary>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> BeSuccessful(string because = "")
    {
        _response.IsSuccessStatusCode.Should().BeTrue(because);
        return this;
    }

    /// <summary>
    /// Asserts that the response has a specific status code
    /// </summary>
    /// <param name="expectedStatusCode">Expected status code</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveStatusCode(HttpStatusCode expectedStatusCode, string because = "")
    {
        _response.StatusCode.Should().Be(expectedStatusCode, because);
        return this;
    }

    /// <summary>
    /// Asserts that the response has a specific status code
    /// </summary>
    /// <param name="expectedStatusCode">Expected status code as integer</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveStatusCode(int expectedStatusCode, string because = "")
    {
        ((int)_response.StatusCode).Should().Be(expectedStatusCode, because);
        return this;
    }

    /// <summary>
    /// Asserts that the response contains a specific header
    /// </summary>
    /// <param name="headerName">Header name</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveHeader(string headerName, string because = "")
    {
        _response.Headers.Should().ContainKey(headerName, because);
        return this;
    }

    /// <summary>
    /// Asserts that the response contains a specific header with a specific value
    /// </summary>
    /// <param name="headerName">Header name</param>
    /// <param name="expectedValue">Expected header value</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveHeader(string headerName, string expectedValue, string because = "")
    {
        _response.Headers.Should().ContainKey(headerName, because);
        var headerValue = _response.GetHeader(headerName);
        headerValue.Should().Be(expectedValue, because);
        return this;
    }

    /// <summary>
    /// Asserts that the response time is within a specific duration
    /// </summary>
    /// <param name="maxDuration">Maximum allowed duration</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveResponseTimeWithin(TimeSpan maxDuration, string because = "")
    {
        _response.ResponseTime.Should().BeLessOrEqualTo(maxDuration, because);
        return this;
    }

    /// <summary>
    /// Asserts that the response has content
    /// </summary>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveContent(string because = "")
    {
        _response.RawContent.Should().NotBeNullOrEmpty(because);
        return this;
    }

    /// <summary>
    /// Asserts that the response content contains a specific string
    /// </summary>
    /// <param name="expectedContent">Expected content</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveContentContaining(string expectedContent, string because = "")
    {
        _response.RawContent.Should().Contain(expectedContent, because);
        return this;
    }

    /// <summary>
    /// Asserts that the response data is not null
    /// </summary>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveData(string because = "")
    {
        _response.Data.Should().NotBeNull(because);
        return this;
    }

    /// <summary>
    /// Asserts that the response data matches a specific value
    /// </summary>
    /// <param name="expectedData">Expected data</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveData(T expectedData, string because = "")
    {
        _response.Data.Should().BeEquivalentTo(expectedData, because);
        return this;
    }

    /// <summary>
    /// Asserts that the response has no exception
    /// </summary>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> NotHaveException(string because = "")
    {
        _response.Exception.Should().BeNull(because);
        return this;
    }

    /// <summary>
    /// Provides access to the response data for custom assertions
    /// </summary>
    /// <param name="assertion">Custom assertion action</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> SatisfyData(Action<T?> assertion)
    {
        assertion(_response.Data);
        return this;
    }

    /// <summary>
    /// Validates JSON schema against the response content
    /// </summary>
    /// <param name="schemaValidator">Schema validation function</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> MatchJsonSchema(Func<string, bool> schemaValidator, string because = "")
    {
        _response.RawContent.Should().NotBeNullOrEmpty("response should have content for schema validation");
        schemaValidator(_response.RawContent!).Should().BeTrue(because);
        return this;
    }

    /// <summary>
    /// Asserts that a JSON property exists in the response
    /// </summary>
    /// <param name="propertyPath">JSON property path (e.g., "data.user.id")</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveJsonProperty(string propertyPath, string because = "")
    {
        _response.RawContent.Should().NotBeNullOrEmpty("response should have content");
        
        var json = JObject.Parse(_response.RawContent!);
        var token = json.SelectToken(propertyPath);
        token.Should().NotBeNull(because);
        return this;
    }

    /// <summary>
    /// Asserts that a JSON property has a specific value
    /// </summary>
    /// <param name="propertyPath">JSON property path</param>
    /// <param name="expectedValue">Expected value</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveJsonPropertyValue(string propertyPath, object expectedValue, string because = "")
    {
        _response.RawContent.Should().NotBeNullOrEmpty("response should have content");
        
        var json = JObject.Parse(_response.RawContent!);
        var token = json.SelectToken(propertyPath);
        token.Should().NotBeNull($"property '{propertyPath}' should exist");
        
        object? actualValue = token!.Type switch
        {
            JTokenType.String => token.Value<string>(),
            JTokenType.Integer => token.Value<long>(),
            JTokenType.Float => token.Value<double>(),
            JTokenType.Boolean => token.Value<bool>(),
            _ => token.ToString()
        };
        
        actualValue.Should().Be(expectedValue, because);
        return this;
    }

    /// <summary>
    /// Asserts that the response is a JSON array with a specific count
    /// </summary>
    /// <param name="expectedCount">Expected array count</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveJsonArrayCount(int expectedCount, string because = "")
    {
        _response.RawContent.Should().NotBeNullOrEmpty("response should have content");
        
        var json = JToken.Parse(_response.RawContent!);
        json.Should().BeOfType<JArray>("response should be a JSON array");
        
        var array = (JArray)json;
        array.Count.Should().Be(expectedCount, because);
        return this;
    }

    /// <summary>
    /// Asserts that the response contains validation errors
    /// </summary>
    /// <param name="fieldName">Field name that should have validation error</param>
    /// <param name="because">Reason for the assertion</param>
    /// <returns>Current assertion instance</returns>
    public ResponseAssertions<T> HaveValidationError(string fieldName, string because = "")
    {
        _response.StatusCode.Should().Be(HttpStatusCode.BadRequest, "validation errors typically return 400 Bad Request");
        
        // Common validation error patterns
        var hasError = _response.RawContent?.Contains(fieldName) == true ||
                      _response.GetDataAs<Dictionary<string, object>>()?.ContainsKey(fieldName) == true;
        
        hasError.Should().BeTrue(because);
        return this;
    }
}
