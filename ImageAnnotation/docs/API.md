# API Documentation

## Domain API

### Value Objects

#### Point2D
```csharp
public readonly struct Point2D
{
    public double X { get; init; }
    public double Y { get; init; }
    
    public Point2D(double x, double y);
    public double DistanceTo(Point2D other);
    
    // Operators
    public static Point2D operator +(Point2D a, Point2D b);
    public static Point2D operator -(Point2D a, Point2D b);
    public static Point2D operator *(Point2D p, double scalar);
    public static Point2D operator /(Point2D p, double scalar);
}
```

#### RectF
```csharp
public readonly struct RectF
{
    public double X { get; init; }
    public double Y { get; init; }
    public double Width { get; init; }
    public double Height { get; init; }
    
    public RectF(double x, double y, double width, double height);
    
    public Point2D Center { get; }
    public Point2D TopLeft { get; }
    public Point2D BottomRight { get; }
    public double Area { get; }
    
    public bool Contains(Point2D point);
    public bool IntersectsWith(RectF other);
    public static RectF FromPoints(Point2D p1, Point2D p2);
}
```

#### Polygon
```csharp
public class Polygon
{
    public IReadOnlyList<Point2D> Points { get; }
    
    public Polygon(IEnumerable<Point2D> points);
    
    public RectF GetBoundingRect();
    public double CalculateArea();
    public bool Contains(Point2D point);
}
```

### Entities

#### Annotation (Base Class)
```csharp
public abstract class Annotation
{
    public Guid Id { get; set; }
    public Guid ImageAssetId { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public bool IsSelected { get; set; }
    public bool IsVisible { get; set; }
    public Dictionary<string, object> Attributes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    
    public abstract RectF GetBoundingRect();
    public abstract bool Contains(Point2D point);
    public abstract Annotation Clone();
}
```

#### BBoxAnnotation
```csharp
public class BBoxAnnotation : Annotation
{
    public RectF BoundingBox { get; set; }
    public double Angle { get; set; }
}
```

#### PolygonAnnotation
```csharp
public class PolygonAnnotation : Annotation
{
    public Polygon Polygon { get; set; }
}
```

#### KeypointAnnotation
```csharp
public class KeypointAnnotation : Annotation
{
    public List<Point2D> Keypoints { get; set; }
    public List<int> Visibility { get; set; }
    public List<(int, int)> Skeleton { get; set; }
}
```

#### Project
```csharp
public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string FilePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public Taxonomy Taxonomy { get; set; }
    public List<ImageAsset> Images { get; set; }
    public Dictionary<string, object> Settings { get; set; }
}
```

## Application API

### Services

#### ProjectService
```csharp
public class ProjectService
{
    public Project CreateProject(string name, string description = "");
    public ImageAsset AddImage(Project project, string filePath, int width, int height);
    public void RemoveImage(Project project, ImageAsset image);
}
```

#### CommandStackService
```csharp
public class CommandStackService
{
    public bool CanUndo { get; }
    public bool CanRedo { get; }
    
    public void ExecuteCommand(Command command);
    public void Undo();
    public void Redo();
    public void Clear();
}
```

#### SelectionService
```csharp
public class SelectionService
{
    public event EventHandler<Annotation?>? SelectionChanged;
    public Annotation? SelectedAnnotation { get; }
    
    public void Select(Annotation? annotation);
    public void ClearSelection();
}
```

### Commands

#### Command (Base)
```csharp
public abstract class Command
{
    public abstract string Description { get; }
    public abstract void Execute();
    public abstract void Undo();
}
```

#### CreateAnnotationCommand
```csharp
public class CreateAnnotationCommand : Command
{
    public CreateAnnotationCommand(ImageAsset imageAsset, Annotation annotation);
}
```

#### DeleteAnnotationCommand
```csharp
public class DeleteAnnotationCommand : Command
{
    public DeleteAnnotationCommand(ImageAsset imageAsset, Annotation annotation);
}
```

### ViewModels

#### MainViewModel
```csharp
public partial class MainViewModel : ObservableObject
{
    public Project? CurrentProject { get; set; }
    public ImageAsset? CurrentImage { get; set; }
    public string StatusMessage { get; set; }
    public bool IsProjectLoaded { get; set; }
    
    public CanvasViewModel CanvasViewModel { get; }
    public ToolbarViewModel ToolbarViewModel { get; }
    public LayerViewModel LayerViewModel { get; }
    public PropertyViewModel PropertyViewModel { get; }
    
    public IRelayCommand NewProjectCommand { get; }
    public IAsyncRelayCommand OpenProjectCommand { get; }
    public IAsyncRelayCommand SaveProjectCommand { get; }
    public IAsyncRelayCommand AddImageCommand { get; }
}
```

