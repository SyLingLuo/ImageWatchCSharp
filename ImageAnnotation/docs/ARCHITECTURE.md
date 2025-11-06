# Architecture Documentation

## Overview

The Image Annotation System is a production-grade WPF application built with .NET 8.0, following Clean Architecture principles and Domain-Driven Design (DDD) patterns.

## Architecture Layers

### 1. Domain Layer (`ImageAnnotation.Domain`)

The core business logic layer, completely independent of any infrastructure concerns.

**Value Objects:**
- `Point2D`: Represents a 2D point with double precision
- `RectF`: Rectangle with floating-point coordinates
- `Polygon`: Collection of points forming a polygon
- `RotatedRect`: Rectangle with rotation support

**Entities:**
- `Project`: Root aggregate for annotation projects
- `ImageAsset`: Represents an image in the project
- `Annotation`: Base class for annotations
  - `BBoxAnnotation`: Bounding box annotations
  - `PolygonAnnotation`: Polygon-based annotations
  - `KeypointAnnotation`: Keypoint annotations with skeleton support
  - `MaskAnnotation`: Segmentation mask annotations
- `Taxonomy`: Category system
- `Category`: Individual category definition

**Interfaces:**
- `IAnnotationService`: Service for annotation operations
- `ITool`: Interface for annotation tools
- `IValidationService`: Validation operations

### 2. Application Layer (`ImageAnnotation.Application`)

Contains application logic, view models, and commands.

**ViewModels:**
- `MainViewModel`: Main application view model
- `CanvasViewModel`: Canvas display and interaction
- `ToolbarViewModel`: Tool selection and commands
- `LayerViewModel`: Layer/annotation list management
- `PropertyViewModel`: Property panel display

**Commands:**
- `Command`: Base class for undoable commands
- `CreateAnnotationCommand`: Create new annotations
- `DeleteAnnotationCommand`: Delete annotations

**Services:**
- `ProjectService`: Project management
- `CommandStackService`: Undo/Redo functionality
- `SelectionService`: Annotation selection management
- `EventAggregatorService`: Event-based communication

### 3. Infrastructure Layer (`ImageAnnotation.Infrastructure`)

Technical infrastructure and external integrations.

**Storage:**
- `ProjectRepository`: Project persistence using JSON

**Format Adapters:**
- `CocoImporter/CocoExporter`: COCO format support
- `YoloExporter`: YOLO format export
- Import/Export interfaces for extensibility

**Tools:**
- `RectangleTool`: Rectangle drawing tool
- `PolygonTool`: Polygon drawing tool

**Logging:**
- `LoggingConfiguration`: Serilog setup

### 4. WPF Presentation Layer (`ImageAnnotation.App.Wpf`)

User interface implementation.

**Views:**
- `MainWindow`: Main application window
- Layout with 3-panel design (Layers, Canvas, Properties)

**Converters:**
- `BoolToVisibilityConverter`: Boolean to Visibility conversion

**Styles:**
- `DefaultStyles.xaml`: Application-wide styling

## Design Patterns

### Dependency Injection
- Microsoft.Extensions.DependencyInjection
- All services registered in `App.xaml.cs`
- Constructor injection throughout

### MVVM (Model-View-ViewModel)
- CommunityToolkit.Mvvm for base implementations
- Observable properties and commands
- Data binding between views and view models

### Command Pattern
- Undoable commands via `Command` base class
- Command stack for undo/redo operations

### Repository Pattern
- Abstraction over data access
- `ProjectRepository` for project persistence

### Strategy Pattern
- `ITool` interface for different annotation tools
- Pluggable tool implementations

## Data Flow

1. **User Interaction** → UI (WPF Views)
2. **UI Events** → ViewModels (MVVM Binding)
3. **ViewModel Commands** → Application Services
4. **Services** → Domain Entities
5. **Persistence** → Infrastructure Repositories
6. **Format Export** → Infrastructure Adapters

## Key Features

### Undo/Redo System
- Full command stack implementation
- All modifications are undoable
- Command pattern with Execute/Undo methods

### Format Support
- **COCO**: Import and export with full metadata
- **YOLO**: Export with normalized coordinates
- Extensible via `IImporter`/`IExporter` interfaces

### Tool System
- State machine-based tools
- Mouse event handling
- Keyboard shortcuts
- Preview rendering during drawing

### Logging
- Serilog integration
- Structured logging
- Configurable output (console, file)

## Technology Stack

- **.NET 8.0**: Target framework
- **WPF**: Desktop UI framework
- **CommunityToolkit.Mvvm**: MVVM helpers
- **Serilog**: Logging framework
- **xUnit**: Testing framework
- **NSubstitute**: Mocking library
- **System.Text.Json**: Serialization

## Testing Strategy

### Unit Tests
- Domain value objects and entities
- Command stack undo/redo
- Geometry calculations
- Format conversions

### Integration Tests
- Project save/load
- Import/export workflows
- End-to-end scenarios

## Extension Points

1. **New Annotation Types**: Inherit from `Annotation` base class
2. **New Tools**: Implement `ITool` interface
3. **New Formats**: Implement `IImporter`/`IExporter`
4. **Custom Validation**: Implement `IValidationService`
5. **Plugin System**: Extensibility via DI container

## Performance Considerations

- **Lazy Loading**: Images loaded on demand
- **Caching**: TileCache for large images (planned)
- **Async Operations**: Import/export run asynchronously
- **Observable Collections**: Efficient UI updates
- **Dirty Rectangle**: Optimized rendering (planned)

## Security

- Input validation on all user data
- Path sanitization for file operations
- Exception handling throughout
- No direct SQL/database access (file-based)
