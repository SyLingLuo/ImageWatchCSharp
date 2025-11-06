using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Domain.Entities;

/// <summary>
/// Represents a bounding box annotation.
/// </summary>
public class BBoxAnnotation : Annotation
{
    /// <summary>
    /// Gets or sets the bounding rectangle.
    /// </summary>
    public RectF BoundingBox { get; set; }

    /// <summary>
    /// Gets or sets the rotation angle (optional).
    /// </summary>
    public double Angle { get; set; }

    public override RectF GetBoundingRect() => BoundingBox;

    public override bool Contains(Point2D point)
    {
        if (Math.Abs(Angle) < double.Epsilon)
        {
            return BoundingBox.Contains(point);
        }

        // For rotated boxes, create a RotatedRect and check
        var rotated = new RotatedRect(BoundingBox.Center, BoundingBox.Width, BoundingBox.Height, Angle);
        var corners = rotated.GetCorners();
        var polygon = new Polygon(corners);
        return polygon.Contains(point);
    }

    public override Annotation Clone()
    {
        return new BBoxAnnotation
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
            BoundingBox = BoundingBox,
            Angle = Angle
        };
    }
}
