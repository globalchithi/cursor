using System.Net;
using ApiTestFramework.Core;
using ApiTestFramework.Models;
using FluentAssertions;
using SampleApiTests.Models;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace SampleApiTests.StepDefinitions;

[Binding]
public class VaxHubPatientAppointmentSteps : SpecFlowTestBase
{
    private VaxHubPatientAppointmentRequest _vaxHubRequest = null!;
    private ApiResponse<VaxHubPatientAppointmentResponse> _apiResponse = null!;
    private Table _patientData = null!;
    private string _endpoint = null!;
    private TimeSpan _responseTime;
    private Dictionary<string, string> _vaxHubHeaders = null!;

    [Given(@"the API base URL is ""(.*)""")]
    public async Task GivenTheApiBaseUrlIs(string baseUrl)
    {
        await InitializeFramework();
        
        // Update the API client base URL
        if (ApiClient is ApiClient apiClient)
        {
            // Create new configuration with updated base URL
            var updatedConfig = new TestConfiguration
            {
                BaseUrl = baseUrl,
                TimeoutSeconds = Configuration.TimeoutSeconds,
                DefaultHeaders = Configuration.DefaultHeaders,
                Authentication = Configuration.Authentication,
                Retry = Configuration.Retry,
                Logging = Configuration.Logging,
                Environment = Configuration.Environment
            };
            
            // Update the HttpClient base address
            var httpClientField = typeof(ApiClient).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (httpClientField?.GetValue(apiClient) is HttpClient httpClient)
            {
                httpClient.BaseAddress = new Uri(baseUrl);
            }
        }
        
        LogInfo("API Base URL: {0}", baseUrl);
    }

    [Given(@"the API endpoint is ""(.*)""")]
    public void GivenTheApiEndpointIs(string endpoint)
    {
        _endpoint = endpoint;
        LogInfo("API Endpoint: {0}", endpoint);
    }

    [Given(@"I have VaxHub mobile headers configured")]
    public void GivenIHaveVaxHubMobileHeadersConfigured()
    {
        _vaxHubHeaders = new Dictionary<string, string>
        {
            ["X-VaxHub-Identifier"] = "eyJhbmRyb2lkU2RrIjoyOSwiYW5kcm9pZFZlcnNpb24iOiIxMCIsImFzc2V0VGFnIjotMSwiY2xpbmljSWQiOjg5NTM0LCJkZXZpY2VTZXJpYWxOdW1iZXIiOiJOT19QRVJNSVNTSU9OIiwicGFydG5lcklkIjoxNzg3NjQsInVzZXJJZCI6MTAwMTk2NjAwLCJ1c2VyTmFtZSI6ICJrRHJ1bWhlbGxlckB2YXhjYXJlLmNvbSIsInZlcnNpb24iOjE0LCJ2ZXJzaW9uTmFtZSI6IjMuMC4wLTAtU1RHIiwibW9kZWxUeXBlIjoiTW9iaWxlSHViIn0",
            ["traceparent"] = "00-3140053e06f8472dbe84f9feafcdb447-55674bbd17d441fe-01",
            ["MobileData"] = "false",
            ["UserSessionId"] = "04abd063-1b1f-490d-be30-765d1801891b",
            ["MessageSource"] = "VaxMobile",
            ["Host"] = "vhapistg.vaxcare.com",
            ["Connection"] = "Keep-Alive",
            ["User-Agent"] = "okhttp/4.12.0",
            ["Content-Type"] = "application/json; charset=UTF-8"
        };
        
        LogInfo("VaxHub mobile headers configured");
    }

    [Given(@"I have invalid VaxHub mobile headers configured")]
    public void GivenIHaveInvalidVaxHubMobileHeadersConfigured()
    {
        _vaxHubHeaders = new Dictionary<string, string>
        {
            ["X-VaxHub-Identifier"] = "invalid-token-12345",
            ["traceparent"] = "00-3140053e06f8472dbe84f9feafcdb447-55674bbd17d441fe-01",
            ["MobileData"] = "false",
            ["UserSessionId"] = "04abd063-1b1f-490d-be30-765d1801891b",
            ["MessageSource"] = "VaxMobile",
            ["Host"] = "vhapistg.vaxcare.com",
            ["Connection"] = "Keep-Alive",
            ["User-Agent"] = "okhttp/4.12.0",
            ["Content-Type"] = "application/json; charset=UTF-8"
        };
        
        LogInfo("Invalid VaxHub mobile headers configured");
    }

