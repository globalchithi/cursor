#!/bin/bash

# Updated VaxHub Patient Appointment API Test Runner
# This script runs the updated VaxHub tests with unique lastName generation

set -e

echo "🚀 Running Updated VaxHub Patient Appointment API Tests..."

# Set up .NET environment
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"

# Navigate to the test project directory
cd /Users/azaman/apiTest/cursor/samples/SampleApiTests

echo "📋 Updated test scenarios:"
echo "  - VaxHub mobile headers with updated identifier"
echo "  - Unique lastName generation with timestamp"
echo "  - Correct endpoint: /api/patients/appointment (singular)"
echo "  - VaxHub response structure validation"
echo "  - VaxHub authentication error handling"

echo ""
echo "🧪 Running updated VaxHub tests..."

# Run only VaxHub-tagged tests
dotnet test SampleApiTests.csproj --filter "TestCategory=vaxhub" --logger "console;verbosity=normal"

echo ""
echo "✅ Updated VaxHub tests completed!"
echo ""
echo "📖 Test scenarios covered:"
echo "   - Patient appointment creation with updated VaxHub identifier"
echo "   - Unique lastName generation (Patient_YYYYMMDDHHMMSS_RANDOM)"
echo "   - Correct API endpoint (/api/patients/appointment)"
echo "   - Response validation and structure verification"
echo "   - Authentication error handling"
echo ""
echo "🔧 To run specific scenarios:"
echo "   dotnet test --filter \"TestCategory=vaxhub&TestCategory=success\""
echo "   dotnet test --filter \"TestCategory=vaxhub&TestCategory=validation\""
echo "   dotnet test --filter \"TestCategory=vaxhub&TestCategory=error-handling\""
echo ""
echo "💡 PowerShell script also available:"
echo "   ./create-appointment.ps1"
