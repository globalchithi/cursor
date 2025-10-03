using System.Collections;
using ApiTestFramework.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace ApiTestFramework.Configuration;

/// <summary>
/// Manages test configuration from various sources
/// </summary>
public class ConfigurationManager
{
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, TestConfiguration> _environmentConfigs;

    public ConfigurationManager(IConfiguration? configuration = null)
    {
        _configuration = configuration ?? BuildDefaultConfiguration();
        _environmentConfigs = new Dictionary<string, TestConfiguration>();
        LoadEnvironmentConfigurations();
    }

    /// <summary>
    /// Gets configuration for a specific environment
    /// </summary>
    /// <param name="environment">Environment name</param>
    /// <returns>Test configuration</returns>
    public TestConfiguration GetConfiguration(string environment = "Development")
    {
        if (_environmentConfigs.TryGetValue(environment, out var config))
        {
            return config;
        }

        // If specific environment not found, try to build from configuration
        var testConfig = new TestConfiguration { Environment = environment };
        _configuration.GetSection($"Environments:{environment}").Bind(testConfig);
        
        // If still empty, use default configuration
        if (string.IsNullOrEmpty(testConfig.BaseUrl))
        {
            _configuration.GetSection("DefaultConfiguration").Bind(testConfig);
            testConfig.Environment = environment;
        }

        _environmentConfigs[environment] = testConfig;
        return testConfig;
    }

    /// <summary>
    /// Adds or updates configuration for an environment
    /// </summary>
    /// <param name="environment">Environment name</param>
    /// <param name="configuration">Test configuration</param>
    public void SetConfiguration(string environment, TestConfiguration configuration)
    {
        configuration.Environment = environment;
        _environmentConfigs[environment] = configuration;
    }

    /// <summary>
    /// Gets all available environment names
    /// </summary>
    /// <returns>Environment names</returns>
    public IEnumerable<string> GetAvailableEnvironments()
    {
        var configEnvironments = _configuration.GetSection("Environments").GetChildren().Select(x => x.Key);
        var loadedEnvironments = _environmentConfigs.Keys;
        return configEnvironments.Union(loadedEnvironments).Distinct();
    }

    /// <summary>
    /// Loads configuration from a JSON file
    /// </summary>
    /// <param name="filePath">Path to JSON configuration file</param>
    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {filePath}");
        }

        var json = File.ReadAllText(filePath);
        var configData = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

        if (configData?.ContainsKey("Environments") == true)
        {
            var environmentsJson = JsonConvert.SerializeObject(configData["Environments"]);
            var environments = JsonConvert.DeserializeObject<Dictionary<string, TestConfiguration>>(environmentsJson);
            
            if (environments != null)
            {
                foreach (var env in environments)
                {
                    env.Value.Environment = env.Key;
                    _environmentConfigs[env.Key] = env.Value;
                }
            }
        }
    }

    /// <summary>
    /// Saves current configurations to a JSON file
    /// </summary>
    /// <param name="filePath">Path to save the configuration file</param>
    public void SaveToFile(string filePath)
    {
        var configData = new
        {
            Environments = _environmentConfigs
        };

        var json = JsonConvert.SerializeObject(configData, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Creates a configuration from environment variables
    /// </summary>
    /// <param name="prefix">Environment variable prefix (default: "API_TEST_")</param>
    /// <returns>Test configuration</returns>
    public TestConfiguration CreateFromEnvironmentVariables(string prefix = "API_TEST_")
    {
        var config = new TestConfiguration();

        // Basic settings
        config.BaseUrl = Environment.GetEnvironmentVariable($"{prefix}BASE_URL") ?? config.BaseUrl;
        
        if (int.TryParse(Environment.GetEnvironmentVariable($"{prefix}TIMEOUT_SECONDS"), out var timeout))
        {
            config.TimeoutSeconds = timeout;
        }

        config.Environment = Environment.GetEnvironmentVariable($"{prefix}ENVIRONMENT") ?? config.Environment;

        // Authentication
        var authType = Environment.GetEnvironmentVariable($"{prefix}AUTH_TYPE");
        if (!string.IsNullOrEmpty(authType) && Enum.TryParse<AuthenticationType>(authType, true, out var authTypeEnum))
        {
            config.Authentication = new AuthenticationConfig { Type = authTypeEnum };

            switch (authTypeEnum)
            {
                case AuthenticationType.ApiKey:
                    config.Authentication.ApiKey = Environment.GetEnvironmentVariable($"{prefix}API_KEY");
                    config.Authentication.ApiKeyHeader = Environment.GetEnvironmentVariable($"{prefix}API_KEY_HEADER") ?? "X-API-Key";
                    break;

                case AuthenticationType.Bearer:
                    config.Authentication.BearerToken = Environment.GetEnvironmentVariable($"{prefix}BEARER_TOKEN");
                    break;

                case AuthenticationType.Basic:
                    config.Authentication.BasicAuth = new BasicAuthConfig
                    {
                        Username = Environment.GetEnvironmentVariable($"{prefix}BASIC_USERNAME") ?? "",
                        Password = Environment.GetEnvironmentVariable($"{prefix}BASIC_PASSWORD") ?? ""
                    };
                    break;

                case AuthenticationType.OAuth2:
                    config.Authentication.OAuth2 = new OAuth2Config
                    {
                        ClientId = Environment.GetEnvironmentVariable($"{prefix}OAUTH2_CLIENT_ID") ?? "",
                        ClientSecret = Environment.GetEnvironmentVariable($"{prefix}OAUTH2_CLIENT_SECRET") ?? "",
                        TokenUrl = Environment.GetEnvironmentVariable($"{prefix}OAUTH2_TOKEN_URL") ?? "",
                        Scope = Environment.GetEnvironmentVariable($"{prefix}OAUTH2_SCOPE")
                    };
                    break;
            }
        }

        // Default headers
        var headerPrefix = $"{prefix}HEADER_";
        foreach (DictionaryEntry envVar in Environment.GetEnvironmentVariables())
        {
            var key = envVar.Key.ToString();
            if (key?.StartsWith(headerPrefix) == true)
            {
                var headerName = key.Substring(headerPrefix.Length).Replace("_", "-");
                config.DefaultHeaders[headerName] = envVar.Value?.ToString() ?? "";
            }
        }

        return config;
    }

    private IConfiguration BuildDefaultConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("testsettings.json", optional: true)
            .AddEnvironmentVariables();

        return builder.Build();
    }

    private void LoadEnvironmentConfigurations()
    {
        var environmentsSection = _configuration.GetSection("Environments");
        foreach (var environmentSection in environmentsSection.GetChildren())
        {
            var config = new TestConfiguration();
            environmentSection.Bind(config);
            config.Environment = environmentSection.Key;
            _environmentConfigs[environmentSection.Key] = config;
        }
    }
}

