using ApiTestFramework.Configuration;
using ApiTestFramework.Core;
using ApiTestFramework.Models;
using ApiTestFramework.TestData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace ApiTestFramework.Utilities;

/// <summary>
/// Base class for API tests providing common functionality
/// </summary>
public abstract class ApiTestBase
{
    protected IApiClient ApiClient { get; private set; } = null!;
    protected TestConfiguration Configuration { get; private set; } = null!;
    protected TestDataManager TestData { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;

    private ServiceProvider? _serviceProvider;

    /// <summary>
    /// Gets the environment name for tests (can be overridden)
    /// </summary>
    protected virtual string Environment => TestContext.Parameters["Environment"] ?? "Development";

    /// <summary>
    /// Gets the configuration file path (can be overridden)
    /// </summary>
    protected virtual string? ConfigurationFile => TestContext.Parameters["ConfigFile"];

    /// <summary>
    /// Sets up the test environment before each test
    /// </summary>
    [SetUp]
    public virtual async Task SetUp()
    {
        await InitializeFramework();
        await OnSetUp();
    }

    /// <summary>
    /// Cleans up after each test
    /// </summary>
    [TearDown]
    public virtual async Task TearDown()
    {
        await OnTearDown();
        ApiClient?.Dispose();
        _serviceProvider?.Dispose();
    }

    /// <summary>
    /// Override this method for custom setup logic
    /// </summary>
    protected virtual Task OnSetUp()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Override this method for custom teardown logic
    /// </summary>
    protected virtual Task OnTearDown()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Initializes the testing framework
    /// </summary>
    private async Task InitializeFramework()
    {
        // Set up dependency injection
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // Get configuration
        var configManager = _serviceProvider.GetRequiredService<ConfigurationManager>();
        if (!string.IsNullOrEmpty(ConfigurationFile))
        {
            configManager.LoadFromFile(ConfigurationFile);
        }
        Configuration = configManager.GetConfiguration(Environment);

        // Set up logging
        Logger = _serviceProvider.GetRequiredService<ILogger<ApiTestBase>>();

        // Create API client
        ApiClient = _serviceProvider.GetRequiredService<IApiClient>();

        // Set up test data
        TestData = _serviceProvider.GetRequiredService<TestDataManager>();

        // Custom initialization
        await OnInitialize();
    }

    /// <summary>
    /// Configures dependency injection services
    /// </summary>
    /// <param name="services">Service collection</param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSingleton<ConfigurationManager>();
        services.AddSingleton<TestDataManager>();
        
        services.AddTransient<IApiClient>(provider =>
        {
            var configManager = provider.GetRequiredService<ConfigurationManager>();
            var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<ApiClient>();
            var config = configManager.GetConfiguration(Environment);
            return new ApiClient(config, logger);
        });
    }

    /// <summary>
    /// Override this method for custom initialization logic
    /// </summary>
    protected virtual Task OnInitialize()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs test information
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="args">Message arguments</param>
    protected void LogInfo(string message, params object[] args)
    {
        Logger.LogInformation(message, args);
        if (args.Length > 0)
        {
            // Convert structured logging format to string.Format for TestContext
            var formattedMessage = message;
            for (int i = 0; i < args.Length; i++)
            {
                formattedMessage = formattedMessage.Replace($"{{{i}}}", args[i]?.ToString() ?? "null");
            }
            TestContext.WriteLine($"[INFO] {formattedMessage}");
        }
        else
        {
            TestContext.WriteLine($"[INFO] {message}");
        }
    }

    /// <summary>
    /// Logs test warnings
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="args">Message arguments</param>
    protected void LogWarning(string message, params object[] args)
    {
        Logger.LogWarning(message, args);
        if (args.Length > 0)
        {
            var formattedMessage = message;
            for (int i = 0; i < args.Length; i++)
            {
                formattedMessage = formattedMessage.Replace($"{{{i}}}", args[i]?.ToString() ?? "null");
            }
            TestContext.WriteLine($"[WARN] {formattedMessage}");
        }
        else
        {
            TestContext.WriteLine($"[WARN] {message}");
        }
    }

