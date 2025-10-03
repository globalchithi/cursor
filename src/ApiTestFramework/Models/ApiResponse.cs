using System.Net;
using Newtonsoft.Json;

namespace ApiTestFramework.Models;

/// <summary>
/// Represents an API response with comprehensive information
/// </summary>
/// <typeparam name="T">The type of the response body</typeparam>
public class ApiResponse<T>
{
    public HttpStatusCode StatusCode { get; set; }
    public string? ReasonPhrase { get; set; }
    public Dictionary<string, IEnumerable<string>> Headers { get; set; } = new();
    public T? Data { get; set; }
    public string? RawContent { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public bool IsSuccessStatusCode => ((int)StatusCode >= 200) && ((int)StatusCode <= 299);
    public Exception? Exception { get; set; }

    /// <summary>
    /// Gets the response data as a specific type
    /// </summary>
    /// <typeparam name="TResult">Target type</typeparam>
    /// <returns>Converted data</returns>
    public TResult? GetDataAs<TResult>()
    {
        if (RawContent == null) return default;
        
        try
        {
            return JsonConvert.DeserializeObject<TResult>(RawContent);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    /// <summary>
    /// Gets a specific header value
    /// </summary>
    /// <param name="headerName">Header name</param>
    /// <returns>Header value or null if not found</returns>
    public string? GetHeader(string headerName)
    {
        return Headers.TryGetValue(headerName, out var values) ? values.FirstOrDefault() : null;
    }
}

/// <summary>
/// Non-generic version of ApiResponse
/// </summary>
public class ApiResponse : ApiResponse<object>
{
}

