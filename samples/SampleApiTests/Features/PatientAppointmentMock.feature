@mock @smoke
Feature: Patient Appointment Management API - Mock Tests
  As an API consumer
  I want to test patient appointment creation with mock responses
  So that I can validate the test framework and response handling

  @mock @success
  Scenario: Create a new patient appointment with mock API (assumes valid Bearer token)
    Given the API base URL is "https://jsonplaceholder.typicode.com"
    And the API endpoint is "/posts"
    And I am authenticated with Bearer token "mock-valid-token-12345"
    And I have valid patient appointment data with random lastName:
      | Field              | Value                   |
      | firstName          | Alice                   |
      | lastName           | Johnson                 |
      | dob                | 1990-05-15 00:00:00.000 |
      | gender             | 1                       |
      | phoneNumber        | 5551234567              |
      | state              | CA                      |
      | primaryInsuranceId | 1000023152              |
      | uninsured          | false                   |
      | primaryMemberId    | xyz789                  |
      | clinicId           | 89535                   |
      | date               | 2025-10-03T10:00:00Z    |
      | providerId         | 1                       |
      | visitType          | Checkup                 |
    When I send a POST request to "/posts" with the patient appointment data for mock API
    Then the response status should be 201 Created
    And the response time should be less than 5 seconds
    And the response should contain "Content-Type" header with "application/json; charset=utf-8"
    And the response should contain the created patient appointment data
    And the mock API response should contain a valid appointment ID
    And the mock API response should reflect the created patient data
    And the mock API response should have a creation timestamp
    And the appointment should be successfully created via mock API

  @mock @validation
  Scenario: Create patient appointment with mock API - validate response structure
    Given the API base URL is "https://jsonplaceholder.typicode.com"
    And the API endpoint is "/posts"
    And I am authenticated with Bearer token "mock-token-67890"
    And I have valid patient appointment data:
      | Field              | Value                   |
      | firstName          | Bob                     |
      | lastName           | Wilson                  |
      | dob                | 1985-12-25 00:00:00.000 |
      | gender             | 0                       |
      | phoneNumber        | 5559876543              |
      | state              | TX                      |
      | primaryInsuranceId | 1000023153              |
      | uninsured          | false                   |
      | primaryMemberId    | def456                  |
      | clinicId           | 89536                   |
      | date               | 2025-10-04T14:00:00Z    |
      | providerId         | 2                       |
      | visitType          | Consultation            |
    When I send a POST request to "/posts" with the patient appointment data for mock API
    Then the response status should be 201 Created
    And the response time should be less than 3 seconds
    And the response should contain "Content-Type" header with "application/json; charset=utf-8"
    And the appointment should be successfully created via mock API
    And the mock API response should reflect the created patient data

  @mock @error-handling
  Scenario: Create patient appointment with invalid Bearer token (mock error response)
    Given the API base URL is "https://jsonplaceholder.typicode.com"
    And the API endpoint is "/posts"
    And I am not authenticated
    And I have valid patient appointment data:
      | Field              | Value                   |
      | firstName          | Charlie                 |
      | lastName           | Brown                   |
      | dob                | 1988-03-10 00:00:00.000 |
      | gender             | 0                       |
      | phoneNumber        | 5551112222              |
      | state              | NY                      |
      | primaryInsuranceId | 1000023154              |
      | uninsured          | false                   |
      | primaryMemberId    | ghi789                  |
      | clinicId           | 89537                   |
      | date               | 2025-10-05T16:00:00Z    |
      | providerId         | 3                       |
      | visitType          | Follow-up               |
    When I send a POST request to "/posts" with the patient appointment data for mock API
    Then the response status should be 401 Unauthorized
    And the response should contain an authentication error message

  @mock @performance
  Scenario: Create patient appointment with mock API - performance test
    Given the API base URL is "https://jsonplaceholder.typicode.com"
    And the API endpoint is "/posts"
    And I am authenticated with Bearer token "performance-test-token"
    And I have valid patient appointment data with random lastName:
      | Field              | Value                   |
      | firstName          | Diana                   |
      | lastName           | Prince                  |
      | dob                | 1992-07-20 00:00:00.000 |
      | gender             | 1                       |
      | phoneNumber        | 5554445555              |
      | state              | WA                      |
      | primaryInsuranceId | 1000023155              |
      | uninsured          | false                   |
      | primaryMemberId    | jkl012                  |
      | clinicId           | 89538                   |
      | date               | 2025-10-06T09:00:00Z    |
      | providerId         | 4                       |
      | visitType          | Annual                  |
    When I send a POST request to "/posts" with the patient appointment data for mock API
    Then the response status should be 201 Created
    And the response time should be less than 2 seconds
    And the response should contain "Content-Type" header with "application/json; charset=utf-8"
    And the response should contain the created patient appointment data
    And the mock API response should contain a valid appointment ID
    And the mock API response should reflect the created patient data
    And the mock API response should have a creation timestamp
    And the appointment should be successfully created via mock API