#### CanvasViewModel
```csharp
public partial class CanvasViewModel : ObservableObject
{
    public ImageAsset? CurrentImage { get; set; }
    public double ZoomLevel { get; set; }
    public Point2D PanOffset { get; set; }
    public Annotation? SelectedAnnotation { get; set; }
    public ObservableCollection<Annotation> Annotations { get; }
    public string CurrentTool { get; set; }
    
    public void SetCurrentImage(ImageAsset? image);
    public void ResetView();
    public void ZoomIn();
    public void ZoomOut();
    public void AddAnnotation(Annotation annotation);
    public void RemoveAnnotation(Annotation annotation);
    public void SelectAnnotation(Annotation? annotation);
}
```

## Infrastructure API

### Storage

#### ProjectRepository
```csharp
public class ProjectRepository
{
    public Task SaveAsync(Project project, string filePath);
    public Task<Project?> LoadAsync(string filePath);
    public bool Exists(string filePath);
}
```

### Format Adapters

#### IImporter
```csharp
public interface IImporter
{
    string FormatName { get; }
    string FileExtension { get; }
    Task<ImportResult> ImportAsync(string filePath, Project project);
}
```

#### IExporter
```csharp
public interface IExporter
{
    string FormatName { get; }
    string FileExtension { get; }
    Task<ExportResult> ExportAsync(Project project, string filePath);
}
```

#### CocoImporter
```csharp
public class CocoImporter : IImporter
{
    public string FormatName => "COCO";
    public string FileExtension => ".json";
    public Task<ImportResult> ImportAsync(string filePath, Project project);
}
```

#### CocoExporter
```csharp
public class CocoExporter : IExporter
{
    public string FormatName => "COCO";
    public string FileExtension => ".json";
    public Task<ExportResult> ExportAsync(Project project, string filePath);
}
```

#### YoloExporter
```csharp
public class YoloExporter : IExporter
{
    public string FormatName => "YOLO";
    public string FileExtension => ".txt";
    public Task<ExportResult> ExportAsync(Project project, string outputDirectory);
}
```

### Tools

#### ITool
```csharp
public interface ITool
{
    string Name { get; }
    ToolState State { get; }
    
    void OnMouseDown(Point2D position, IToolContext context);
    void OnMouseMove(Point2D position, IToolContext context);
    void OnMouseUp(Point2D position, IToolContext context);
    void OnKeyPress(string key, IToolContext context);
    void Reset();
}
```

#### RectangleTool
```csharp
public class RectangleTool : ITool
{
    public string Name => "Rectangle";
    public ToolState State { get; }
    public BBoxAnnotation? GetCurrentAnnotation();
}
```

#### PolygonTool
```csharp
public class PolygonTool : ITool
{
    public string Name => "Polygon";
    public ToolState State { get; }
    public IReadOnlyList<Point2D> GetCurrentPoints();
}
```

## Usage Examples

### Creating a New Project
```csharp
var projectService = new ProjectService();
var project = projectService.CreateProject("My Project", "Description");
```

### Adding an Image
```csharp
var imageAsset = projectService.AddImage(project, "path/to/image.jpg", 1920, 1080);
```

### Creating an Annotation
```csharp
var annotation = new BBoxAnnotation
{
    Id = Guid.NewGuid(),
    ImageAssetId = imageAsset.Id,
    CategoryId = 1,
    CategoryName = "Person",
    BoundingBox = new RectF(100, 100, 200, 300),
    CreatedAt = DateTime.UtcNow,
    ModifiedAt = DateTime.UtcNow
};

imageAsset.Annotations.Add(annotation);
```

### Using Undo/Redo
```csharp
var commandStack = new CommandStackService();
var command = new CreateAnnotationCommand(imageAsset, annotation);

commandStack.ExecuteCommand(command);  // Add annotation
commandStack.Undo();                   // Remove annotation
commandStack.Redo();                   // Add annotation again
```

### Saving a Project
```csharp
var repository = new ProjectRepository();
await repository.SaveAsync(project, "project.json");
```

### Exporting to COCO
```csharp
var exporter = new CocoExporter();
var result = await exporter.ExportAsync(project, "annotations.json");

if (result.Success)
{
    Console.WriteLine($"Exported {result.ImagesExported} images, {result.AnnotationsExported} annotations");
}
```

### Exporting to YOLO
```csharp
var exporter = new YoloExporter();
var result = await exporter.ExportAsync(project, "output/directory");
```
