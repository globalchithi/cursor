using System.Text;
using ApiTestFramework.Models;
using Newtonsoft.Json;

namespace ApiTestFramework.Reporting;

/// <summary>
/// Generates test reports in various formats
/// </summary>
public class TestReporter
{
    private readonly List<TestResult> _testResults = new();
    private readonly string _outputDirectory;

    public TestReporter(string? outputDirectory = null)
    {
        _outputDirectory = outputDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
        
        if (!Directory.Exists(_outputDirectory))
        {
            Directory.CreateDirectory(_outputDirectory);
        }
    }

    /// <summary>
    /// Adds a test result to the report
    /// </summary>
    /// <param name="testResult">Test result</param>
    public void AddTestResult(TestResult testResult)
    {
        _testResults.Add(testResult);
    }

    /// <summary>
    /// Generates an HTML report
    /// </summary>
    /// <param name="fileName">Output file name</param>
    /// <returns>Path to generated report</returns>
    public string GenerateHtmlReport(string fileName = "TestReport.html")
    {
        var filePath = Path.Combine(_outputDirectory, fileName);
        var html = GenerateHtmlContent();
        File.WriteAllText(filePath, html);
        return filePath;
    }

    /// <summary>
    /// Generates a JSON report
    /// </summary>
    /// <param name="fileName">Output file name</param>
    /// <returns>Path to generated report</returns>
    public string GenerateJsonReport(string fileName = "TestReport.json")
    {
        var filePath = Path.Combine(_outputDirectory, fileName);
        var reportData = new
        {
            GeneratedAt = DateTime.UtcNow,
            Summary = GetTestSummary(),
            TestResults = _testResults
        };
        
        var json = JsonConvert.SerializeObject(reportData, Formatting.Indented);
        File.WriteAllText(filePath, json);
        return filePath;
    }

    /// <summary>
    /// Generates a CSV report
    /// </summary>
    /// <param name="fileName">Output file name</param>
    /// <returns>Path to generated report</returns>
    public string GenerateCsvReport(string fileName = "TestReport.csv")
    {
        var filePath = Path.Combine(_outputDirectory, fileName);
        var csv = GenerateCsvContent();
        File.WriteAllText(filePath, csv);
        return filePath;
    }

    /// <summary>
    /// Gets test execution summary
    /// </summary>
    /// <returns>Test summary</returns>
    public TestSummary GetTestSummary()
    {
        return new TestSummary
        {
            TotalTests = _testResults.Count,
            PassedTests = _testResults.Count(r => r.Status == TestStatus.Passed),
            FailedTests = _testResults.Count(r => r.Status == TestStatus.Failed),
            SkippedTests = _testResults.Count(r => r.Status == TestStatus.Skipped),
            TotalExecutionTime = TimeSpan.FromMilliseconds(_testResults.Sum(r => r.ExecutionTime.TotalMilliseconds)),
            AverageResponseTime = _testResults.Any() 
                ? TimeSpan.FromMilliseconds(_testResults.Average(r => r.ResponseTime?.TotalMilliseconds ?? 0))
                : TimeSpan.Zero
        };
    }

