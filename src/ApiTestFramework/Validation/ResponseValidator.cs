using System.Net;
using ApiTestFramework.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace ApiTestFramework.Validation;

/// <summary>
/// Provides validation methods for API responses
/// </summary>
public static class ResponseValidator
{
    /// <summary>
    /// Validates that the response is successful
    /// </summary>
    /// <param name="response">API response</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateSuccess<T>(ApiResponse<T> response)
    {
        var result = new ValidationResult();
        
        if (!response.IsSuccessStatusCode)
        {
            result.AddError($"Expected successful status code (2xx), but got {response.StatusCode}");
        }
        
        if (response.Exception != null)
        {
            result.AddError($"Response contains exception: {response.Exception.Message}");
        }
        
        return result;
    }

    /// <summary>
    /// Validates response time
    /// </summary>
    /// <param name="response">API response</param>
    /// <param name="maxResponseTime">Maximum allowed response time</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateResponseTime<T>(ApiResponse<T> response, TimeSpan maxResponseTime)
    {
        var result = new ValidationResult();
        
        if (response.ResponseTime > maxResponseTime)
        {
            result.AddError($"Response time {response.ResponseTime.TotalMilliseconds}ms exceeds maximum allowed {maxResponseTime.TotalMilliseconds}ms");
        }
        
        return result;
    }

    /// <summary>
    /// Validates required headers
    /// </summary>
    /// <param name="response">API response</param>
    /// <param name="requiredHeaders">Required headers</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateHeaders<T>(ApiResponse<T> response, Dictionary<string, string?> requiredHeaders)
    {
        var result = new ValidationResult();
        
        foreach (var requiredHeader in requiredHeaders)
        {
            if (!response.Headers.ContainsKey(requiredHeader.Key))
            {
                result.AddError($"Missing required header: {requiredHeader.Key}");
                continue;
            }
            
            if (requiredHeader.Value != null)
            {
                var actualValue = response.GetHeader(requiredHeader.Key);
                if (actualValue != requiredHeader.Value)
                {
                    result.AddError($"Header {requiredHeader.Key} has value '{actualValue}', expected '{requiredHeader.Value}'");
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// Validates JSON schema
    /// </summary>
    /// <param name="response">API response</param>
    /// <param name="jsonSchema">JSON schema</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateJsonSchema<T>(ApiResponse<T> response, string jsonSchema)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrEmpty(response.RawContent))
        {
            result.AddError("Response content is empty, cannot validate schema");
            return result;
        }
        
        try
        {
            var schema = JSchema.Parse(jsonSchema);
            var json = JToken.Parse(response.RawContent);
            
            if (!json.IsValid(schema, out IList<string> errorMessages))
            {
                foreach (var error in errorMessages)
                {
                    result.AddError($"Schema validation error: {error}");
                }
            }
        }
        catch (Exception ex)
        {
            result.AddError($"Schema validation failed: {ex.Message}");
        }
        
        return result;
    }

    /// <summary>
    /// Validates that response contains specific JSON properties
    /// </summary>
    /// <param name="response">API response</param>
    /// <param name="requiredProperties">Required JSON properties</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateJsonProperties<T>(ApiResponse<T> response, params string[] requiredProperties)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrEmpty(response.RawContent))
        {
            result.AddError("Response content is empty, cannot validate properties");
            return result;
        }
        
        try
        {
            var json = JObject.Parse(response.RawContent);
            
            foreach (var property in requiredProperties)
            {
                var token = json.SelectToken(property);
                if (token == null)
                {
                    result.AddError($"Missing required property: {property}");
                }
            }
        }
        catch (Exception ex)
        {
            result.AddError($"JSON property validation failed: {ex.Message}");
        }
        
        return result;
    }

    /// <summary>
    /// Validates that response is a valid JSON array with expected count
    /// </summary>
    /// <param name="response">API response</param>
    /// <param name="expectedCount">Expected array count (null for any count)</param>
    /// <param name="minCount">Minimum array count</param>
    /// <param name="maxCount">Maximum array count</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateJsonArray<T>(ApiResponse<T> response, int? expectedCount = null, int? minCount = null, int? maxCount = null)
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrEmpty(response.RawContent))
        {
            result.AddError("Response content is empty, cannot validate array");
            return result;
        }
        
        try
        {
            var json = JToken.Parse(response.RawContent);
            
            if (json.Type != JTokenType.Array)
            {
                result.AddError($"Expected JSON array, but got {json.Type}");
                return result;
            }
            
            var array = (JArray)json;
            var count = array.Count;
            
            if (expectedCount.HasValue && count != expectedCount.Value)
            {
                result.AddError($"Expected array count {expectedCount.Value}, but got {count}");
            }
            
            if (minCount.HasValue && count < minCount.Value)
            {
                result.AddError($"Array count {count} is less than minimum {minCount.Value}");
            }
            
            if (maxCount.HasValue && count > maxCount.Value)
            {
                result.AddError($"Array count {count} exceeds maximum {maxCount.Value}");
            }
        }
        catch (Exception ex)
        {
            result.AddError($"JSON array validation failed: {ex.Message}");
        }
        
        return result;
    }

