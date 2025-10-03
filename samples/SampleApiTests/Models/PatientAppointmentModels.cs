using System.Text.Json.Serialization;

namespace SampleApiTests.Models;

public class PatientAppointmentRequest
{
    [JsonPropertyName("newPatient")]
    public NewPatient NewPatient { get; set; } = null!;

    [JsonPropertyName("clinicId")]
    public int ClinicId { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = null!;

    [JsonPropertyName("providerId")]
    public int ProviderId { get; set; }

    [JsonPropertyName("visitType")]
    public string VisitType { get; set; } = null!;
}

public class NewPatient
{
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("dob")]
    public string Dob { get; set; } = null!;

    [JsonPropertyName("gender")]
    public int Gender { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("paymentInformation")]
    public PaymentInformation PaymentInformation { get; set; } = null!;
}

public class PaymentInformation
{
    [JsonPropertyName("primaryInsuranceId")]
    public int PrimaryInsuranceId { get; set; }

    [JsonPropertyName("uninsured")]
    public bool Uninsured { get; set; }

    [JsonPropertyName("primaryMemberId")]
    public string? PrimaryMemberId { get; set; }
}

public class PatientAppointmentResponse
{
    [JsonPropertyName("appointmentId")]
    public int? AppointmentId { get; set; }

    [JsonPropertyName("patientId")]
    public int? PatientId { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("dob")]
    public string? Dob { get; set; }

    [JsonPropertyName("gender")]
    public int? Gender { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("state")]
    public string? State { get; set; }

    [JsonPropertyName("clinicId")]
    public int? ClinicId { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("providerId")]
    public int? ProviderId { get; set; }

    [JsonPropertyName("visitType")]
    public string? VisitType { get; set; }

    [JsonPropertyName("paymentInformation")]
    public PaymentInformationResponse? PaymentInformation { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

public class PaymentInformationResponse
{
    [JsonPropertyName("primaryInsuranceId")]
    public int? PrimaryInsuranceId { get; set; }

    [JsonPropertyName("uninsured")]
    public bool? Uninsured { get; set; }

    [JsonPropertyName("primaryMemberId")]
    public string? PrimaryMemberId { get; set; }
}

public class PatientAppointmentErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("details")]
    public string? Details { get; set; }

    [JsonPropertyName("validationErrors")]
    public List<ValidationError>? ValidationErrors { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }
}

public class ValidationError
{
    [JsonPropertyName("field")]
    public string? Field { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }
}

// Mock API Response Models for testing
public class MockApiUserResponse
{
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("userId")]
    public int? UserId { get; set; }

    [JsonPropertyName("id")]
    public int? Id { get; set; }
}

public class MockPatientAppointmentResponse
{
    [JsonPropertyName("appointmentId")]
    public string? AppointmentId { get; set; }

    [JsonPropertyName("patientId")]
    public string? PatientId { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("clinicId")]
    public int? ClinicId { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("visitType")]
    public string? VisitType { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
