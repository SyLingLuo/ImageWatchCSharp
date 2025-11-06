using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Domain.Entities;

/// <summary>
/// Represents a polygon annotation.
/// </summary>
public class PolygonAnnotation : Annotation
{
    /// <summary>
    /// Gets or sets the polygon.
    /// </summary>
    public Polygon Polygon { get; set; } = new(new[] { new Point2D(0, 0), new Point2D(1, 0), new Point2D(0, 1) });

    public override RectF GetBoundingRect() => Polygon.GetBoundingRect();

    public override bool Contains(Point2D point) => Polygon.Contains(point);

    public override Annotation Clone()
    {
        return new PolygonAnnotation
        {
            Id = Id,
            ImageAssetId = ImageAssetId,
            CategoryId = CategoryId,
            CategoryName = CategoryName,
            IsSelected = IsSelected,
            IsVisible = IsVisible,
            Attributes = new Dictionary<string, object>(Attributes),
            CreatedAt = CreatedAt,
            ModifiedAt = ModifiedAt,
            Polygon = new Polygon(Polygon.Points)
        };
    }
}
