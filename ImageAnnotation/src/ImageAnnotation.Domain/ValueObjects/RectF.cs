namespace ImageAnnotation.Domain.ValueObjects;

/// <summary>
/// Represents a rectangle with floating-point coordinates.
/// </summary>
public readonly struct RectF : IEquatable<RectF>
{
    /// <summary>
    /// Gets the X coordinate of the top-left corner.
    /// </summary>
    public double X { get; init; }

    /// <summary>
    /// Gets the Y coordinate of the top-left corner.
    /// </summary>
    public double Y { get; init; }

    /// <summary>
    /// Gets the width of the rectangle.
    /// </summary>
    public double Width { get; init; }

    /// <summary>
    /// Gets the height of the rectangle.
    /// </summary>
    public double Height { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RectF"/> struct.
    /// </summary>
    public RectF(double x, double y, double width, double height)
    {
        if (width < 0) throw new ArgumentException("Width must be non-negative", nameof(width));
        if (height < 0) throw new ArgumentException("Height must be non-negative", nameof(height));

        X = x;
        Y = y;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the center point of the rectangle.
    /// </summary>
    public Point2D Center => new(X + Width / 2, Y + Height / 2);

    /// <summary>
    /// Gets the top-left corner point.
    /// </summary>
    public Point2D TopLeft => new(X, Y);

    /// <summary>
    /// Gets the bottom-right corner point.
    /// </summary>
    public Point2D BottomRight => new(X + Width, Y + Height);

    /// <summary>
    /// Gets the area of the rectangle.
    /// </summary>
    public double Area => Width * Height;

    /// <summary>
    /// Determines if the rectangle contains the specified point.
    /// </summary>
    public bool Contains(Point2D point)
        => point.X >= X && point.X <= X + Width && point.Y >= Y && point.Y <= Y + Height;

    /// <summary>
    /// Determines if this rectangle intersects with another rectangle.
    /// </summary>
    public bool IntersectsWith(RectF other)
        => X < other.X + other.Width && X + Width > other.X &&
           Y < other.Y + other.Height && Y + Height > other.Y;

    /// <summary>
    /// Creates a rectangle from two corner points.
    /// </summary>
    public static RectF FromPoints(Point2D p1, Point2D p2)
    {
        var x = Math.Min(p1.X, p2.X);
        var y = Math.Min(p1.Y, p2.Y);
        var width = Math.Abs(p2.X - p1.X);
        var height = Math.Abs(p2.Y - p1.Y);
        return new RectF(x, y, width, height);
    }

    public bool Equals(RectF other)
        => Math.Abs(X - other.X) < double.Epsilon &&
           Math.Abs(Y - other.Y) < double.Epsilon &&
           Math.Abs(Width - other.Width) < double.Epsilon &&
           Math.Abs(Height - other.Height) < double.Epsilon;

    public override bool Equals(object? obj)
        => obj is RectF other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(X, Y, Width, Height);

    public static bool operator ==(RectF left, RectF right)
        => left.Equals(right);

    public static bool operator !=(RectF left, RectF right)
        => !left.Equals(right);

    public override string ToString()
        => $"[{X:F2}, {Y:F2}, {Width:F2}, {Height:F2}]";
}
