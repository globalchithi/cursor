using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ApiTestFramework.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ApiTestFramework.Core;

/// <summary>
/// HTTP client wrapper for API testing with authentication and retry support
/// </summary>
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TestConfiguration _configuration;
    private readonly ILogger<ApiClient> _logger;
    private readonly Dictionary<string, string> _defaultHeaders;
    private bool _disposed;

    public ApiClient(TestConfiguration configuration, ILogger<ApiClient> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultHeaders = new Dictionary<string, string>(configuration.DefaultHeaders);

        var handler = new HttpClientHandler();
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(configuration.BaseUrl),
            Timeout = TimeSpan.FromSeconds(configuration.TimeoutSeconds)
        };

        // Set default headers
        foreach (var header in _defaultHeaders)
        {
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        // Set authentication if configured
        if (configuration.Authentication != null)
        {
            SetAuthenticationAsync(configuration.Authentication).Wait();
        }
    }

    public async Task<ApiResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync<T>(HttpMethod.Get, endpoint, null, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync<T>(HttpMethod.Post, endpoint, body, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync<T>(HttpMethod.Put, endpoint, body, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync<T>(HttpMethod.Patch, endpoint, body, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        return await SendAsync<T>(HttpMethod.Delete, endpoint, null, headers, cancellationToken);
    }

    public async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string endpoint, object? body = null, Dictionary<string, string>? headers = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var retryCount = 0;
        var maxRetries = _configuration.Retry.MaxRetries;

        while (retryCount <= maxRetries)
        {
            try
            {
                using var request = CreateHttpRequestMessage(method, endpoint, body, headers);
                
                if (_configuration.Logging.LogRequests)
                {
                    LogRequest(request, body);
                }

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                stopwatch.Stop();

                var apiResponse = await CreateApiResponse<T>(response, stopwatch.Elapsed);

                if (_configuration.Logging.LogResponses)
                {
                    LogResponse(apiResponse);
                }

                // Check if we should retry
                if (ShouldRetry<T>(apiResponse, retryCount, maxRetries))
                {
                    retryCount++;
                    var delay = CalculateRetryDelay(retryCount);
                    _logger.LogWarning("Request failed with status {StatusCode}. Retrying in {Delay}ms (attempt {RetryCount}/{MaxRetries})", 
                        apiResponse.StatusCode, delay, retryCount, maxRetries);
                    
                    await Task.Delay(delay, cancellationToken);
                    stopwatch.Restart();
                    continue;
                }

                return apiResponse;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                if (retryCount < maxRetries)
                {
                    retryCount++;
                    var delay = CalculateRetryDelay(retryCount);
                    _logger.LogWarning(ex, "Request failed with exception. Retrying in {Delay}ms (attempt {RetryCount}/{MaxRetries})", 
                        delay, retryCount, maxRetries);
                    
                    await Task.Delay(delay, cancellationToken);
                    stopwatch.Restart();
                    continue;
                }

                _logger.LogError(ex, "Request failed after {MaxRetries} retries", maxRetries);
                return new ApiResponse<T>
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Exception = ex,
                    ResponseTime = stopwatch.Elapsed
                };
            }
        }

        // This should never be reached, but just in case
        return new ApiResponse<T>
        {
            StatusCode = HttpStatusCode.InternalServerError,
            ResponseTime = stopwatch.Elapsed
        };
    }

    public async Task SetAuthenticationAsync(AuthenticationConfig authConfig)
    {
        _httpClient.DefaultRequestHeaders.Authorization = null;
        RemoveDefaultHeader("X-API-Key");

        switch (authConfig.Type)
        {
            case AuthenticationType.ApiKey:
                if (!string.IsNullOrEmpty(authConfig.ApiKey))
                {
                    AddDefaultHeader(authConfig.ApiKeyHeader ?? "X-API-Key", authConfig.ApiKey);
                }
                break;

            case AuthenticationType.Bearer:
                if (!string.IsNullOrEmpty(authConfig.BearerToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authConfig.BearerToken);
                }
                break;

            case AuthenticationType.Basic:
                if (authConfig.BasicAuth != null)
                {
                    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{authConfig.BasicAuth.Username}:{authConfig.BasicAuth.Password}"));
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
                }
                break;

            case AuthenticationType.OAuth2:
                if (authConfig.OAuth2 != null)
                {
                    var token = await GetOAuth2TokenAsync(authConfig.OAuth2);
                    if (!string.IsNullOrEmpty(token))
                    {
                        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    }
                }
                break;
        }
    }

    public void AddDefaultHeader(string name, string value)
    {
        _defaultHeaders[name] = value;
        
        if (_httpClient.DefaultRequestHeaders.Contains(name))
        {
            _httpClient.DefaultRequestHeaders.Remove(name);
        }
        
        _httpClient.DefaultRequestHeaders.Add(name, value);
    }

    public void RemoveDefaultHeader(string name)
    {
        _defaultHeaders.Remove(name);
        _httpClient.DefaultRequestHeaders.Remove(name);
    }

    private HttpRequestMessage CreateHttpRequestMessage(HttpMethod method, string endpoint, object? body, Dictionary<string, string>? headers)
    {
        var request = new HttpRequestMessage(method, endpoint);

        // Add custom headers
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
        }

        // Add body if present
        if (body != null && (method == HttpMethod.Post || method == HttpMethod.Put || method == HttpMethod.Patch))
        {
            var json = JsonConvert.SerializeObject(body, Formatting.None);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return request;
    }

    private async Task<ApiResponse<T>> CreateApiResponse<T>(HttpResponseMessage response, TimeSpan responseTime)
    {
        var content = await response.Content.ReadAsStringAsync();
        var headers = response.Headers.ToDictionary(h => h.Key, h => h.Value);

        // Add content headers
        foreach (var header in response.Content.Headers)
        {
            headers[header.Key] = header.Value;
        }

        var apiResponse = new ApiResponse<T>
        {
            StatusCode = response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            Headers = headers,
            RawContent = content,
            ResponseTime = responseTime
        };

        // Try to deserialize the response
        if (!string.IsNullOrEmpty(content))
        {
            try
            {
                apiResponse.Data = JsonConvert.DeserializeObject<T>(content);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize response content to type {Type}", typeof(T).Name);
            }
        }

        return apiResponse;
    }

    private bool ShouldRetry<T>(ApiResponse<T> response, int retryCount, int maxRetries)
    {
        return retryCount < maxRetries && 
               _configuration.Retry.RetryOnStatusCodes.Contains((int)response.StatusCode);
    }

    private int CalculateRetryDelay(int retryCount)
    {
        var baseDelay = _configuration.Retry.DelayMilliseconds;
        
        if (_configuration.Retry.ExponentialBackoff)
        {
            return (int)(baseDelay * Math.Pow(2, retryCount - 1));
        }
        
        return baseDelay;
    }

    private async Task<string?> GetOAuth2TokenAsync(OAuth2Config oauth2Config)
    {
        try
        {
            using var client = new HttpClient();
            var tokenRequest = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("client_id", oauth2Config.ClientId),
                new("client_secret", oauth2Config.ClientSecret)
            };

            if (!string.IsNullOrEmpty(oauth2Config.Scope))
            {
                tokenRequest.Add(new KeyValuePair<string, string>("scope", oauth2Config.Scope));
            }

            var response = await client.PostAsync(oauth2Config.TokenUrl, new FormUrlEncodedContent(tokenRequest));
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                dynamic? tokenResponse = JsonConvert.DeserializeObject(content);
                return tokenResponse?.access_token;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to obtain OAuth2 token");
        }

        return null;
    }

    private void LogRequest(HttpRequestMessage request, object? body)
    {
        var logMessage = new StringBuilder();
        logMessage.AppendLine($"=== REQUEST ===");
        logMessage.AppendLine($"{request.Method} {request.RequestUri}");

        if (_configuration.Logging.LogHeaders && request.Headers.Any())
        {
            logMessage.AppendLine("Headers:");
            foreach (var header in request.Headers)
            {
                logMessage.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        if (_configuration.Logging.LogBody && body != null)
        {
            logMessage.AppendLine($"Body: {JsonConvert.SerializeObject(body, Formatting.Indented)}");
        }

        _logger.LogInformation(logMessage.ToString());
    }

    private void LogResponse<T>(ApiResponse<T> response)
    {
        var logMessage = new StringBuilder();
        logMessage.AppendLine($"=== RESPONSE ===");
        logMessage.AppendLine($"Status: {(int)response.StatusCode} {response.ReasonPhrase}");
        logMessage.AppendLine($"Response Time: {response.ResponseTime.TotalMilliseconds}ms");

        if (_configuration.Logging.LogHeaders && response.Headers.Any())
        {
            logMessage.AppendLine("Headers:");
            foreach (var header in response.Headers)
            {
                logMessage.AppendLine($"  {header.Key}: {string.Join(", ", header.Value)}");
            }
        }

        if (_configuration.Logging.LogBody && !string.IsNullOrEmpty(response.RawContent))
        {
            logMessage.AppendLine($"Body: {response.RawContent}");
        }

        _logger.LogInformation(logMessage.ToString());
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}
