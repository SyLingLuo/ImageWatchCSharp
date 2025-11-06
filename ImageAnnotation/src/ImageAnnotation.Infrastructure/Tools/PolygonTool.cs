using ImageAnnotation.Domain.Entities;
using ImageAnnotation.Domain.Interfaces;
using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Infrastructure.Tools;

/// <summary>
/// Polygon annotation tool.
/// </summary>
public class PolygonTool : ITool
{
    private readonly List<Point2D> _points = new();
    private PolygonAnnotation? _currentAnnotation;

    public string Name => "Polygon";
    public ToolState State { get; private set; } = ToolState.Idle;

    public void OnMouseDown(Point2D position, IToolContext context)
    {
        if (State == ToolState.Idle)
        {
            State = ToolState.Drawing;
            _points.Clear();
        }

        _points.Add(position);
        context.RequestRedraw();
    }

    public void OnMouseMove(Point2D position, IToolContext context)
    {
        // Preview line to cursor if drawing
        if (State == ToolState.Drawing)
        {
            context.RequestRedraw();
        }
    }

    public void OnMouseUp(Point2D position, IToolContext context)
    {
        // Polygon is completed on double-click or Enter key
    }

    public void OnKeyPress(string key, IToolContext context)
    {
        if (key == "Enter" && _points.Count >= 3)
        {
            // Complete the polygon
            _currentAnnotation = new PolygonAnnotation
            {
                Id = Guid.NewGuid(),
                ImageAssetId = context.CurrentImageAssetId,
                CategoryId = context.CurrentCategoryId,
                Polygon = new Polygon(_points),
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };

            context.AddAnnotation(_currentAnnotation);
            Reset();
            context.RequestRedraw();
        }
        else if (key == "Escape")
        {
            Reset();
            context.RequestRedraw();
        }
    }

    public void Reset()
    {
        _points.Clear();
        _currentAnnotation = null;
        State = ToolState.Idle;
    }

    /// <summary>
    /// Gets the current points being drawn.
    /// </summary>
    public IReadOnlyList<Point2D> GetCurrentPoints() => _points.AsReadOnly();
}