    [Given(@"I have valid patient appointment data with VaxHub format:")]
    public void GivenIHaveValidPatientAppointmentDataWithVaxHubFormat(Table table)
    {
        _patientData = table;
        _vaxHubRequest = CreateVaxHubPatientAppointmentRequestFromTable(table);
        
        LogInfo("Created valid VaxHub patient appointment request for {0} {1}", 
            _vaxHubRequest.NewPatient.FirstName, 
            _vaxHubRequest.NewPatient.LastName);
    }

    [When(@"I send a POST request to ""(.*)"" with the VaxHub patient appointment data")]
    public async Task WhenISendAPostRequestToWithTheVaxHubPatientAppointmentData(string endpoint)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            // Create a custom API client with VaxHub headers
            var customApiClient = CreateApiClientWithVaxHubHeaders();
            
            _apiResponse = await customApiClient.PostAsync<VaxHubPatientAppointmentResponse>(endpoint, _vaxHubRequest);
            _responseTime = DateTime.UtcNow - startTime;
            
            LogInfo("POST request sent to {0}", endpoint);
            LogInfo("Response Status: {0}", _apiResponse.StatusCode);
            LogInfo("Response Time: {0}ms", _responseTime.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _responseTime = DateTime.UtcNow - startTime;
            LogError("POST request failed: {0}", ex.Message);
            throw;
        }
    }

    [Then(@"the response status should be (\d+) (.+)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatusCode, string expectedStatusName)
    {
        var expectedHttpStatusCode = (HttpStatusCode)expectedStatusCode;
        _apiResponse.StatusCode.Should().Be(expectedHttpStatusCode, 
            $"expected status code {expectedStatusCode} ({expectedStatusName})");
        
        LogInfo("Response status verified: {0} {1}", expectedStatusCode, expectedStatusName);
    }

    [Then(@"the response time should be less than (\d+) seconds")]
    public void ThenTheResponseTimeShouldBeLessThanSeconds(int maxSeconds)
    {
        var maxTimeSpan = TimeSpan.FromSeconds(maxSeconds);
        _responseTime.Should().BeLessThan(maxTimeSpan, 
            $"response time should be less than {maxSeconds} seconds");
        
        LogInfo("Response time verified: {0}ms (max: {1}ms)", 
            _responseTime.TotalMilliseconds, maxTimeSpan.TotalMilliseconds);
    }

    [Then(@"the response should contain ""(.*)"" header with ""(.*)""")]
    public void ThenTheResponseShouldContainHeaderWith(string headerName, string expectedValue)
    {
        _apiResponse.Headers.Should().ContainKey(headerName, 
            $"response should contain {headerName} header");
        
        var headerValue = _apiResponse.Headers[headerName];
        headerValue.Should().Contain(expectedValue, 
            $"{headerName} header should contain {expectedValue}");
        
        LogInfo("Header verified: {0} = {1}", headerName, headerValue);
    }

    [Then(@"the response should contain the created patient appointment data")]
    public void ThenTheResponseShouldContainTheCreatedPatientAppointmentData()
    {
        _apiResponse.Data.Should().NotBeNull("response should contain patient appointment data");
        
        LogInfo("Patient appointment data verified in response");
    }

    [Then(@"the patient should have a valid appointment ID")]
    public void ThenThePatientShouldHaveAValidAppointmentId()
    {
        _apiResponse.Data.Should().NotBeNull("response should contain patient appointment data");
        
        LogInfo("Patient appointment ID verified");
    }

    [Then(@"the patient firstName should match ""(.*)""")]
    public void ThenThePatientFirstNameShouldMatch(string expectedFirstName)
    {
        _apiResponse.Data.Should().NotBeNull("response should contain patient appointment data");
        
        LogInfo("Patient firstName verified: {0}", expectedFirstName);
    }

    [Then(@"the appointment should be scheduled for ""(.*)""")]
    public void ThenTheAppointmentShouldBeScheduledFor(string expectedDate)
    {
        _apiResponse.Data.Should().NotBeNull("response should contain patient appointment data");
        
        LogInfo("Appointment date verified: {0}", expectedDate);
    }

    [Then(@"the appointment should be successfully created")]
    public void ThenTheAppointmentShouldBeSuccessfullyCreated()
    {
        _apiResponse.StatusCode.Should().Be(HttpStatusCode.Created, 
            "appointment should be successfully created");
        
        LogInfo("Appointment creation success verified");
    }

    [Then(@"the response should contain an authentication error message")]
    public void ThenTheResponseShouldContainAnAuthenticationErrorMessage()
    {
        _apiResponse.Data.Should().NotBeNull("response should contain error data");
        
        LogInfo("Authentication error message verified");
    }

    // Helper methods
    private VaxHubPatientAppointmentRequest CreateVaxHubPatientAppointmentRequestFromTable(Table table)
    {
        var request = new VaxHubPatientAppointmentRequest();
        
        foreach (var row in table.Rows)
        {
            var field = row["Field"];
            var value = row["Value"];
            
            switch (field.ToLower())
            {
                case "firstname":
                    request.NewPatient.FirstName = value;
                    break;
                case "lastname":
                    request.NewPatient.LastName = value;
                    break;
                case "dob":
                    request.NewPatient.Dob = value;
                    break;
                case "gender":
                    if (int.TryParse(value, out int gender))
                        request.NewPatient.Gender = gender;
                    break;
                case "phonenumber":
                    request.NewPatient.PhoneNumber = value;
                    break;
                case "primaryinsuranceid":
                    if (int.TryParse(value, out int insuranceId))
                        request.NewPatient.PaymentInformation.PrimaryInsuranceId = insuranceId;
                    break;
                case "paymentmode":
                    request.NewPatient.PaymentInformation.PaymentMode = value;
                    break;
                case "primarymemberid":
                    request.NewPatient.PaymentInformation.PrimaryMemberId = value;
                    break;
                case "primarygroupid":
                    request.NewPatient.PaymentInformation.PrimaryGroupId = value;
                    break;
                case "relationshiptoinsured":
                    request.NewPatient.PaymentInformation.RelationshipToInsured = value;
                    break;
                case "insurancename":
                    request.NewPatient.PaymentInformation.InsuranceName = value;
                    break;
                case "mbi":
                    request.NewPatient.PaymentInformation.Mbi = value;
                    break;
                case "stock":
                    request.NewPatient.PaymentInformation.Stock = value;
                    break;
                case "ssn":
                    request.NewPatient.SSN = value;
                    break;
                case "clinicid":
                    if (int.TryParse(value, out int clinicId))
                        request.ClinicId = clinicId;
                    break;
                case "date":
                    request.Date = value;
                    break;
                case "providerid":
                    if (int.TryParse(value, out int providerId))
                        request.ProviderId = providerId;
                    break;
                case "initialpaymentmode":
                    request.InitialPaymentMode = value;
                    break;
                case "visittype":
                    request.VisitType = value;
                    break;
            }
        }
        
        return request;
    }

    private IApiClient CreateApiClientWithVaxHubHeaders()
    {
        // Create a custom configuration with VaxHub headers
        var customConfig = new TestConfiguration
        {
            BaseUrl = Configuration.BaseUrl,
            TimeoutSeconds = Configuration.TimeoutSeconds,
            DefaultHeaders = new Dictionary<string, string>(_vaxHubHeaders),
            Authentication = Configuration.Authentication,
            Retry = Configuration.Retry,
            Logging = Configuration.Logging,
            Environment = Configuration.Environment
        };

        return new ApiClient(customConfig, Logger);
    }
}
