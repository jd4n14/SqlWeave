# SqlWeave NuGet Deployment Guide

## Prerequisites

1. **NuGet Account**: Create an account at [nuget.org](https://www.nuget.org)
2. **API Key**: Generate an API key from your [NuGet account settings](https://www.nuget.org/account/apikeys)
3. **.NET 9 SDK**: Ensure you have .NET 9 SDK installed

## Quick Start

### 1. Build and Package

```bash
# Make scripts executable (first time only)
chmod +x *.sh

# Build and create packages
./pack.sh
```

This will create `.nupkg` and `.snupkg` files in the `packages/` directory.

### 2. Publish to NuGet

```bash
# Replace YOUR_API_KEY with your actual NuGet API key
./publish.sh YOUR_API_KEY
```

## Version Management

Use the `version.sh` script to manage versions:

```bash
# Increment patch version (1.0.0 -> 1.0.1)
./version.sh patch

# Increment minor version (1.0.0 -> 1.1.0)
./version.sh minor

# Increment major version (1.0.0 -> 2.0.0)
./version.sh major

# Increment preview version (preview1 -> preview2)
./version.sh preview

# Remove preview suffix (release version)
./version.sh release
```

## Manual Commands

If you prefer manual control:

### Build and Pack

```bash
# Clean and restore
dotnet clean -c Release
dotnet restore

# Build
dotnet build -c Release

# Pack SqlWeave
dotnet pack src/SqlWeave/SqlWeave.csproj -c Release -o packages

# Pack SqlWeave.Npgsql
dotnet pack src/SqlWeave.Npgsql/SqlWeave.Npgsql.csproj -c Release -o packages
```

### Publish

```bash
# Publish SqlWeave first (it's a dependency)
dotnet nuget push packages/SqlWeave.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Wait a moment, then publish SqlWeave.Npgsql
dotnet nuget push packages/SqlWeave.Npgsql.*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Package Structure

### SqlWeave (Core Package)
- Contains Source Generators and Interceptors
- Core functionality for data mapping
- No database-specific dependencies

### SqlWeave.Npgsql (Extension Package)
- Extension methods for NpgsqlConnection
- Depends on SqlWeave core package
- Includes Npgsql dependency

## Important Notes

1. **Preview Versions**: Initial releases use `-preview1` suffix for early testing
2. **Source Generator Packaging**: The core package includes special configuration for Source Generator distribution
3. **Symbol Packages**: `.snupkg` files are automatically generated for debugging support
4. **Version Synchronization**: Both packages should always have the same version number
5. **Publishing Order**: Always publish SqlWeave before SqlWeave.Npgsql (dependency order)

## Troubleshooting

### Package Not Found After Publishing
- Wait 5-10 minutes for NuGet indexing
- Check package status at nuget.org/packages/[PackageName]

### Source Generator Not Working
- Ensure target project uses .NET 9 and C# 13
- Verify `InterceptorsPreviewNamespaces` is configured
- Check that both packages have the same version

### Build Errors
- Clean solution: `dotnet clean`
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Restore packages: `dotnet restore`

## Links

- [NuGet Package Manager](https://www.nuget.org)
- [.NET CLI Pack Reference](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-pack)
- [.NET CLI Push Reference](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push)
