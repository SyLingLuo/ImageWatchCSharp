#!/bin/bash
set -e

# Add project references
dotnet add src/ImageAnnotation.Application/ImageAnnotation.Application.csproj reference src/ImageAnnotation.Domain/ImageAnnotation.Domain.csproj
dotnet add src/ImageAnnotation.Infrastructure/ImageAnnotation.Infrastructure.csproj reference src/ImageAnnotation.Domain/ImageAnnotation.Domain.csproj
dotnet add src/ImageAnnotation.Infrastructure/ImageAnnotation.Infrastructure.csproj reference src/ImageAnnotation.Application/ImageAnnotation.Application.csproj

# Add test project references
dotnet add tests/ImageAnnotation.Domain.Tests/ImageAnnotation.Domain.Tests.csproj reference src/ImageAnnotation.Domain/ImageAnnotation.Domain.csproj
dotnet add tests/ImageAnnotation.Integration.Tests/ImageAnnotation.Integration.Tests.csproj reference src/ImageAnnotation.Domain/ImageAnnotation.Domain.csproj
dotnet add tests/ImageAnnotation.Integration.Tests/ImageAnnotation.Integration.Tests.csproj reference src/ImageAnnotation.Application/ImageAnnotation.Application.csproj
dotnet add tests/ImageAnnotation.Integration.Tests/ImageAnnotation.Integration.Tests.csproj reference src/ImageAnnotation.Infrastructure/ImageAnnotation.Infrastructure.csproj

# Add NuGet packages to Application layer
dotnet add src/ImageAnnotation.Application/ImageAnnotation.Application.csproj package CommunityToolkit.Mvvm --version 8.2.2
dotnet add src/ImageAnnotation.Application/ImageAnnotation.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.0

# Add NuGet packages to Infrastructure layer
dotnet add src/ImageAnnotation.Infrastructure/ImageAnnotation.Infrastructure.csproj package System.Text.Json --version 8.0.0
dotnet add src/ImageAnnotation.Infrastructure/ImageAnnotation.Infrastructure.csproj package Serilog --version 3.1.1
dotnet add src/ImageAnnotation.Infrastructure/ImageAnnotation.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection --version 8.0.0

# Add NuGet packages to test projects
dotnet add tests/ImageAnnotation.Domain.Tests/ImageAnnotation.Domain.Tests.csproj package NSubstitute --version 5.1.0
dotnet add tests/ImageAnnotation.Integration.Tests/ImageAnnotation.Integration.Tests.csproj package NSubstitute --version 5.1.0

echo "All project references and packages added successfully!"
