using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Domain.Tests;

public class PolygonTests
{
    [Fact]
    public void Constructor_ThrowsForLessThanThreePoints()
    {
        // Arrange
        var points = new[] { new Point2D(0, 0), new Point2D(1, 1) };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Polygon(points));
    }

    [Fact]
    public void Constructor_AcceptsThreeOrMorePoints()
    {
        // Arrange
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(1, 0),
            new Point2D(0, 1)
        };

        // Act
        var polygon = new Polygon(points);

        // Assert
        Assert.Equal(3, polygon.Points.Count);
    }

    [Fact]
    public void CalculateArea_ReturnsCorrectArea()
    {
        // Arrange - right triangle with base 1 and height 1
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(1, 0),
            new Point2D(0, 1)
        };
        var polygon = new Polygon(points);

        // Act
        var area = polygon.CalculateArea();

        // Assert
        Assert.Equal(0.5, area, precision: 5);
    }

    [Fact]
    public void GetBoundingRect_ReturnsCorrectBounds()
    {
        // Arrange
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10, 10),
            new Point2D(0, 10)
        };
        var polygon = new Polygon(points);

        // Act
        var bounds = polygon.GetBoundingRect();

        // Assert
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(10, bounds.Width);
        Assert.Equal(10, bounds.Height);
    }

    [Fact]
    public void Contains_ReturnsTrueForPointInside()
    {
        // Arrange - square
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10, 10),
            new Point2D(0, 10)
        };
        var polygon = new Polygon(points);
        var point = new Point2D(5, 5);

        // Act & Assert
        Assert.True(polygon.Contains(point));
    }

    [Fact]
    public void Contains_ReturnsFalseForPointOutside()
    {
        // Arrange
        var points = new[]
        {
            new Point2D(0, 0),
            new Point2D(10, 0),
            new Point2D(10, 10),
            new Point2D(0, 10)
        };
        var polygon = new Polygon(points);
        var point = new Point2D(15, 15);

        // Act & Assert
        Assert.False(polygon.Contains(point));
    }
}
