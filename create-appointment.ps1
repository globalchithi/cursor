# PowerShell script to create patient appointment using VaxHub API
# This script replicates the exact curl command from the user
#
# Parameters:
#   -ProxyHost <string>    : Proxy host (default: localhost)
#   -ProxyPort <int>       : Proxy port (default: 8888)
#   -UseEnvProxy           : Use proxy settings from environment variables API_TEST_PROXY_HOST and API_TEST_PROXY_PORT
#   -Endpoint <string>     : API endpoint (default: /api/patients/appointment)
#   -Method <string>       : HTTP method (default: POST)

param(
    [string]$ProxyHost,
    [int]$ProxyPort = 8888,
    [switch]$UseEnvProxy,
    [string]$Endpoint = "/api/patients/appointment",
    [string]$Method = "POST"
)

# Create JSON without BOM
$jsonContent = @'
{
  "newPatient": {
    "firstName": "Test",
    "lastName": "Patient00989",
    "dob": "1990-07-07 00:00:00.000",
    "gender": 0,
    "phoneNumber": "5555555555",
    "paymentInformation": {
      "primaryInsuranceId": 12,
      "paymentMode": "InsurancePay",
      "primaryMemberId": "",
      "primaryGroupId": "",
      "relationshipToInsured": "Self",
      "insuranceName": "Cigna",
      "mbi": "",
      "stock": "Private"
    },
    "SSN": ""
  },
  "clinicId": 10808,
  "date": "2025-10-16T20:00:00Z",
  "providerId": 100001877,
  "initialPaymentMode": "InsurancePay",
  "visitType": "Well"
}
'@

# Generate unique lastName with timestamp
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
$randomSuffix = Get-Random -Minimum 1000 -Maximum 9999
$uniqueLastName = "Patient_$timestamp" + "_$randomSuffix"

# Update JSON with unique lastName
$jsonObject = $jsonContent | ConvertFrom-Json
$jsonObject.newPatient.lastName = $uniqueLastName
$updatedJsonContent = $jsonObject | ConvertTo-Json -Depth 10

# Save without BOM using UTF8NoBOM encoding
[System.IO.File]::WriteAllText("appointment.json", $updatedJsonContent, [System.Text.UTF8Encoding]::new($false))

Write-Host "Created appointment.json with unique lastName: $uniqueLastName" -ForegroundColor Green

# Now use curl with the file
Write-Host "Sending POST request to VaxHub API..." -ForegroundColor Yellow

# Check if proxy should be used (environment variable or parameter)
$useProxy = $false
$proxyHost = "localhost"
$proxyPort = "8888"

if ($UseEnvProxy -and $env:API_TEST_PROXY_HOST) {
    $useProxy = $true
    $proxyHost = $env:API_TEST_PROXY_HOST
    $proxyPort = if ($env:API_TEST_PROXY_PORT) { $env:API_TEST_PROXY_PORT } else { "8888" }
} elseif ($ProxyHost) {
    $useProxy = $true
    $proxyHost = $ProxyHost
    $proxyPort = $ProxyPort.ToString()
}

$baseUrl = "https://vhapistg.vaxcare.com"
$fullUrl = "$baseUrl$Endpoint"

# Default headers for patient appointment
$defaultHeaders = @(
    "-H", "X-VaxHub-Identifier: eyJhbmRyb2lkU2RrIjoyOSwiYW5kcm9pZFZlcnNpb24iOiIxMCIsImFzc2V0VGFnIjotMSwiY2xpbmljSWQiOjg5NTM0LCJkZXZpY2VTZXJpYWxOdW1iZXIiOiJOT19QRVJNSVNTSU9OIiwicGFydG5lcklkIjoxNzg3NjQsInVzZXJJZCI6MTAwMTg2ODk0LCJ1c2VyTmFtZSI6ICJxYXJvYm90QHZheGNhcmUuY29tIiwidmVyc2lvbiI6MTQsInZlcnNpb25OYW1lIjoiMy4wLjAtMC1TVEciLCJtb2RlbFR5cGUiOiJNb2JpbGVIdWIifQ==",
    "-H", "MobileData: false",
    "-H", "UserSessionId: 04abd063-1b1f-490d-be30-765d1801891b",
    "-H", "MessageSource: VaxMobile",
    "-H", "Host: vhapistg.vaxcare.com",
    "-H", "Connection: Keep-Alive",
    "-H", "User-Agent: okhttp/4.12.0"
)

$curlArgs = @(
    "--insecure",
    $fullUrl,
    "-X", $Method
)

# Add default headers
$curlArgs += $defaultHeaders

# Add content type and data for POST requests
if ($Method -eq "POST") {
    $curlArgs += @(
        "-H", "Content-Type: application/json; charset=UTF-8",
        "--data", "@appointment.json"
    )
}

# Add proxy configuration if enabled
if ($useProxy) {
    $curlArgs += "--proxy"
    $curlArgs += "http://$proxyHost`:$proxyPort"
    Write-Host "Using proxy: http://$proxyHost`:$proxyPort" -ForegroundColor Cyan
}

try {
    $response = & curl.exe $curlArgs
    
    Write-Host "Response received:" -ForegroundColor Green
    Write-Host $response -ForegroundColor White
}
catch {
    Write-Host "Error occurred: $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    # Clean up the temporary JSON file
    if (Test-Path "appointment.json") {
        Remove-Item "appointment.json"
        Write-Host "Cleaned up temporary appointment.json file" -ForegroundColor Gray
    }
}

Write-Host "`nScript completed!" -ForegroundColor Cyan