    /// <summary>
    /// Logs test errors
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="args">Message arguments</param>
    protected void LogError(string message, params object[] args)
    {
        Logger.LogError(message, args);
        if (args.Length > 0)
        {
            var formattedMessage = message;
            for (int i = 0; i < args.Length; i++)
            {
                formattedMessage = formattedMessage.Replace($"{{{i}}}", args[i]?.ToString() ?? "null");
            }
            TestContext.WriteLine($"[ERROR] {formattedMessage}");
        }
        else
        {
            TestContext.WriteLine($"[ERROR] {message}");
        }
    }

    /// <summary>
    /// Waits for a condition to be true with timeout
    /// </summary>
    /// <param name="condition">Condition to check</param>
    /// <param name="timeout">Timeout duration</param>
    /// <param name="interval">Check interval</param>
    /// <returns>True if condition met, false if timeout</returns>
    protected async Task<bool> WaitForCondition(Func<Task<bool>> condition, TimeSpan timeout, TimeSpan? interval = null)
    {
        var checkInterval = interval ?? TimeSpan.FromSeconds(1);
        var endTime = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < endTime)
        {
            if (await condition())
            {
                return true;
            }

            await Task.Delay(checkInterval);
        }

        return false;
    }

    /// <summary>
    /// Retries an operation with exponential backoff
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    /// <param name="operation">Operation to retry</param>
    /// <param name="maxRetries">Maximum retries</param>
    /// <param name="baseDelay">Base delay between retries</param>
    /// <returns>Operation result</returns>
    protected async Task<T> RetryOperation<T>(Func<Task<T>> operation, int maxRetries = 3, TimeSpan? baseDelay = null)
    {
        var delay = baseDelay ?? TimeSpan.FromSeconds(1);
        Exception? lastException = null;

        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                lastException = ex;
                
                if (i == maxRetries)
                {
                    break;
                }

                LogWarning("Operation failed (attempt {Attempt}/{MaxRetries}): {Error}", i + 1, maxRetries + 1, ex.Message);
                
                var currentDelay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, i));
                await Task.Delay(currentDelay);
            }
        }

        throw lastException ?? new InvalidOperationException("Retry operation failed");
    }

    /// <summary>
    /// Creates a test context with additional information
    /// </summary>
    /// <param name="testName">Test name</param>
    /// <param name="additionalInfo">Additional context information</param>
    protected void SetTestContext(string testName, Dictionary<string, object>? additionalInfo = null)
    {
        TestContext.WriteLine($"=== {testName} ===");
        TestContext.WriteLine($"Environment: {Environment}");
        TestContext.WriteLine($"Base URL: {Configuration.BaseUrl}");
        TestContext.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

        if (additionalInfo != null)
        {
            foreach (var info in additionalInfo)
            {
                TestContext.WriteLine($"{info.Key}: {info.Value}");
            }
        }

        TestContext.WriteLine("");
    }
}

/// <summary>
/// Attribute to specify test environment
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class TestEnvironmentAttribute : Attribute
{
    public string Environment { get; }

    public TestEnvironmentAttribute(string environment)
    {
        Environment = environment;
    }
}

/// <summary>
/// Attribute to specify test category for API tests
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class ApiTestCategoryAttribute : CategoryAttribute
{
    public ApiTestCategoryAttribute(string category) : base(category) { }
}

/// <summary>
/// Common test categories
/// </summary>
public static class TestCategories
{
    public const string Smoke = "Smoke";
    public const string Integration = "Integration";
    public const string Regression = "Regression";
    public const string Performance = "Performance";
    public const string Security = "Security";
    public const string Authentication = "Authentication";
    public const string CRUD = "CRUD";
    public const string Validation = "Validation";
    public const string ErrorHandling = "ErrorHandling";
}
