@smoke @direct-auth
Feature: Direct Bearer Token Authentication Example
  As a developer
  I want to see how to use Bearer token directly in scenarios
  So that I can understand the authentication setup

  Scenario: Create patient appointment with direct Bearer token
    Given the API base URL is "https://vhapistg.vaxcare.com"
    And the API endpoint is "/api/patients/appointment"
    And I am authenticated with Bearer token "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
    And I have valid patient appointment data with random lastName:
      | Field              | Value                   |
      | firstName          | John                    |
      | lastName           | Smith                   |
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
