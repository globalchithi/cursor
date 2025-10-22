@vaxhub @smoke
Feature: Patient Appointment Management API with VaxHub Headers
  As an API consumer
  I want to create new patient appointments with VaxHub mobile headers
  So that I can securely manage patient data from mobile applications

  @vaxhub @success @cleanup-json-file
  Scenario: Create a new patient appointment with VaxHub mobile headers using JSON file
    Given the VaxHub API base URL is "https://vhapistg.vaxcare.com"
    And the VaxHub API endpoint is "/api/patients/appointments"
    And I have VaxHub mobile headers configured
    And I have patient appointment JSON data with unique lastName:
      | Field                    | Value                   |
      | firstName               | Test                    |
      | lastName                | Patient00989           |
      | dob                     | 1990-07-07 00:00:00.000 |
      | gender                  | 0                       |
      | phoneNumber             | 5555555555             |
      | primaryInsuranceId      | 12                      |
      | paymentMode             | InsurancePay            |
      | primaryMemberId         | MEM123                  |
      | primaryGroupId          | GRP456                  |
      | relationshipToInsured    | Self                    |
      | insuranceName           | Cigna                  |
      | mbi                     |                         |
      | stock                   | Private                 |
      | SSN                     |                         |
      | clinicId                | 10808                   |
      | date                    | 2025-10-16T20:00:00Z    |
      | providerId              | 100001877              |
      | initialPaymentMode      | InsurancePay            |
      | visitType               | Well                    |
    When I send a POST request to "/api/patients/appointment" using the JSON file
    Then the VaxHub response status should be 201 Created
    And the VaxHub response time should be less than 5 seconds
    And the response should contain "Content-Type" header with "application/json"
    And the response should contain the created patient appointment data
    And the patient should have a valid appointment ID
    And the patient firstName should match "Test"
    And the appointment should be scheduled for "2025-10-16T20:00:00Z"

  @vaxhub @success @legacy
  Scenario: Create a new patient appointment with VaxHub mobile headers (legacy in-memory object approach)
    Given the VaxHub API base URL is "https://vhapistg.vaxcare.com"
    And the VaxHub API endpoint is "/api/patients/appointments"
    And I have VaxHub mobile headers configured
    And I have valid patient appointment data with VaxHub format and unique lastName:
      | Field                    | Value                   |
      | firstName               | Test                    |
      | lastName                | Patient00989           |
      | dob                     | 1990-07-07 00:00:00.000 |
      | gender                  | 0                       |
      | phoneNumber             | 5555555555             |
      | primaryInsuranceId      | 12                      |
      | paymentMode             | InsurancePay            |
      | primaryMemberId         | MEM123                  |
      | primaryGroupId          | GRP456                  |
      | relationshipToInsured    | Self                    |
      | insuranceName           | Cigna                  |
      | mbi                     |                         |
      | stock                   | Private                 |
      | SSN                     |                         |
      | clinicId                | 10808                   |
      | date                    | 2025-10-16T20:00:00Z    |
      | providerId              | 100001877              |
      | initialPaymentMode      | InsurancePay            |
      | visitType               | Well                    |
    When I send a POST request to "/api/patients/appointment" with the VaxHub patient appointment data
    Then the VaxHub response status should be 201 Created
    And the VaxHub response time should be less than 5 seconds
    And the response should contain "Content-Type" header with "application/json"
    And the response should contain the created patient appointment data
    And the patient should have a valid appointment ID
    And the patient firstName should match "Test"
    And the appointment should be scheduled for "2025-10-16T20:00:00Z"

  @vaxhub @validation
  Scenario: Create patient appointment with VaxHub headers - validate response structure
    Given the VaxHub API base URL is "https://vhapistg.vaxcare.com"
    And the VaxHub API endpoint is "/api/patients/appointments"
    And I have VaxHub mobile headers configured
    And I have valid patient appointment data with VaxHub format and unique lastName:
      | Field                    | Value                   |
      | firstName               | Alice                   |
      | lastName                | Johnson                 |
      | dob                     | 1985-05-15 00:00:00.000 |
      | gender                  | 1                       |
      | phoneNumber             | 5551234567             |
      | primaryInsuranceId      | 15                      |
      | paymentMode             | InsurancePay            |
      | primaryMemberId         | MEM123                  |
      | primaryGroupId          | GRP456                  |
      | relationshipToInsured    | Self                    |
      | insuranceName           | Aetna                   |
      | mbi                     | 123456789              |
      | stock                   | Private                 |
      | SSN                     |                         |
      | clinicId                | 10809                  |
      | date                    | 2025-10-17T14:00:00Z    |
      | providerId              | 100001878              |
      | initialPaymentMode      | InsurancePay            |
      | visitType               | Checkup                 |
    When I send a POST request to "/api/patients/appointment" with the VaxHub patient appointment data
    Then the VaxHub response status should be 201 Created
    And the VaxHub response time should be less than 3 seconds
    And the appointment should be successfully created

  @vaxhub @error-handling
  Scenario: Create patient appointment with invalid VaxHub identifier
    Given the VaxHub API base URL is "https://vhapistg.vaxcare.com"
    And the VaxHub API endpoint is "/api/patients/appointments"
    And I have invalid VaxHub mobile headers configured
    And I have valid patient appointment data with VaxHub format and unique lastName:
      | Field                    | Value                   |
      | firstName               | Bob                     |
      | lastName                | Wilson                  |
      | dob                     | 1988-12-25 00:00:00.000 |
      | gender                  | 0                       |
      | phoneNumber             | 5559876543             |
      | primaryInsuranceId      | 20                      |
      | paymentMode             | InsurancePay            |
      | primaryMemberId         | MEM789                  |
      | primaryGroupId          | GRP012                  |
      | relationshipToInsured    | Self                    |
      | insuranceName           | BlueCross               |
      | mbi                     | 987654321              |
      | stock                   | Private                 |
      | SSN                     |                         |
      | clinicId                | 10810                   |
      | date                    | 2025-10-18T16:00:00Z    |
      | providerId              | 100001879              |
      | initialPaymentMode      | InsurancePay            |
      | visitType               | Consultation            |
    When I send a POST request to "/api/patients/appointment" with the VaxHub patient appointment data
    Then the VaxHub response status should be 401 Unauthorized
    And the response should contain an authentication error message
