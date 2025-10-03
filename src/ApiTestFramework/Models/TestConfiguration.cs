namespace ApiTestFramework.Models;

/// <summary>
/// Configuration settings for API tests
/// </summary>
public class TestConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public Dictionary<string, string> DefaultHeaders { get; set; } = new();
    public AuthenticationConfig? Authentication { get; set; }
    public RetryConfig Retry { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
    public string Environment { get; set; } = "Development";
}

/// <summary>
/// Authentication configuration
/// </summary>
public class AuthenticationConfig
{
    public AuthenticationType Type { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiKeyHeader { get; set; } = "X-API-Key";
    public string? BearerToken { get; set; }
    public BasicAuthConfig? BasicAuth { get; set; }
    public OAuth2Config? OAuth2 { get; set; }
}

/// <summary>
/// Basic authentication configuration
/// </summary>
public class BasicAuthConfig
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// OAuth2 configuration
/// </summary>
public class OAuth2Config
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = string.Empty;
    public string? Scope { get; set; }
}

/// <summary>
/// Retry configuration
/// </summary>
public class RetryConfig
{
    public int MaxRetries { get; set; } = 3;
    public int DelayMilliseconds { get; set; } = 1000;
    public bool ExponentialBackoff { get; set; } = true;
    public List<int> RetryOnStatusCodes { get; set; } = new() { 429, 500, 502, 503, 504 };
}

/// <summary>
/// Logging configuration
/// </summary>
public class LoggingConfig
{
    public bool LogRequests { get; set; } = true;
    public bool LogResponses { get; set; } = true;
    public bool LogHeaders { get; set; } = false;
    public bool LogBody { get; set; } = true;
    public string LogLevel { get; set; } = "Information";
    public string? LogFilePath { get; set; }
}

/// <summary>
/// Authentication types
/// </summary>
public enum AuthenticationType
{
    None,
    ApiKey,
    Bearer,
    Basic,
    OAuth2
}

