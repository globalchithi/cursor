#!/bin/bash

# Script to run VaxHub Inventory API tests
echo "Running VaxHub Inventory API Tests..."

# Set proxy environment variables if needed
export API_TEST_PROXY_HOST=localhost
export API_TEST_PROXY_PORT=8888

# Run the specific inventory test
echo "Running inventory product API test..."
dotnet test --filter "TestCategory~inventory&TestCategory~proxy" --verbosity normal

# Check exit code
if [ $? -eq 0 ]; then
    echo "✅ Inventory API tests passed!"
else
    echo "❌ Inventory API tests failed!"
    exit 1
fi

echo "Inventory API tests completed successfully!"
