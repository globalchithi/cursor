using System.Text.Json.Serialization;

namespace SampleApiTests.Models;

/// <summary>
/// VaxHub Patient Appointment Request Model
/// </summary>
public class VaxHubPatientAppointmentRequest
{
    [JsonPropertyName("newPatient")]
    public VaxHubNewPatient NewPatient { get; set; } = new();

    [JsonPropertyName("clinicId")]
    public int ClinicId { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("providerId")]
    public int ProviderId { get; set; }

    [JsonPropertyName("initialPaymentMode")]
    public string InitialPaymentMode { get; set; } = string.Empty;

    [JsonPropertyName("visitType")]
    public string VisitType { get; set; } = string.Empty;
}

/// <summary>
/// VaxHub New Patient Model
/// </summary>
public class VaxHubNewPatient
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("dob")]
    public string Dob { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public int Gender { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string PhoneNumber { get; set; } = string.Empty;

    [JsonPropertyName("paymentInformation")]
    public VaxHubPaymentInformation PaymentInformation { get; set; } = new();

    [JsonPropertyName("SSN")]
    public string SSN { get; set; } = string.Empty;
}

/// <summary>
/// VaxHub Payment Information Model
/// </summary>
public class VaxHubPaymentInformation
{
    [JsonPropertyName("primaryInsuranceId")]
    public int PrimaryInsuranceId { get; set; }

    [JsonPropertyName("paymentMode")]
    public string PaymentMode { get; set; } = string.Empty;

    [JsonPropertyName("primaryMemberId")]
    public string PrimaryMemberId { get; set; } = string.Empty;

    [JsonPropertyName("primaryGroupId")]
    public string PrimaryGroupId { get; set; } = string.Empty;

    [JsonPropertyName("relationshipToInsured")]
    public string RelationshipToInsured { get; set; } = string.Empty;

    [JsonPropertyName("insuranceName")]
    public string InsuranceName { get; set; } = string.Empty;

    [JsonPropertyName("mbi")]
    public string Mbi { get; set; } = string.Empty;

    [JsonPropertyName("stock")]
    public string Stock { get; set; } = string.Empty;
}

/// <summary>
/// VaxHub Patient Appointment Response Model
/// </summary>
public class VaxHubPatientAppointmentResponse
{
    [JsonPropertyName("appointmentId")]
    public int? AppointmentId { get; set; }

    [JsonPropertyName("patientId")]
    public int? PatientId { get; set; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("clinicId")]
    public int ClinicId { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("visitType")]
    public string VisitType { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("providerId")]
    public int ProviderId { get; set; }

    [JsonPropertyName("initialPaymentMode")]
    public string InitialPaymentMode { get; set; } = string.Empty;
}
