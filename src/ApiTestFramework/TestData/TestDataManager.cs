using Newtonsoft.Json;

namespace ApiTestFramework.TestData;

/// <summary>
/// Manages test data for API tests
/// </summary>
public class TestDataManager
{
    private readonly Dictionary<string, object> _testData;
    private readonly string? _dataDirectory;

    public TestDataManager(string? dataDirectory = null)
    {
        _dataDirectory = dataDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "TestData");
        _testData = new Dictionary<string, object>();
        
        if (Directory.Exists(_dataDirectory))
        {
            LoadTestDataFromDirectory();
        }
    }

    /// <summary>
    /// Gets test data by key
    /// </summary>
    /// <typeparam name="T">Type of test data</typeparam>
    /// <param name="key">Data key</param>
    /// <returns>Test data</returns>
    public T? GetData<T>(string key)
    {
        if (_testData.TryGetValue(key, out var data))
        {
            if (data is T directMatch)
            {
                return directMatch;
            }
            
            // Try to convert if it's a string (JSON)
            if (data is string jsonString)
            {
                try
                {
                    return JsonConvert.DeserializeObject<T>(jsonString);
                }
                catch (JsonException)
                {
                    return default;
                }
            }
        }
        
        return default;
    }

    /// <summary>
    /// Sets test data
    /// </summary>
    /// <param name="key">Data key</param>
    /// <param name="data">Test data</param>
    public void SetData(string key, object data)
    {
        _testData[key] = data;
    }

    /// <summary>
    /// Loads test data from a JSON file
    /// </summary>
    /// <param name="fileName">File name (without extension)</param>
    /// <returns>Test data</returns>
    public T? LoadFromFile<T>(string fileName)
    {
        var filePath = Path.Combine(_dataDirectory ?? "", $"{fileName}.json");
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test data file not found: {filePath}");
        }
        
        var json = File.ReadAllText(filePath);
        var data = JsonConvert.DeserializeObject<T>(json);
        
        // Cache the data
        _testData[fileName] = data!;
        
        return data;
    }

    /// <summary>
    /// Saves test data to a JSON file
    /// </summary>
    /// <param name="fileName">File name (without extension)</param>
    /// <param name="data">Data to save</param>
    public void SaveToFile(string fileName, object data)
    {
        if (_dataDirectory != null && !Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
        }
        
        var filePath = Path.Combine(_dataDirectory ?? "", $"{fileName}.json");
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(filePath, json);
        
        // Cache the data
        _testData[fileName] = data;
    }

    /// <summary>
    /// Gets all available test data keys
    /// </summary>
    /// <returns>Data keys</returns>
    public IEnumerable<string> GetAvailableKeys()
    {
        return _testData.Keys;
    }

    /// <summary>
    /// Clears all test data
    /// </summary>
    public void Clear()
    {
        _testData.Clear();
    }

    /// <summary>
    /// Removes specific test data
    /// </summary>
    /// <param name="key">Data key</param>
    /// <returns>True if removed, false if not found</returns>
    public bool RemoveData(string key)
    {
        return _testData.Remove(key);
    }

    /// <summary>
    /// Checks if test data exists
    /// </summary>
    /// <param name="key">Data key</param>
    /// <returns>True if exists</returns>
    public bool HasData(string key)
    {
        return _testData.ContainsKey(key);
    }

    /// <summary>
    /// Merges test data from another manager
    /// </summary>
    /// <param name="other">Other test data manager</param>
    /// <param name="overwrite">Whether to overwrite existing keys</param>
    public void Merge(TestDataManager other, bool overwrite = false)
    {
        foreach (var kvp in other._testData)
        {
            if (overwrite || !_testData.ContainsKey(kvp.Key))
            {
                _testData[kvp.Key] = kvp.Value;
            }
        }
    }

    private void LoadTestDataFromDirectory()
    {
        if (_dataDirectory == null || !Directory.Exists(_dataDirectory))
            return;

        var jsonFiles = Directory.GetFiles(_dataDirectory, "*.json");
        
        foreach (var file in jsonFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var json = File.ReadAllText(file);
                _testData[fileName] = json;
            }
            catch (Exception)
            {
                // Ignore files that can't be read
            }
        }
    }
}

/// <summary>
/// Factory for creating test data objects
/// </summary>
public static class TestDataFactory
{
    private static readonly Random _random = new();

