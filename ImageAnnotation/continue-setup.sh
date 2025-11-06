#!/bin/bash
set -e

# Create WPF project directory manually
mkdir -p src/ImageAnnotation.App.Wpf

# Create the test projects
dotnet new xunit -n ImageAnnotation.Domain.Tests -o tests/ImageAnnotation.Domain.Tests -f net8.0
dotnet sln add tests/ImageAnnotation.Domain.Tests/ImageAnnotation.Domain.Tests.csproj

dotnet new xunit -n ImageAnnotation.Integration.Tests -o tests/ImageAnnotation.Integration.Tests -f net8.0
dotnet sln add tests/ImageAnnotation.Integration.Tests/ImageAnnotation.Integration.Tests.csproj

echo "Test projects created successfully!"
