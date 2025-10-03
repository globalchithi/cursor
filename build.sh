#!/bin/bash

# API Test Framework Build Script

set -e

echo "🚀 Building API Test Framework..."

# Clean previous builds
echo "🧹 Cleaning previous builds..."
dotnet clean

# Restore packages
echo "📦 Restoring NuGet packages..."
dotnet restore

# Build the solution
echo "🔨 Building solution..."
dotnet build --configuration Release --no-restore

# Run framework tests
echo "🧪 Running framework tests..."
dotnet test tests/ApiTestFramework.Tests --configuration Release --no-build --verbosity normal

# Run sample tests (these might fail if pointing to real APIs)
echo "📋 Running sample tests..."
dotnet test samples/SampleApiTests --configuration Release --no-build --verbosity normal || echo "⚠️  Sample tests failed (expected if API endpoints are not available)"

echo "✅ Build completed successfully!"
echo ""
echo "📖 Next steps:"
echo "   - Check the samples/ directory for usage examples"
echo "   - Read the documentation in docs/ directory"
echo "   - Start creating your own API tests!"
echo ""
echo "🎯 Quick start:"
echo "   dotnet new nunit -n MyApiTests"
echo "   cd MyApiTests"
echo "   dotnet add reference ../src/ApiTestFramework/ApiTestFramework.csproj"
echo "   # Create your test classes inheriting from ApiTestBase"