    /// <summary>
    /// Creates a random string
    /// </summary>
    /// <param name="length">String length</param>
    /// <param name="includeNumbers">Include numbers</param>
    /// <param name="includeSpecialChars">Include special characters</param>
    /// <returns>Random string</returns>
    public static string RandomString(int length = 10, bool includeNumbers = true, bool includeSpecialChars = false)
    {
        const string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        
        var chars = letters;
        if (includeNumbers) chars += numbers;
        if (includeSpecialChars) chars += specialChars;
        
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[_random.Next(s.Length)]).ToArray());
    }

    /// <summary>
    /// Creates a random email address
    /// </summary>
    /// <param name="domain">Email domain</param>
    /// <returns>Random email</returns>
    public static string RandomEmail(string domain = "example.com")
    {
        var username = RandomString(8, true, false).ToLower();
        return $"{username}@{domain}";
    }

    /// <summary>
    /// Creates a random phone number
    /// </summary>
    /// <param name="format">Phone format (default: US format)</param>
    /// <returns>Random phone number</returns>
    public static string RandomPhoneNumber(string format = "(###) ###-####")
    {
        var result = format;
        while (result.Contains('#'))
        {
            result = result.ReplaceFirst("#", _random.Next(0, 10).ToString());
        }
        return result;
    }

    /// <summary>
    /// Creates a random integer within a range
    /// </summary>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <returns>Random integer</returns>
    public static int RandomInt(int min = 0, int max = 100)
    {
        return _random.Next(min, max + 1);
    }

    /// <summary>
    /// Creates a random decimal within a range
    /// </summary>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <param name="decimals">Number of decimal places</param>
    /// <returns>Random decimal</returns>
    public static decimal RandomDecimal(decimal min = 0, decimal max = 100, int decimals = 2)
    {
        var range = max - min;
        var randomValue = min + (decimal)_random.NextDouble() * range;
        return Math.Round(randomValue, decimals);
    }

    /// <summary>
    /// Creates a random date within a range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Random date</returns>
    public static DateTime RandomDate(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Now.AddYears(-1);
        var end = endDate ?? DateTime.Now.AddYears(1);
        
        var range = end - start;
        var randomTimeSpan = new TimeSpan((long)(_random.NextDouble() * range.Ticks));
        
        return start + randomTimeSpan;
    }

    /// <summary>
    /// Creates a random boolean
    /// </summary>
    /// <returns>Random boolean</returns>
    public static bool RandomBool()
    {
        return _random.Next(2) == 1;
    }

    /// <summary>
    /// Selects a random item from a collection
    /// </summary>
    /// <typeparam name="T">Item type</typeparam>
    /// <param name="items">Collection of items</param>
    /// <returns>Random item</returns>
    public static T RandomItem<T>(IEnumerable<T> items)
    {
        var itemList = items.ToList();
        if (!itemList.Any())
            throw new ArgumentException("Collection cannot be empty", nameof(items));
        
        return itemList[_random.Next(itemList.Count)];
    }

    /// <summary>
    /// Creates a random GUID
    /// </summary>
    /// <returns>Random GUID</returns>
    public static Guid RandomGuid()
    {
        return Guid.NewGuid();
    }

    /// <summary>
    /// Creates a random URL
    /// </summary>
    /// <param name="scheme">URL scheme</param>
    /// <param name="domain">Domain name</param>
    /// <returns>Random URL</returns>
    public static string RandomUrl(string scheme = "https", string domain = "example.com")
    {
        var path = RandomString(8, true, false).ToLower();
        return $"{scheme}://{domain}/{path}";
    }
}

/// <summary>
/// Extension methods for test data
/// </summary>
public static class TestDataExtensions
{
    /// <summary>
    /// Replaces the first occurrence of a string
    /// </summary>
    /// <param name="source">Source string</param>
    /// <param name="find">String to find</param>
    /// <param name="replace">Replacement string</param>
    /// <returns>Modified string</returns>
    public static string ReplaceFirst(this string source, string find, string replace)
    {
        var index = source.IndexOf(find, StringComparison.Ordinal);
        return index < 0 ? source : source.Remove(index, find.Length).Insert(index, replace);
    }
}

/// <summary>
/// Attribute to mark test data properties for automatic generation
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class TestDataAttribute : Attribute
{
    public string? Generator { get; set; }
    public object[]? Parameters { get; set; }

    public TestDataAttribute(string? generator = null, params object[] parameters)
    {
        Generator = generator;
        Parameters = parameters;
    }
}

/// <summary>
/// Generates test data objects using reflection and attributes
/// </summary>
public static class TestDataGenerator
{
    /// <summary>
    /// Generates a test data object of the specified type
    /// </summary>
    /// <typeparam name="T">Type to generate</typeparam>
    /// <returns>Generated object</returns>
    public static T Generate<T>() where T : new()
    {
        var instance = new T();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            if (!property.CanWrite) continue;

            var attribute = property.GetCustomAttributes(typeof(TestDataAttribute), false)
                .FirstOrDefault() as TestDataAttribute;

            var value = GenerateValueForProperty(property.PropertyType, attribute);
            if (value != null)
            {
                property.SetValue(instance, value);
            }
        }

        return instance;
    }

    private static object? GenerateValueForProperty(Type propertyType, TestDataAttribute? attribute)
    {
        var generator = attribute?.Generator;
        var parameters = attribute?.Parameters ?? Array.Empty<object>();

        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        return underlyingType.Name switch
        {
            nameof(String) => generator switch
            {
                "Email" => TestDataFactory.RandomEmail(),
                "Phone" => TestDataFactory.RandomPhoneNumber(),
                "Url" => TestDataFactory.RandomUrl(),
                _ => TestDataFactory.RandomString(parameters.Length > 0 ? (int)parameters[0] : 10)
            },
            nameof(Int32) => TestDataFactory.RandomInt(
                parameters.Length > 0 ? (int)parameters[0] : 0,
                parameters.Length > 1 ? (int)parameters[1] : 100),
            nameof(Decimal) => TestDataFactory.RandomDecimal(
                parameters.Length > 0 ? (decimal)parameters[0] : 0,
                parameters.Length > 1 ? (decimal)parameters[1] : 100),
            nameof(DateTime) => TestDataFactory.RandomDate(),
            nameof(Boolean) => TestDataFactory.RandomBool(),
            nameof(Guid) => TestDataFactory.RandomGuid(),
            _ => null
        };
    }
}

