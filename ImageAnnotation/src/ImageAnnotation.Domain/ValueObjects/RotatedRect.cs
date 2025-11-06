namespace ImageAnnotation.Domain.ValueObjects;

/// <summary>
/// Represents a rotated rectangle.
/// </summary>
public readonly struct RotatedRect : IEquatable<RotatedRect>
{
    /// <summary>
    /// Gets the center point of the rectangle.
    /// </summary>
    public Point2D Center { get; init; }

    /// <summary>
    /// Gets the width of the rectangle.
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// Gets the height of the rectangle.
    /// </summary>
    public double Height { get; init; }

    /// <summary>
    /// Gets the rotation angle in degrees.
    /// </summary>
    public double Angle { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RotatedRect"/> struct.
    /// </summary>
    public RotatedRect(Point2D center, double width, double height, double angle)
    {
        if (width < 0) throw new ArgumentException("Width must be non-negative", nameof(width));
        if (height < 0) throw new ArgumentException("Height must be non-negative", nameof(height));

        Center = center;
        Width = width;
        Height = height;
        Angle = angle;
    }

    /// <summary>
    /// Gets the four corner points of the rotated rectangle.
    /// </summary>
    public Point2D[] GetCorners()
    {
        var angleRad = Angle * Math.PI / 180.0;
        var cos = Math.Cos(angleRad);
        var sin = Math.Sin(angleRad);

        var hw = Width / 2;
        var hh = Height / 2;

        var corners = new Point2D[4];

        // Top-left
        corners[0] = new Point2D(
            Center.X - hw * cos + hh * sin,
            Center.Y - hw * sin - hh * cos
        );

        // Top-right
        corners[1] = new Point2D(
            Center.X + hw * cos + hh * sin,
            Center.Y + hw * sin - hh * cos
        );

        // Bottom-right
        corners[2] = new Point2D(
            Center.X + hw * cos - hh * sin,
            Center.Y + hw * sin + hh * cos
        );

        // Bottom-left
        corners[3] = new Point2D(
            Center.X - hw * cos - hh * sin,
            Center.Y - hw * sin + hh * cos
        );

        return corners;
    }

    /// <summary>
    /// Gets the bounding rectangle (axis-aligned).
    /// </summary>
    public RectF GetBoundingRect()
    {
        var corners = GetCorners();
        var minX = corners.Min(p => p.X);
        var minY = corners.Min(p => p.Y);
        var maxX = corners.Max(p => p.X);
        var maxY = corners.Max(p => p.Y);

        return new RectF(minX, minY, maxX - minX, maxY - minY);
    }

    public bool Equals(RotatedRect other)
        => Center.Equals(other.Center) &&
           Math.Abs(Width - other.Width) < double.Epsilon &&
           Math.Abs(Height - other.Height) < double.Epsilon &&
           Math.Abs(Angle - other.Angle) < double.Epsilon;

    public override bool Equals(object? obj)
        => obj is RotatedRect other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(Center, Width, Height, Angle);

    public static bool operator ==(RotatedRect left, RotatedRect right)
        => left.Equals(right);

    public static bool operator !=(RotatedRect left, RotatedRect right)
        => !left.Equals(right);

    public override string ToString()
        => $"Center: {Center}, Size: ({Width:F2} x {Height:F2}), Angle: {Angle:F2}°";
}
