using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Domain.Tests;

public class Point2DTests
{
    [Fact]
    public void Constructor_SetsCoordinates()
    {
        // Arrange & Act
        var point = new Point2D(10.5, 20.3);

        // Assert
        Assert.Equal(10.5, point.X);
        Assert.Equal(20.3, point.Y);
    }

    [Fact]
    public void DistanceTo_CalculatesCorrectDistance()
    {
        // Arrange
        var point1 = new Point2D(0, 0);
        var point2 = new Point2D(3, 4);

        // Act
        var distance = point1.DistanceTo(point2);

        // Assert
        Assert.Equal(5.0, distance, precision: 5);
    }

    [Fact]
    public void Addition_AddsCoordinates()
    {
        // Arrange
        var point1 = new Point2D(1, 2);
        var point2 = new Point2D(3, 4);

        // Act
        var result = point1 + point2;

        // Assert
        Assert.Equal(4, result.X);
        Assert.Equal(6, result.Y);
    }

    [Fact]
    public void Subtraction_SubtractsCoordinates()
    {
        // Arrange
        var point1 = new Point2D(5, 7);
        var point2 = new Point2D(2, 3);

        // Act
        var result = point1 - point2;

        // Assert
        Assert.Equal(3, result.X);
        Assert.Equal(4, result.Y);
    }

    [Fact]
    public void Equals_ReturnsTrueForEqualPoints()
    {
        // Arrange
        var point1 = new Point2D(1.5, 2.5);
        var point2 = new Point2D(1.5, 2.5);

        // Act & Assert
        Assert.True(point1.Equals(point2));
        Assert.True(point1 == point2);
    }
}
