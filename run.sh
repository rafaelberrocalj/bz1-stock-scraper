#!/bin/bash

# Navigate to the source directory
cd src

# Build and run the .NET project
dotnet build
dotnet run

# Move the generated tickersData.json to the parent directory
mv ./tickersData.json ../tickersData.json

# Navigate back to the repository root
cd ..

# Check if tickersData.json has changed
if git diff --quiet tickersData.json; then
    echo "No changes detected in tickersData.json"
else
    # Add changes to git
    git add tickersData.json

    # Commit the changes with a message
    git commit -m "update tickersData.json"

    # Push the changes to the remote repository
    git push
fi