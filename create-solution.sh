#!/bin/bash
set -e

# Create the ImageAnnotation directory structure
mkdir -p ImageAnnotation/{src,tests,docs}
cd ImageAnnotation

# Create new solution
dotnet new sln -n ImageAnnotation

# Create Domain layer project
dotnet new classlib -n ImageAnnotation.Domain -o src/ImageAnnotation.Domain -f net8.0
dotnet sln add src/ImageAnnotation.Domain/ImageAnnotation.Domain.csproj

# Create Application layer project
dotnet new classlib -n ImageAnnotation.Application -o src/ImageAnnotation.Application -f net8.0
dotnet sln add src/ImageAnnotation.Application/ImageAnnotation.Application.csproj

# Create Infrastructure layer project
dotnet new classlib -n ImageAnnotation.Infrastructure -o src/ImageAnnotation.Infrastructure -f net8.0
dotnet sln add src/ImageAnnotation.Infrastructure/ImageAnnotation.Infrastructure.csproj

# Create WPF app project
dotnet new wpf -n ImageAnnotation.App.Wpf -o src/ImageAnnotation.App.Wpf -f net8.0
dotnet sln add src/ImageAnnotation.App.Wpf/ImageAnnotation.App.Wpf.csproj

# Create test projects
dotnet new xunit -n ImageAnnotation.Domain.Tests -o tests/ImageAnnotation.Domain.Tests -f net8.0
dotnet sln add tests/ImageAnnotation.Domain.Tests/ImageAnnotation.Domain.Tests.csproj

dotnet new xunit -n ImageAnnotation.Integration.Tests -o tests/ImageAnnotation.Integration.Tests -f net8.0
dotnet sln add tests/ImageAnnotation.Integration.Tests/ImageAnnotation.Integration.Tests.csproj

echo "Solution structure created successfully!"
