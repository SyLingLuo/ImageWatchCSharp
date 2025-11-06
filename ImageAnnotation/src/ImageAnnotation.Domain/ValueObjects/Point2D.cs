namespace ImageAnnotation.Domain.ValueObjects;

/// <summary>
/// Represents a 2D point with double precision coordinates.
/// </summary>
public readonly struct Point2D : IEquatable<Point2D>
{
    /// <summary>
    /// Gets the X coordinate.
    /// </summary>
    public double X { get; init; }

    /// <summary>
    /// Gets the Y coordinate.
    /// </summary>
    public double Y { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Point2D"/> struct.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
    }

    /// <summary>
    /// Gets the distance to another point.
    /// </summary>
    /// <param name="other">The other point.</param>
    /// <returns>The Euclidean distance.</returns>
    public double DistanceTo(Point2D other)
    {
        var dx = X - other.X;
        var dy = Y - other.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Adds two points.
    /// </summary>
    public static Point2D operator +(Point2D a, Point2D b)
        => new(a.X + b.X, a.Y + b.Y);

    /// <summary>
    /// Subtracts two points.
    /// </summary>
    public static Point2D operator -(Point2D a, Point2D b)
        => new(a.X - b.X, a.Y - b.Y);

    /// <summary>
    /// Multiplies a point by a scalar.
    /// </summary>
    public static Point2D operator *(Point2D p, double scalar)
        => new(p.X * scalar, p.Y * scalar);

    /// <summary>
    /// Divides a point by a scalar.
    /// </summary>
    public static Point2D operator /(Point2D p, double scalar)
        => new(p.X / scalar, p.Y / scalar);

    public bool Equals(Point2D other)
        => Math.Abs(X - other.X) < double.Epsilon && Math.Abs(Y - other.Y) < double.Epsilon;

    public override bool Equals(object? obj)
        => obj is Point2D other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(X, Y);

    public static bool operator ==(Point2D left, Point2D right)
        => left.Equals(right);

    public static bool operator !=(Point2D left, Point2D right)
        => !left.Equals(right);

    public override string ToString()
        => $"({X:F2}, {Y:F2})";
}
