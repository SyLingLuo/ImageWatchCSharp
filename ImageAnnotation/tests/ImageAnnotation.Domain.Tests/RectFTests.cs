using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Domain.Tests;

public class RectFTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange & Act
        var rect = new RectF(10, 20, 30, 40);

        // Assert
        Assert.Equal(10, rect.X);
        Assert.Equal(20, rect.Y);
        Assert.Equal(30, rect.Width);
        Assert.Equal(40, rect.Height);
    }

    [Fact]
    public void Constructor_ThrowsForNegativeWidth()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RectF(0, 0, -10, 10));
    }

    [Fact]
    public void Constructor_ThrowsForNegativeHeight()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RectF(0, 0, 10, -10));
    }

    [Fact]
    public void Center_CalculatesCorrectly()
    {
        // Arrange
        var rect = new RectF(10, 10, 20, 20);

        // Act
        var center = rect.Center;

        // Assert
        Assert.Equal(20, center.X);
        Assert.Equal(20, center.Y);
    }

    [Fact]
    public void Area_CalculatesCorrectly()
    {
        // Arrange
        var rect = new RectF(0, 0, 5, 4);

        // Act
        var area = rect.Area;

        // Assert
        Assert.Equal(20, area);
    }

    [Fact]
    public void Contains_ReturnsTrueForPointInside()
    {
        // Arrange
        var rect = new RectF(10, 10, 20, 20);
        var point = new Point2D(15, 15);

        // Act & Assert
        Assert.True(rect.Contains(point));
    }

    [Fact]
    public void Contains_ReturnsFalseForPointOutside()
    {
        // Arrange
        var rect = new RectF(10, 10, 20, 20);
        var point = new Point2D(5, 5);

        // Act & Assert
        Assert.False(rect.Contains(point));
    }

    [Fact]
    public void FromPoints_CreatesRectangle()
    {
        // Arrange
        var p1 = new Point2D(10, 10);
        var p2 = new Point2D(30, 40);

        // Act
        var rect = RectF.FromPoints(p1, p2);

        // Assert
        Assert.Equal(10, rect.X);
        Assert.Equal(10, rect.Y);
        Assert.Equal(20, rect.Width);
        Assert.Equal(30, rect.Height);
    }

    [Fact]
    public void IntersectsWith_ReturnsTrueForIntersectingRectangles()
    {
        // Arrange
        var rect1 = new RectF(10, 10, 20, 20);
        var rect2 = new RectF(20, 20, 20, 20);

        // Act & Assert
        Assert.True(rect1.IntersectsWith(rect2));
    }

    [Fact]
    public void IntersectsWith_ReturnsFalseForNonIntersectingRectangles()
    {
        // Arrange
        var rect1 = new RectF(10, 10, 10, 10);
        var rect2 = new RectF(30, 30, 10, 10);

        // Act & Assert
        Assert.False(rect1.IntersectsWith(rect2));
    }
}
