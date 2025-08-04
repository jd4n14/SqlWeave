#!/bin/bash

# Git commit and push script for SqlWeave
# This script commits all NuGet deployment configurations and pushes to GitHub

set -e

echo "🔄 Preparing SqlWeave for GitHub deployment..."

# Check if we're in a git repository
if [ ! -d ".git" ]; then
    echo "❌ Error: Not in a git repository"
    echo "Please run: git init && git remote add origin https://github.com/jd4n14/SqlWeave.git"
    exit 1
fi

# Check git status
echo "📊 Current git status:"
git status --porcelain

# Add all files
echo "📦 Adding files to git..."
git add .

# Show what will be committed
echo "📋 Files to be committed:"
git diff --cached --name-only

# Commit with descriptive message
echo "💾 Committing changes..."
git commit -m "🚀 Prepare SqlWeave for NuGet deployment

- Configure NuGet package metadata for both SqlWeave and SqlWeave.Npgsql
- Add automated build and packaging scripts
- Update documentation with GitHub URLs
- Add comprehensive README.md with usage examples
- Include MIT license
- Set up centralized version management
- Create deployment guides and troubleshooting docs

Ready for NuGet publication! 📦✨"

# Push to GitHub
echo "🚀 Pushing to GitHub..."
git push origin main || git push origin master || {
    echo "❌ Push failed. Please check your remote configuration."
    echo "Current remotes:"
    git remote -v
    exit 1
}

echo "✅ Successfully pushed to GitHub!"
echo "🌐 Repository: https://github.com/jd4n14/SqlWeave"
echo ""
echo "🎯 Next steps:"
echo "1. Get your NuGet API key from https://www.nuget.org/account/apikeys"
echo "2. Run: ./publish.sh YOUR_API_KEY"
echo "3. Your packages will be available globally in ~10 minutes! 🎉"