    private string GenerateHtmlContent()
    {
        var summary = GetTestSummary();
        var html = new StringBuilder();

        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("    <title>API Test Report</title>");
        html.AppendLine("    <style>");
        html.AppendLine(GetHtmlStyles());
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("    <div class='container'>");
        html.AppendLine("        <h1>API Test Report</h1>");
        html.AppendLine($"        <p class='timestamp'>Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
        
        // Summary section
        html.AppendLine("        <div class='summary'>");
        html.AppendLine("            <h2>Test Summary</h2>");
        html.AppendLine("            <div class='summary-grid'>");
        html.AppendLine($"                <div class='summary-item'><span class='label'>Total Tests:</span> <span class='value'>{summary.TotalTests}</span></div>");
        html.AppendLine($"                <div class='summary-item passed'><span class='label'>Passed:</span> <span class='value'>{summary.PassedTests}</span></div>");
        html.AppendLine($"                <div class='summary-item failed'><span class='label'>Failed:</span> <span class='value'>{summary.FailedTests}</span></div>");
        html.AppendLine($"                <div class='summary-item skipped'><span class='label'>Skipped:</span> <span class='value'>{summary.SkippedTests}</span></div>");
        html.AppendLine($"                <div class='summary-item'><span class='label'>Total Time:</span> <span class='value'>{summary.TotalExecutionTime:hh\\:mm\\:ss}</span></div>");
        html.AppendLine($"                <div class='summary-item'><span class='label'>Avg Response:</span> <span class='value'>{summary.AverageResponseTime.TotalMilliseconds:F0}ms</span></div>");
        html.AppendLine("            </div>");
        html.AppendLine("        </div>");

        // Test results section
        html.AppendLine("        <div class='results'>");
        html.AppendLine("            <h2>Test Results</h2>");
        html.AppendLine("            <table>");
        html.AppendLine("                <thead>");
        html.AppendLine("                    <tr>");
        html.AppendLine("                        <th>Test Name</th>");
        html.AppendLine("                        <th>Status</th>");
        html.AppendLine("                        <th>Execution Time</th>");
        html.AppendLine("                        <th>Response Time</th>");
        html.AppendLine("                        <th>Environment</th>");
        html.AppendLine("                        <th>Details</th>");
        html.AppendLine("                    </tr>");
        html.AppendLine("                </thead>");
        html.AppendLine("                <tbody>");

        foreach (var result in _testResults)
        {
            var statusClass = result.Status.ToString().ToLower();
            html.AppendLine($"                    <tr class='{statusClass}'>");
            html.AppendLine($"                        <td>{result.TestName}</td>");
            html.AppendLine($"                        <td><span class='status {statusClass}'>{result.Status}</span></td>");
            html.AppendLine($"                        <td>{result.ExecutionTime.TotalMilliseconds:F0}ms</td>");
            html.AppendLine($"                        <td>{result.ResponseTime?.TotalMilliseconds:F0}ms</td>");
            html.AppendLine($"                        <td>{result.Environment}</td>");
            html.AppendLine($"                        <td>{result.ErrorMessage ?? "N/A"}</td>");
            html.AppendLine("                    </tr>");
        }

        html.AppendLine("                </tbody>");
        html.AppendLine("            </table>");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private string GetHtmlStyles()
    {
        return @"
        body { font-family: Arial, sans-serif; margin: 0; padding: 20px; background-color: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }
        h1 { color: #333; margin-bottom: 10px; }
        h2 { color: #555; border-bottom: 2px solid #eee; padding-bottom: 10px; }
        .timestamp { color: #666; font-style: italic; margin-bottom: 30px; }
        .summary { margin-bottom: 30px; }
        .summary-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 15px; }
        .summary-item { padding: 15px; border-radius: 5px; background: #f8f9fa; }
        .summary-item.passed { background: #d4edda; }
        .summary-item.failed { background: #f8d7da; }
        .summary-item.skipped { background: #fff3cd; }
        .label { font-weight: bold; }
        .value { float: right; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background-color: #f8f9fa; font-weight: bold; }
        tr.passed { background-color: #f8fff8; }
        tr.failed { background-color: #fff8f8; }
        tr.skipped { background-color: #fffef8; }
        .status { padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: bold; }
        .status.passed { background: #28a745; color: white; }
        .status.failed { background: #dc3545; color: white; }
        .status.skipped { background: #ffc107; color: black; }
        ";
    }

    private string GenerateCsvContent()
    {
        var csv = new StringBuilder();
        csv.AppendLine("TestName,Status,ExecutionTime(ms),ResponseTime(ms),Environment,ErrorMessage");

        foreach (var result in _testResults)
        {
            csv.AppendLine($"\"{result.TestName}\",{result.Status},{result.ExecutionTime.TotalMilliseconds:F0},{result.ResponseTime?.TotalMilliseconds:F0},{result.Environment},\"{result.ErrorMessage?.Replace("\"", "\"\"")}\"");
        }

        return csv.ToString();
    }
}

/// <summary>
/// Represents a test result
/// </summary>
public class TestResult
{
    public string TestName { get; set; } = string.Empty;
    public TestStatus Status { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public TimeSpan? ResponseTime { get; set; }
    public string Environment { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Test execution status
/// </summary>
public enum TestStatus
{
    Passed,
    Failed,
    Skipped
}

/// <summary>
/// Test execution summary
/// </summary>
public class TestSummary
{
    public int TotalTests { get; set; }
    public int PassedTests { get; set; }
    public int FailedTests { get; set; }
    public int SkippedTests { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public double PassRate => TotalTests > 0 ? (double)PassedTests / TotalTests * 100 : 0;
}