/// <summary>
/// Builder for creating test configurations fluently
/// </summary>
public class TestConfigurationBuilder
{
    private readonly TestConfiguration _configuration = new();

    /// <summary>
    /// Sets the base URL
    /// </summary>
    /// <param name="baseUrl">Base URL</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder WithBaseUrl(string baseUrl)
    {
        _configuration.BaseUrl = baseUrl;
        return this;
    }

    /// <summary>
    /// Sets the timeout
    /// </summary>
    /// <param name="timeoutSeconds">Timeout in seconds</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder WithTimeout(int timeoutSeconds)
    {
        _configuration.TimeoutSeconds = timeoutSeconds;
        return this;
    }

    /// <summary>
    /// Sets the environment
    /// </summary>
    /// <param name="environment">Environment name</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder ForEnvironment(string environment)
    {
        _configuration.Environment = environment;
        return this;
    }

    /// <summary>
    /// Adds a default header
    /// </summary>
    /// <param name="name">Header name</param>
    /// <param name="value">Header value</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder WithHeader(string name, string value)
    {
        _configuration.DefaultHeaders[name] = value;
        return this;
    }

    /// <summary>
    /// Sets API key authentication
    /// </summary>
    /// <param name="apiKey">API key</param>
    /// <param name="headerName">Header name (default: X-API-Key)</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder WithApiKeyAuth(string apiKey, string headerName = "X-API-Key")
    {
        _configuration.Authentication = new AuthenticationConfig
        {
            Type = AuthenticationType.ApiKey,
            ApiKey = apiKey,
            ApiKeyHeader = headerName
        };
        return this;
    }

    /// <summary>
    /// Sets Bearer token authentication
    /// </summary>
    /// <param name="token">Bearer token</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder WithBearerAuth(string token)
    {
        _configuration.Authentication = new AuthenticationConfig
        {
            Type = AuthenticationType.Bearer,
            BearerToken = token
        };
        return this;
    }

    /// <summary>
    /// Sets Basic authentication
    /// </summary>
    /// <param name="username">Username</param>
    /// <param name="password">Password</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder WithBasicAuth(string username, string password)
    {
        _configuration.Authentication = new AuthenticationConfig
        {
            Type = AuthenticationType.Basic,
            BasicAuth = new BasicAuthConfig
            {
                Username = username,
                Password = password
            }
        };
        return this;
    }

    /// <summary>
    /// Sets OAuth2 authentication
    /// </summary>
    /// <param name="clientId">Client ID</param>
    /// <param name="clientSecret">Client secret</param>
    /// <param name="tokenUrl">Token URL</param>
    /// <param name="scope">Scope (optional)</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder WithOAuth2Auth(string clientId, string clientSecret, string tokenUrl, string? scope = null)
    {
        _configuration.Authentication = new AuthenticationConfig
        {
            Type = AuthenticationType.OAuth2,
            OAuth2 = new OAuth2Config
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                TokenUrl = tokenUrl,
                Scope = scope
            }
        };
        return this;
    }

    /// <summary>
    /// Configures retry settings
    /// </summary>
    /// <param name="maxRetries">Maximum retries</param>
    /// <param name="delayMilliseconds">Delay between retries</param>
    /// <param name="exponentialBackoff">Use exponential backoff</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder WithRetry(int maxRetries, int delayMilliseconds = 1000, bool exponentialBackoff = true)
    {
        _configuration.Retry.MaxRetries = maxRetries;
        _configuration.Retry.DelayMilliseconds = delayMilliseconds;
        _configuration.Retry.ExponentialBackoff = exponentialBackoff;
        return this;
    }

    /// <summary>
    /// Configures logging settings
    /// </summary>
    /// <param name="logRequests">Log requests</param>
    /// <param name="logResponses">Log responses</param>
    /// <param name="logHeaders">Log headers</param>
    /// <param name="logBody">Log body</param>
    /// <returns>Builder instance</returns>
    public TestConfigurationBuilder WithLogging(bool logRequests = true, bool logResponses = true, bool logHeaders = false, bool logBody = true)
    {
        _configuration.Logging.LogRequests = logRequests;
        _configuration.Logging.LogResponses = logResponses;
        _configuration.Logging.LogHeaders = logHeaders;
        _configuration.Logging.LogBody = logBody;
        return this;
    }

    /// <summary>
    /// Builds the configuration
    /// </summary>
    /// <returns>Test configuration</returns>
    public TestConfiguration Build()
    {
        return _configuration;
    }
}
