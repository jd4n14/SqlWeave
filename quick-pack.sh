#!/bin/bash

# Simple pack script for SqlWeave
export PATH="/opt/homebrew/bin:$PATH"

echo "🏗️ Building and packing SqlWeave..."

# Clean and build
dotnet clean -c Release > /dev/null 2>&1
dotnet build -c Release --no-restore

if [ $? -eq 0 ]; then
    echo "✅ Build successful"
    
    # Create packages directory
    mkdir -p packages
    
    # Pack both projects
    dotnet pack src/SqlWeave/SqlWeave.csproj -c Release --no-build -o packages
    dotnet pack src/SqlWeave.Npgsql/SqlWeave.Npgsql.csproj -c Release --no-build -o packages
    
    echo "📦 Packages created:"
    ls -la packages/*.nupkg packages/*.snupkg 2>/dev/null
else
    echo "❌ Build failed"
    exit 1
fi
