using ApiTestFramework.Models;

namespace ApiTestFramework.Core;

/// <summary>
/// Interface for API client operations
/// </summary>
public interface IApiClient : IDisposable
{
    /// <summary>
    /// Performs a GET request
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="headers">Additional headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a POST request
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="body">Request body</param>
    /// <param name="headers">Additional headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a POST request with file content
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="filePath">Path to the file containing the request body</param>
    /// <param name="headers">Additional headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    Task<ApiResponse<T>> PostAsyncWithFile<T>(string endpoint, string filePath, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a PUT request
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="body">Request body</param>
    /// <param name="headers">Additional headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a PATCH request
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="body">Request body</param>
    /// <param name="headers">Additional headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a DELETE request
    /// </summary>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="headers">Additional headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a custom HTTP request
    /// </summary>
    /// <param name="method">HTTP method</param>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="body">Request body</param>
    /// <param name="headers">Additional headers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>API response</returns>
    Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets authentication for subsequent requests
    /// </summary>
    /// <param name="authConfig">Authentication configuration</param>
    Task SetAuthenticationAsync(AuthenticationConfig authConfig);

    /// <summary>
    /// Adds a default header for all requests
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    void AddDefaultHeader(string name, string value);

    /// <summary>
    /// Removes a default header
    /// </summary>
    /// <param name="name">Header name</param>
    void RemoveDefaultHeader(string name);
}

