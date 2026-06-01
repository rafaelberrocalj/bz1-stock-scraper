#!/bin/bash

# Navigate to the source directory
cd src

output_file="./tickersData.json"

delete_output_file() {
    if [ -f "$output_file" ]; then
        rm "$output_file"
        echo "Deleted generated output file: $output_file"
    fi
}

# Build and run the .NET project
dotnet build
build_exit_code=$?

if [ $build_exit_code -ne 0 ]; then
    echo "dotnet build failed with exit code $build_exit_code. Stopping script."
    delete_output_file
    exit $build_exit_code
fi

dotnet run
run_exit_code=$?

if [ $run_exit_code -ne 0 ]; then
    echo "dotnet run failed with exit code $run_exit_code. Stopping script."
    delete_output_file
    exit $run_exit_code
fi

# Move the generated tickersData.json to the parent directory
mv "$output_file" ../tickersData.json

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
