using ImageAnnotation.Domain.Entities;
using ImageAnnotation.Domain.Interfaces;
using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Infrastructure.Tools;

/// <summary>
/// Rectangle annotation tool.
/// </summary>
public class RectangleTool : ITool
{
    private Point2D? _startPoint;
    private BBoxAnnotation? _currentAnnotation;

    public string Name => "Rectangle";
    public ToolState State { get; private set; } = ToolState.Idle;

    public void OnMouseDown(Point2D position, IToolContext context)
    {
        _startPoint = position;
        State = ToolState.Drawing;

        _currentAnnotation = new BBoxAnnotation
        {
            Id = Guid.NewGuid(),
            ImageAssetId = context.CurrentImageAssetId,
            CategoryId = context.CurrentCategoryId,
            BoundingBox = new RectF(position.X, position.Y, 0, 0),
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        context.RequestRedraw();
    }

    public void OnMouseMove(Point2D position, IToolContext context)
    {
        if (State == ToolState.Drawing && _startPoint.HasValue && _currentAnnotation != null)
        {
            _currentAnnotation.BoundingBox = RectF.FromPoints(_startPoint.Value, position);
            context.RequestRedraw();
        }
    }

    public void OnMouseUp(Point2D position, IToolContext context)
    {
        if (State == ToolState.Drawing && _startPoint.HasValue && _currentAnnotation != null)
        {
            _currentAnnotation.BoundingBox = RectF.FromPoints(_startPoint.Value, position);

            // Only add if rectangle has meaningful size
            if (_currentAnnotation.BoundingBox.Width > 5 && _currentAnnotation.BoundingBox.Height > 5)
            {
                context.AddAnnotation(_currentAnnotation);
            }

            Reset();
            context.RequestRedraw();
        }
    }

    public void OnKeyPress(string key, IToolContext context)
    {
        if (key == "Escape")
        {
            Reset();
            context.RequestRedraw();
        }
    }

    public void Reset()
    {
        _startPoint = null;
        _currentAnnotation = null;
        State = ToolState.Idle;
    }

    /// <summary>
    /// Gets the current annotation being drawn (for preview rendering).
    /// </summary>
    public BBoxAnnotation? GetCurrentAnnotation() => _currentAnnotation;
}
