@authentication
Feature: Patient Appointment Management API with Authentication
  As an API consumer
  I want to create new patient appointments with proper authentication
  So that I can securely manage patient data

  @smoke @auth
  Scenario: Create a new patient appointment with Bearer token authentication
    Given the API base URL is "https://vhapistg.vaxcare.com"
    And the API endpoint is "/api/patients/appointment"
    And I am authenticated with Bearer token "YOUR_ACTUAL_BEARER_TOKEN"
    And I have valid patient appointment data with random lastName:
      | Field              | Value                   |
      | firstName          | Tammy                   |
      | dob                | 1985-10-02 00:00:00.000 |
      | gender             | 0                       |
      | phoneNumber        | 1234567890              |
      | state              | FL                      |
      | primaryInsuranceId | 1000023151              |
      | uninsured          | false                   |
      | primaryMemberId    | abc123                  |
      | clinicId           | 89534                   |
      | date               | 2025-10-02T12:30:00Z    |
      | providerId         | 0                       |
      | visitType          | Well                    |
    When I send a POST request to "/api/patients/appointment" with the patient appointment data
    Then the response status should be 201 Created
    And the response time should be less than 5 seconds
    And the response should contain "Content-Type" header with "application/json"
    And the response should contain the created patient appointment data
    And the patient should have a valid appointment ID
    And the patient firstName should match "Tammy"
    And the appointment should be scheduled for "2025-10-02T12:30:00Z"
