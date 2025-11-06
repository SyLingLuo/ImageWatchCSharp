# Image Annotation System

A production-grade WPF image annotation application built with .NET 8.0, following Clean Architecture principles.

## Features

✅ **Complete Architecture**
- Clean architecture with Domain, Application, Infrastructure, and Presentation layers
- Domain-Driven Design (DDD) principles
- MVVM pattern with CommunityToolkit.Mvvm
- Dependency Injection with Microsoft.Extensions.DependencyInjection

✅ **Annotation Types**
- Bounding Box (Rectangle) annotations
- Polygon annotations
- Keypoint annotations with skeleton support
- Segmentation mask annotations (RLE-based)

✅ **Format Support**
- COCO format import/export
- YOLO format export
- Extensible adapter system for custom formats

✅ **Interactive Tools**
- Rectangle drawing tool
- Polygon drawing tool
- Selection tool
- State machine-based tool system

✅ **Undo/Redo System**
- Complete command pattern implementation
- Full undo/redo support for all operations
- Command stack management

✅ **User Interface**
- Modern WPF interface
- Three-panel layout (Layers, Canvas, Properties)
- Toolbar with tool selection
- Status bar with messages
- Keyboard shortcuts

✅ **Testing**
- Comprehensive unit tests
- Integration tests
- Test coverage for core functionality

✅ **Logging**
- Serilog integration
- Structured logging
- Configurable output

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 (recommended) or Visual Studio Code
- Windows OS (for WPF support)

### Building the Project

```bash
# Clone the repository
git clone <repository-url>
cd ImageAnnotation

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run the application
dotnet run --project src/ImageAnnotation.App.Wpf/ImageAnnotation.App.Wpf.csproj
```

### Project Structure

```
ImageAnnotation/
├── src/
│   ├── ImageAnnotation.Domain/           # Domain layer (entities, value objects)
│   ├── ImageAnnotation.Application/      # Application layer (ViewModels, services)
│   ├── ImageAnnotation.Infrastructure/   # Infrastructure layer (storage, adapters)
│   └── ImageAnnotation.App.Wpf/         # WPF presentation layer
├── tests/
│   ├── ImageAnnotation.Domain.Tests/     # Unit tests
│   └── ImageAnnotation.Integration.Tests/ # Integration tests
├── docs/
│   ├── ARCHITECTURE.md                   # Architecture documentation
│   └── API.md                            # API documentation
└── ImageAnnotation.sln                   # Solution file
```

## Usage

### Creating a New Project

1. Launch the application
2. Click **File → New Project**
3. A new project is created with default categories

### Adding Images

1. Click **File → Add Image**
2. Select images to annotate
3. Images are added to the project

### Drawing Annotations

#### Rectangle Tool
1. Select **Rectangle** from the toolbar
2. Click and drag on the canvas to draw a rectangle
3. Release to complete the annotation

#### Polygon Tool
1. Select **Polygon** from the toolbar
2. Click to add points
3. Press **Enter** to complete the polygon
4. Press **Esc** to cancel

### Managing Annotations

- View all annotations in the **Layers** panel
- Select an annotation to view its properties
- Toggle visibility with the checkbox
- Delete selected annotations with **Delete** key

### Undo/Redo

- **Ctrl+Z**: Undo last action
- **Ctrl+Y**: Redo last undone action
- Available in Edit menu and toolbar

### Saving and Loading

#### Save Project
```
File → Save Project
```
Saves the project as a JSON file with all images and annotations.

#### Open Project
```
File → Open Project
```
Loads a previously saved project.

### Exporting Annotations

#### Export to COCO Format
```csharp
var exporter = new CocoExporter();
var result = await exporter.ExportAsync(project, "output.json");
```

#### Export to YOLO Format
```csharp
var exporter = new YoloExporter();
var result = await exporter.ExportAsync(project, "output_directory");
```

## Architecture

### Domain Layer
- Pure business logic
- No dependencies on other layers
- Value objects: Point2D, RectF, Polygon, RotatedRect
- Entities: Project, ImageAsset, Annotation types, Taxonomy

### Application Layer
- ViewModels for MVVM
- Application services
- Command pattern for undo/redo
- Event aggregation

### Infrastructure Layer
- Data persistence
- Format adapters (COCO, YOLO)
- Tool implementations
- Logging configuration

### Presentation Layer
- WPF views and controls
- XAML layouts
- Converters and behaviors
- Dependency injection setup

## Technologies

- **.NET 8.0**: Modern .NET platform
- **WPF**: Windows Presentation Foundation
- **CommunityToolkit.Mvvm**: MVVM helpers
- **Serilog**: Logging framework
- **System.Text.Json**: JSON serialization
- **xUnit**: Testing framework
- **NSubstitute**: Mocking library

## Testing

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
dotnet test tests/ImageAnnotation.Domain.Tests/
```

### Test Coverage
- Value object operations
- Entity behavior
- Command stack undo/redo
- Geometry calculations
- Format conversions

## Extending the Application

### Adding a New Annotation Type

1. Create a new class inheriting from `Annotation`:
```csharp
public class CustomAnnotation : Annotation
{
    // Custom properties
    public override RectF GetBoundingRect() { /* ... */ }
    public override bool Contains(Point2D point) { /* ... */ }
    public override Annotation Clone() { /* ... */ }
}
```

### Adding a New Tool

1. Implement the `ITool` interface:
```csharp
public class CustomTool : ITool
{
    public string Name => "Custom";
    public ToolState State { get; private set; }
    
    public void OnMouseDown(Point2D position, IToolContext context) { /* ... */ }
    public void OnMouseMove(Point2D position, IToolContext context) { /* ... */ }
    public void OnMouseUp(Point2D position, IToolContext context) { /* ... */ }
    public void OnKeyPress(string key, IToolContext context) { /* ... */ }
    public void Reset() { /* ... */ }
}
```

### Adding a New Format Adapter

1. Implement `IImporter` for import:
```csharp
public class CustomImporter : IImporter
{
    public string FormatName => "Custom";
    public string FileExtension => ".custom";
    
    public async Task<ImportResult> ImportAsync(string filePath, Project project)
    {
        // Implementation
    }
}
```

2. Implement `IExporter` for export:
```csharp
public class CustomExporter : IExporter
{
    public string FormatName => "Custom";
    public string FileExtension => ".custom";
    
    public async Task<ExportResult> ExportAsync(Project project, string filePath)
    {
        // Implementation
    }
}
```

## Documentation

- [Architecture Documentation](docs/ARCHITECTURE.md)
- [API Documentation](docs/API.md)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License.

## Support

For issues, questions, or contributions, please open an issue on GitHub.

## Acknowledgments

- Built with Clean Architecture principles
- Inspired by industry-standard annotation tools
- Uses modern .NET 8.0 features and best practices
