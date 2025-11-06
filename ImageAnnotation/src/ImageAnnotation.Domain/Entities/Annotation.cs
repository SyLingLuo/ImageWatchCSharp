using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Domain.Entities;

/// <summary>
/// Base class for all annotation types.
/// </summary>
public abstract class Annotation
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the image asset ID this annotation belongs to.
    /// </summary>
    public Guid ImageAssetId { get; set; }

    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether this annotation is selected.
    /// </summary>
    public bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets whether this annotation is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets or sets additional attributes.
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; } = new();

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modified timestamp.
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Gets the bounding rectangle of this annotation.
    /// </summary>
    public abstract RectF GetBoundingRect();

    /// <summary>
    /// Determines if the annotation contains the specified point.
    /// </summary>
    public abstract bool Contains(Point2D point);

    /// <summary>
    /// Creates a deep copy of the annotation.
    /// </summary>
    public abstract Annotation Clone();
}
