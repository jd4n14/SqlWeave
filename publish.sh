#!/bin/bash

# SqlWeave NuGet Publishing Script
# This script publishes both SqlWeave and SqlWeave.Npgsql packages to NuGet.org

set -e  # Exit on any error

# Add dotnet to PATH
export PATH="/opt/homebrew/bin:$PATH"

# Check if API key is provided
if [ -z "$1" ]; then
    echo "❌ Error: NuGet API key is required"
    echo "Usage: ./publish.sh <your-nuget-api-key>"
    echo ""
    echo "You can get your API key from: https://www.nuget.org/account/apikeys"
    exit 1
fi

API_KEY=$1

echo "🚀 Publishing SqlWeave packages to NuGet.org..."

# Ensure packages exist
if [ ! -d "packages" ]; then
    echo "❌ Error: packages directory not found. Please run ./pack.sh first"
    exit 1
fi

# Find the latest packages
SQLWEAVE_PACKAGE=$(find packages -name "SqlWeave.*.nupkg" ! -name "SqlWeave.Npgsql*" | sort -V | tail -1)
NPGSQL_PACKAGE=$(find packages -name "SqlWeave.Npgsql.*.nupkg" | sort -V | tail -1)

if [ -z "$SQLWEAVE_PACKAGE" ]; then
    echo "❌ Error: SqlWeave package not found"
    exit 1
fi

if [ -z "$NPGSQL_PACKAGE" ]; then
    echo "❌ Error: SqlWeave.Npgsql package not found"
    exit 1
fi

echo "📦 Found packages:"
echo "  - $SQLWEAVE_PACKAGE"
echo "  - $NPGSQL_PACKAGE"
echo ""

# Publish SqlWeave first (dependency)
echo "🚀 Publishing SqlWeave..."
dotnet nuget push "$SQLWEAVE_PACKAGE" --api-key "$API_KEY" --source https://api.nuget.org/v3/index.json

# Wait a moment for NuGet to process
echo "⏳ Waiting for NuGet to process SqlWeave..."
sleep 10

# Publish SqlWeave.Npgsql
echo "🚀 Publishing SqlWeave.Npgsql..."
dotnet nuget push "$NPGSQL_PACKAGE" --api-key "$API_KEY" --source https://api.nuget.org/v3/index.json

echo ""
echo "✅ Publishing completed successfully!"
echo "🎉 Your packages should be available at:"
echo "  - https://www.nuget.org/packages/SqlWeave/"
echo "  - https://www.nuget.org/packages/SqlWeave.Npgsql/"
echo ""
echo "Note: It may take a few minutes for packages to appear in search results."
