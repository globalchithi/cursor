# PowerShell script to test VaxHub Inventory Product API
# This script replicates the exact curl command provided by the user

# Parameters:
#   -ProxyHost <string>    : Proxy host (default: localhost)
#   -ProxyPort <int>       : Proxy port (default: 8888)
#   -UseEnvProxy           : Use proxy settings from environment variables API_TEST_PROXY_HOST and API_TEST_PROXY_PORT

param(
    [string]$ProxyHost,
    [int]$ProxyPort = 8888,
    [switch]$UseEnvProxy
)

# Check if proxy should be used (environment variable or parameter)
$useProxy = $false
$proxyHostValue = "localhost"
$proxyPortValue = "8888"

if ($UseEnvProxy -and $env:API_TEST_PROXY_HOST) {
    $useProxy = $true
    $proxyHostValue = $env:API_TEST_PROXY_HOST
    $proxyPortValue = if ($env:API_TEST_PROXY_PORT) { $env:API_TEST_PROXY_PORT } else { "8888" }
} elseif ($ProxyHost) {
    $useProxy = $true
    $proxyHostValue = $ProxyHost
    $proxyPortValue = $ProxyPort.ToString()
}

$curlArgs = @(
    "--insecure",
    "--proxy", "http://$proxyHostValue`:$proxyPortValue",
    "https://vhapistg.vaxcare.com/api/inventory/product/v2",
    "-X", "GET",
    "-H", "IsCalledByJob: true",
    "-H", "X-VaxHub-Identifier: eyJhbmRyb2lkU2RrIjoyOSwiYW5kcm9pZFZlcnNpb24iOiIxMCIsImFzc2V0VGFnIjotMSwiY2xpbmljSWQiOjg5NTM0LCJkZXZpY2VTZXJpYWxOdW1iZXIiOiJOT19QRVJNSVNTSU9OIiwicGFydG5lcklkIjoxNzg3NjQsInVzZXJJZCI6MCwidXNlck5hbWUiOiAiIiwidmVyc2lvbiI6MTQsInZlcnNpb25OYW1lIjoiMy4wLjAtMC1TVEciLCJtb2RlbFR5cGUiOiJNb2JpbGVIdWIifQ==",
    "-H", "traceparent: 00-3140053e06f8472dbe84f9feafcdb447-55674bbd17d441fe-01",
    "-H", "MobileData: false",
    "-H", "UserSessionId: NO USER LOGGED IN",
    "-H", "MessageSource: VaxMobile",
    "-H", "Host: vhapistg.vaxcare.com",
    "-H", "Connection: Keep-Alive",
    "-H", "User-Agent: okhttp/4.12.0"
)

Write-Host "Testing VaxHub Inventory Product API..." -ForegroundColor Yellow
Write-Host "Endpoint: https://vhapistg.vaxcare.com/api/inventory/product/v2" -ForegroundColor Cyan
Write-Host "Method: GET" -ForegroundColor Cyan

if ($useProxy) {
    Write-Host "Using proxy: http://$proxyHostValue`:$proxyPortValue" -ForegroundColor Cyan
}

try {
    Write-Host "Sending GET request..." -ForegroundColor Yellow
    $response = & curl.exe $curlArgs

    Write-Host "Response received:" -ForegroundColor Green
    Write-Host $response -ForegroundColor White
}
catch {
    Write-Host "Error occurred: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nInventory API test completed!" -ForegroundColor Cyan
