using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Domain.Entities;

/// <summary>
/// Represents a keypoint annotation.
/// </summary>
public class KeypointAnnotation : Annotation
{
    /// <summary>
    /// Gets or sets the keypoint locations.
    /// </summary>
    public List<Point2D> Keypoints { get; set; } = new();

    /// <summary>
    /// Gets or sets the visibility flags for each keypoint.
    /// 0 = not labeled, 1 = labeled but not visible, 2 = labeled and visible
    /// </summary>
    public List<int> Visibility { get; set; } = new();

    /// <summary>
    /// Gets or sets the skeleton connections (pairs of keypoint indices).
    /// </summary>
    public List<(int, int)> Skeleton { get; set; } = new();

    public override RectF GetBoundingRect()
    {
        if (Keypoints.Count == 0)
            return new RectF(0, 0, 0, 0);

        var minX = Keypoints.Min(p => p.X);
        var minY = Keypoints.Min(p => p.Y);
        var maxX = Keypoints.Max(p => p.X);
        var maxY = Keypoints.Max(p => p.Y);

        return new RectF(minX, minY, maxX - minX, maxY - minY);
    }

    public override bool Contains(Point2D point)
    {
        // Check if point is close to any keypoint (within 5 pixels)
        const double threshold = 5.0;
        return Keypoints.Any(kp => kp.DistanceTo(point) < threshold);
    }

    public override Annotation Clone()
    {
        return new KeypointAnnotation
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
            Keypoints = new List<Point2D>(Keypoints),
            Visibility = new List<int>(Visibility),
            Skeleton = new List<(int, int)>(Skeleton)
        };
    }
}
