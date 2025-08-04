#!/bin/bash

# Version management script for SqlWeave
# Usage: ./version.sh [major|minor|patch|preview]

set -e

VERSION_FILE="Directory.Build.props"

if [ ! -f "$VERSION_FILE" ]; then
    echo "❌ Error: $VERSION_FILE not found"
    exit 1
fi

# Get current version
CURRENT_VERSION=$(grep -o '<VersionPrefix>[^<]*' "$VERSION_FILE" | sed 's/<VersionPrefix>//')
CURRENT_SUFFIX=$(grep -o '<VersionSuffix>[^<]*' "$VERSION_FILE" | sed 's/<VersionSuffix>//' || echo "")

echo "Current version: $CURRENT_VERSION${CURRENT_SUFFIX:+-$CURRENT_SUFFIX}"

if [ -z "$1" ]; then
    echo "Usage: ./version.sh [major|minor|patch|preview|release]"
    echo ""
    echo "Commands:"
    echo "  major   - Increment major version (1.0.0 -> 2.0.0)"
    echo "  minor   - Increment minor version (1.0.0 -> 1.1.0)"
    echo "  patch   - Increment patch version (1.0.0 -> 1.0.1)"
    echo "  preview - Increment preview version (preview1 -> preview2)"
    echo "  release - Remove preview suffix"
    exit 0
fi

COMMAND=$1

# Parse current version
IFS='.' read -r MAJOR MINOR PATCH <<< "$CURRENT_VERSION"

case $COMMAND in
    "major")
        MAJOR=$((MAJOR + 1))
        MINOR=0
        PATCH=0
        NEW_SUFFIX=""
        ;;
    "minor")
        MINOR=$((MINOR + 1))
        PATCH=0
        NEW_SUFFIX=""
        ;;
    "patch")
        PATCH=$((PATCH + 1))
        NEW_SUFFIX=""
        ;;
    "preview")
        if [[ $CURRENT_SUFFIX =~ preview([0-9]+) ]]; then
            PREVIEW_NUM=$((${BASH_REMATCH[1]} + 1))
            NEW_SUFFIX="preview$PREVIEW_NUM"
        else
            NEW_SUFFIX="preview1"
        fi
        ;;
    "release")
        NEW_SUFFIX=""
        ;;
    *)
        echo "❌ Error: Unknown command '$COMMAND'"
        echo "Use: major, minor, patch, preview, or release"
        exit 1
        ;;
esac

NEW_VERSION="$MAJOR.$MINOR.$PATCH"

# Update Directory.Build.props
sed -i.bak "s|<VersionPrefix>.*</VersionPrefix>|<VersionPrefix>$NEW_VERSION</VersionPrefix>|" "$VERSION_FILE"
sed -i.bak "s|<VersionSuffix>.*</VersionSuffix>|<VersionSuffix>$NEW_SUFFIX</VersionSuffix>|" "$VERSION_FILE"

# Clean up backup file
rm "$VERSION_FILE.bak"

echo "✅ Version updated: $NEW_VERSION${NEW_SUFFIX:+-$NEW_SUFFIX}"

# Show the change
echo ""
echo "Updated $VERSION_FILE:"
grep -A 1 -B 1 "VersionPrefix\|VersionSuffix" "$VERSION_FILE"
