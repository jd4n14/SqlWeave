#!/bin/bash

# SqlWeave NuGet Packaging Script
# This script builds and packages both SqlWeave and SqlWeave.Npgsql projects

set -e  # Exit on any error

# Add dotnet to PATH
export PATH="/opt/homebrew/bin:$PATH"

echo "🏗️  Building SqlWeave Solution..."

# Clean previous builds
dotnet clean -c Release

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore

# Build in Release mode
echo "🔨 Building in Release mode..."
dotnet build -c Release --no-restore

# Pack the main SqlWeave project
echo "📦 Packing SqlWeave..."
dotnet pack src/SqlWeave/SqlWeave.csproj -c Release --no-build -o packages

# Pack the SqlWeave.Npgsql project
echo "📦 Packing SqlWeave.Npgsql..."
dotnet pack src/SqlWeave.Npgsql/SqlWeave.Npgsql.csproj -c Release --no-build -o packages

echo "✅ Packaging completed successfully!"
echo "📁 Packages are located in: ./packages/"

# List generated packages
echo ""
echo "Generated packages:"
ls -la packages/*.nupkg packages/*.snupkg 2>/dev/null || echo "No packages found"
