using ApiTestFramework.Configuration;
using ApiTestFramework.Core;
using ApiTestFramework.Models;
using ApiTestFramework.TestData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using TechTalk.SpecFlow;

namespace SampleApiTests.StepDefinitions;

/// <summary>
/// Base class for SpecFlow step definitions providing common functionality
/// </summary>
public abstract class SpecFlowTestBase
{
    protected IApiClient ApiClient { get; private set; } = null!;
    protected TestConfiguration Configuration { get; private set; } = null!;
    protected TestDataManager TestData { get; private set; } = null!;
    protected ILogger Logger { get; private set; } = null!;

    private ServiceProvider? _serviceProvider;
    private bool _isInitialized = false;

    /// <summary>
    /// Gets the environment name for tests
    /// </summary>
    protected virtual string Environment => "Development";

    /// <summary>
    /// Gets the configuration file path
    /// </summary>
    protected virtual string? ConfigurationFile => null;

    /// <summary>
    /// Initializes the framework if not already initialized
    /// </summary>
    protected async Task InitializeFramework()
    {
        if (_isInitialized) return;

        try
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
            Logger = _serviceProvider.GetRequiredService<ILogger<SpecFlowTestBase>>();

            // Create API client
            ApiClient = _serviceProvider.GetRequiredService<IApiClient>();

            // Set up test data
            TestData = _serviceProvider.GetRequiredService<TestDataManager>();

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            // Fallback to console logging if framework initialization fails
            Logger = new ConsoleLogger<SpecFlowTestBase>();
            Console.WriteLine($"Framework initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Configures dependency injection services
    /// </summary>
    /// <param name="services">Service collection</param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddProvider(new SimpleConsoleLoggerProvider());
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
    /// Logs test information
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="args">Message arguments</param>
    protected void LogInfo(string message, params object[] args)
    {
        Logger.LogInformation(message, args);
        
        // Always output to console for SpecFlow tests
        if (args.Length > 0)
        {
            Console.WriteLine($"[INFO] {string.Format(message, args)}");
        }
        else
        {
            Console.WriteLine($"[INFO] {message}");
        }
    }

    /// <summary>
    /// Logs error information
    /// </summary>
    /// <param name="message">Log message</param>
    /// <param name="args">Message arguments</param>
    protected void LogError(string message, params object[] args)
    {
        Logger.LogError(message, args);
        
        // Always output to console for SpecFlow tests
        if (args.Length > 0)
        {
            Console.WriteLine($"[ERROR] {string.Format(message, args)}");
        }
        else
        {
            Console.WriteLine($"[ERROR] {message}");
        }
    }

    /// <summary>
    /// Cleanup method called by SpecFlow
    /// </summary>
    [AfterScenario]
    public Task Cleanup()
    {
        try
        {
            ApiClient?.Dispose();
            _serviceProvider?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Cleanup error: {ex.Message}");
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Simple console logger implementation for fallback
/// </summary>
internal class ConsoleLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        Console.WriteLine($"[{logLevel}] {message}");
        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception}");
        }
    }
}

/// <summary>
/// Simple console logger provider for fallback logging
/// </summary>
internal class SimpleConsoleLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new SimpleConsoleLogger(categoryName);
    }

    public void Dispose() { }
}

/// <summary>
/// Simple console logger implementation
/// </summary>
internal class SimpleConsoleLogger : ILogger
{
    private readonly string _categoryName;

    public SimpleConsoleLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = formatter(state, exception);
        Console.WriteLine($"[{logLevel}] {_categoryName}: {message}");
        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception}");
        }
    }
}
