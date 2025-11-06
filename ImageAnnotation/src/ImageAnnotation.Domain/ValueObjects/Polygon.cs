namespace ImageAnnotation.Domain.ValueObjects;

/// <summary>
/// Represents a polygon defined by a list of points.
/// </summary>
public class Polygon : IEquatable<Polygon>
{
    /// <summary>
    /// Gets the points defining the polygon.
    /// </summary>
    public IReadOnlyList<Point2D> Points { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Polygon"/> class.
    /// </summary>
    /// <param name="points">The points defining the polygon.</param>
    public Polygon(IEnumerable<Point2D> points)
    {
        var pointList = points.ToList();
        if (pointList.Count < 3)
            throw new ArgumentException("Polygon must have at least 3 points", nameof(points));

        Points = pointList.AsReadOnly();
    }

    /// <summary>
    /// Gets the bounding rectangle of the polygon.
    /// </summary>
    public RectF GetBoundingRect()
    {
        var minX = Points.Min(p => p.X);
        var minY = Points.Min(p => p.Y);
        var maxX = Points.Max(p => p.X);
        var maxY = Points.Max(p => p.Y);

        return new RectF(minX, minY, maxX - minX, maxY - minY);
    }

    /// <summary>
    /// Calculates the area of the polygon using the shoelace formula.
    /// </summary>
    public double CalculateArea()
    {
        double area = 0;
        int n = Points.Count;

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            area += Points[i].X * Points[j].Y;
            area -= Points[j].X * Points[i].Y;
        }

        return Math.Abs(area) / 2.0;
    }

    /// <summary>
    /// Determines if the polygon contains the specified point.
    /// </summary>
    public bool Contains(Point2D point)
    {
        int n = Points.Count;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            if (((Points[i].Y > point.Y) != (Points[j].Y > point.Y)) &&
                (point.X < (Points[j].X - Points[i].X) * (point.Y - Points[i].Y) / (Points[j].Y - Points[i].Y) + Points[i].X))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    public bool Equals(Polygon? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Points.Count != other.Points.Count) return false;

        for (int i = 0; i < Points.Count; i++)
        {
            if (Points[i] != other.Points[i]) return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
        => Equals(obj as Polygon);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var point in Points)
            hash.Add(point);
        return hash.ToHashCode();
    }
}