    /// <summary>
    /// Validates content type header
    /// </summary>
    /// <param name="response">API response</param>
    /// <param name="expectedContentType">Expected content type</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateContentType<T>(ApiResponse<T> response, string expectedContentType)
    {
        var result = new ValidationResult();
        
        var contentType = response.GetHeader("Content-Type");
        if (contentType == null)
        {
            result.AddError("Missing Content-Type header");
        }
        else if (!contentType.Contains(expectedContentType, StringComparison.OrdinalIgnoreCase))
        {
            result.AddError($"Expected Content-Type to contain '{expectedContentType}', but got '{contentType}'");
        }
        
        return result;
    }

    /// <summary>
    /// Validates that response indicates an error with appropriate status code and message
    /// </summary>
    /// <param name="response">API response</param>
    /// <param name="expectedStatusCode">Expected error status code</param>
    /// <param name="expectedErrorMessage">Expected error message (optional)</param>
    /// <returns>Validation result</returns>
    public static ValidationResult ValidateError<T>(ApiResponse<T> response, HttpStatusCode expectedStatusCode, string? expectedErrorMessage = null)
    {
        var result = new ValidationResult();
        
        if (response.StatusCode != expectedStatusCode)
        {
            result.AddError($"Expected status code {expectedStatusCode}, but got {response.StatusCode}");
        }
        
        if (!string.IsNullOrEmpty(expectedErrorMessage) && !string.IsNullOrEmpty(response.RawContent))
        {
            if (!response.RawContent.Contains(expectedErrorMessage, StringComparison.OrdinalIgnoreCase))
            {
                result.AddError($"Expected error message to contain '{expectedErrorMessage}', but response was: {response.RawContent}");
            }
        }
        
        return result;
    }
}

/// <summary>
/// Represents the result of a validation operation
/// </summary>
public class ValidationResult
{
    private readonly List<string> _errors = new();

    /// <summary>
    /// Gets whether the validation was successful
    /// </summary>
    public bool IsValid => !_errors.Any();

    /// <summary>
    /// Gets the validation errors
    /// </summary>
    public IReadOnlyList<string> Errors => _errors.AsReadOnly();

    /// <summary>
    /// Adds an error to the validation result
    /// </summary>
    /// <param name="error">Error message</param>
    public void AddError(string error)
    {
        _errors.Add(error);
    }

    /// <summary>
    /// Adds multiple errors to the validation result
    /// </summary>
    /// <param name="errors">Error messages</param>
    public void AddErrors(IEnumerable<string> errors)
    {
        _errors.AddRange(errors);
    }

    /// <summary>
    /// Throws an exception if validation failed
    /// </summary>
    public void ThrowIfInvalid()
    {
        if (!IsValid)
        {
            throw new ValidationException($"Validation failed: {string.Join(", ", _errors)}");
        }
    }

    /// <summary>
    /// Gets a formatted error message
    /// </summary>
    /// <returns>Formatted error message</returns>
    public override string ToString()
    {
        return IsValid ? "Validation successful" : $"Validation failed: {string.Join("; ", _errors)}";
    }
}

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}

