#!/bin/bash

# VaxHub Patient Appointment API Test Runner
# This script runs the VaxHub-specific patient appointment tests

set -e

echo "🚀 Running VaxHub Patient Appointment API Tests..."

# Set up .NET environment
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"

# Navigate to the test project directory
cd /Users/azaman/apiTest/cursor/samples/SampleApiTests

echo "📋 Available test scenarios:"
echo "  - VaxHub mobile headers with valid authentication"
echo "  - VaxHub response structure validation"
echo "  - VaxHub authentication error handling"

echo ""
echo "🧪 Running VaxHub tests..."

# Run only VaxHub-tagged tests
dotnet test SampleApiTests.csproj --filter "TestCategory=vaxhub" --logger "console;verbosity=normal"

echo ""
echo "✅ VaxHub tests completed!"
echo ""
echo "📖 Test scenarios covered:"
echo "   - Patient appointment creation with VaxHub mobile headers"
echo "   - Response validation and structure verification"
echo "   - Authentication error handling"
echo ""
echo "🔧 To run specific scenarios:"
echo "   dotnet test --filter \"TestCategory=vaxhub&TestCategory=success\""
echo "   dotnet test --filter \"TestCategory=vaxhub&TestCategory=validation\""
echo "   dotnet test --filter \"TestCategory=vaxhub&TestCategory=error-handling\""
