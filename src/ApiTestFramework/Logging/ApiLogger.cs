using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ApiTestFramework.Logging;

/// <summary>
/// Configures and provides logging for the API test framework
/// </summary>
public static class ApiLogger
{
    /// <summary>
    /// Creates a logger factory with Serilog configuration
    /// </summary>
    /// <param name="logLevel">Minimum log level</param>
    /// <param name="logFilePath">Optional log file path</param>
    /// <param name="includeConsole">Include console logging</param>
    /// <returns>Logger factory</returns>
    public static ILoggerFactory CreateLoggerFactory(LogLevel logLevel = LogLevel.Information, string? logFilePath = null, bool includeConsole = true)
    {
        var serilogLevel = ConvertLogLevel(logLevel);
        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(serilogLevel)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ApiTestFramework");

        if (includeConsole)
        {
            loggerConfig.WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        }

        if (!string.IsNullOrEmpty(logFilePath))
        {
            loggerConfig.WriteTo.File(
                logFilePath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
        }

        Log.Logger = loggerConfig.CreateLogger();

        return LoggerFactory.Create(builder =>
        {
            builder.AddSerilog(Log.Logger);
        });
    }

    /// <summary>
    /// Creates a logger for a specific type
    /// </summary>
    /// <typeparam name="T">Type to create logger for</typeparam>
    /// <param name="loggerFactory">Logger factory</param>
    /// <returns>Logger instance</returns>
    public static ILogger<T> CreateLogger<T>(ILoggerFactory loggerFactory)
    {
        return loggerFactory.CreateLogger<T>();
    }

    private static LogEventLevel ConvertLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}

/// <summary>
/// Performance logger for tracking API response times and test execution
/// </summary>
public class PerformanceLogger
{
    private readonly ILogger<PerformanceLogger> _logger;
    private readonly Dictionary<string, PerformanceMetric> _metrics = new();

    public PerformanceLogger(ILogger<PerformanceLogger> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts tracking performance for an operation
    /// </summary>
    /// <param name="operationName">Operation name</param>
    /// <returns>Performance tracker</returns>
    public PerformanceTracker StartTracking(string operationName)
    {
        return new PerformanceTracker(operationName, this);
    }

    /// <summary>
    /// Records a performance metric
    /// </summary>
    /// <param name="operationName">Operation name</param>
    /// <param name="duration">Duration</param>
    /// <param name="success">Whether operation was successful</param>
    /// <param name="additionalData">Additional metric data</param>
    public void RecordMetric(string operationName, TimeSpan duration, bool success = true, Dictionary<string, object>? additionalData = null)
    {
        if (!_metrics.ContainsKey(operationName))
        {
            _metrics[operationName] = new PerformanceMetric { OperationName = operationName };
        }

        var metric = _metrics[operationName];
        metric.TotalExecutions++;
        metric.TotalDuration += duration;
        
        if (success)
        {
            metric.SuccessfulExecutions++;
        }

        if (duration > metric.MaxDuration)
        {
            metric.MaxDuration = duration;
        }

        if (metric.MinDuration == TimeSpan.Zero || duration < metric.MinDuration)
        {
            metric.MinDuration = duration;
        }

        _logger.LogInformation("Performance: {Operation} completed in {Duration}ms (Success: {Success})", 
            operationName, duration.TotalMilliseconds, success);

        if (additionalData != null)
        {
            foreach (var data in additionalData)
            {
                _logger.LogDebug("Performance Data: {Key} = {Value}", data.Key, data.Value);
            }
        }
    }

    /// <summary>
    /// Gets performance metrics for an operation
    /// </summary>
    /// <param name="operationName">Operation name</param>
    /// <returns>Performance metric or null if not found</returns>
    public PerformanceMetric? GetMetric(string operationName)
    {
        return _metrics.TryGetValue(operationName, out var metric) ? metric : null;
    }

    /// <summary>
    /// Gets all performance metrics
    /// </summary>
    /// <returns>All performance metrics</returns>
    public IReadOnlyDictionary<string, PerformanceMetric> GetAllMetrics()
    {
        return new Dictionary<string, PerformanceMetric>(_metrics);
    }

    /// <summary>
    /// Generates a performance report
    /// </summary>
    /// <returns>Performance report</returns>
    public string GenerateReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== Performance Report ===");
        report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine();

        foreach (var metric in _metrics.Values)
        {
            report.AppendLine($"Operation: {metric.OperationName}");
            report.AppendLine($"  Total Executions: {metric.TotalExecutions}");
            report.AppendLine($"  Successful: {metric.SuccessfulExecutions} ({metric.SuccessRate:P1})");
            report.AppendLine($"  Average Duration: {metric.AverageDuration.TotalMilliseconds:F1}ms");
            report.AppendLine($"  Min Duration: {metric.MinDuration.TotalMilliseconds:F1}ms");
            report.AppendLine($"  Max Duration: {metric.MaxDuration.TotalMilliseconds:F1}ms");
            report.AppendLine();
        }

        return report.ToString();
    }
}

/// <summary>
/// Tracks performance for a single operation
/// </summary>
public class PerformanceTracker : IDisposable
{
    private readonly string _operationName;
    private readonly PerformanceLogger _logger;
    private readonly DateTime _startTime;
    private readonly Dictionary<string, object> _additionalData = new();
    private bool _disposed;
    private bool _success = true;

    internal PerformanceTracker(string operationName, PerformanceLogger logger)
    {
        _operationName = operationName;
        _logger = logger;
        _startTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the operation as failed
    /// </summary>
    public void MarkAsFailed()
    {
        _success = false;
    }

    /// <summary>
    /// Adds additional data to the performance metric
    /// </summary>
    /// <param name="key">Data key</param>
    /// <param name="value">Data value</param>
    public void AddData(string key, object value)
    {
        _additionalData[key] = value;
    }

    /// <summary>
    /// Completes the performance tracking
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            var duration = DateTime.UtcNow - _startTime;
            _logger.RecordMetric(_operationName, duration, _success, _additionalData);
            _disposed = true;
        }
    }
}

/// <summary>
/// Performance metric data
/// </summary>
public class PerformanceMetric
{
    public string OperationName { get; set; } = string.Empty;
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    
    public TimeSpan AverageDuration => TotalExecutions > 0 
        ? TimeSpan.FromMilliseconds(TotalDuration.TotalMilliseconds / TotalExecutions) 
        : TimeSpan.Zero;
    
    public double SuccessRate => TotalExecutions > 0 
        ? (double)SuccessfulExecutions / TotalExecutions 
        : 0;
}
