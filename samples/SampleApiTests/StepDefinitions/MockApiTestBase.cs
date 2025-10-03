using SampleApiTests.StepDefinitions;

namespace SampleApiTests.StepDefinitions;

/// <summary>
/// Base class for Mock API tests using SpecFlow
/// </summary>
public abstract class MockApiTestBase : SpecFlowTestBase
{
    /// <summary>
    /// Gets the environment name for mock API tests
    /// </summary>
    protected override string Environment => "MockApi";

    /// <summary>
    /// Gets the configuration file path for mock API tests
    /// </summary>
    protected override string? ConfigurationFile => "testsettings.json";
}
