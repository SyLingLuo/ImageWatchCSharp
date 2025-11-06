using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Domain.Entities;

/// <summary>
/// Represents a mask annotation (segmentation).
/// </summary>
public class MaskAnnotation : Annotation
{
    /// <summary>
    /// Gets or sets the RLE (Run-Length Encoding) data.
    /// </summary>
    public byte[] RleData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Gets or sets the mask width.
    /// </summary>
    public int MaskWidth { get; set; }

    /// <summary>
    /// Gets or sets the mask height.
    /// </summary>
    public int MaskHeight { get; set; }

    /// <summary>
    /// Gets or sets the bounding box (cached for performance).
    /// </summary>
    public RectF CachedBoundingBox { get; set; }

    public override RectF GetBoundingRect() => CachedBoundingBox;

    public override bool Contains(Point2D point)
    {
        // Simplified: check if point is within bounding box
        // A full implementation would decode RLE and check the actual mask
        return CachedBoundingBox.Contains(point);
    }

    public override Annotation Clone()
    {
        return new MaskAnnotation
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
            RleData = (byte[])RleData.Clone(),
            MaskWidth = MaskWidth,
            MaskHeight = MaskHeight,
            CachedBoundingBox = CachedBoundingBox
        };
    }
}
