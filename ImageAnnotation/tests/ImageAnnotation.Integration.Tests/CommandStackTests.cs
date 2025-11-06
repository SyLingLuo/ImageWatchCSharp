using ImageAnnotation.Application.Commands;
using ImageAnnotation.Application.Services;
using ImageAnnotation.Domain.Entities;
using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Integration.Tests;

public class CommandStackTests
{
    [Fact]
    public void ExecuteCommand_AddsToUndoStack()
    {
        // Arrange
        var commandStack = new CommandStackService();
        var imageAsset = new ImageAsset { Id = Guid.NewGuid() };
        var annotation = new BBoxAnnotation
        {
            Id = Guid.NewGuid(),
            BoundingBox = new RectF(0, 0, 10, 10)
        };
        var command = new CreateAnnotationCommand(imageAsset, annotation);

        // Act
        commandStack.ExecuteCommand(command);

        // Assert
        Assert.True(commandStack.CanUndo);
        Assert.False(commandStack.CanRedo);
        Assert.Single(imageAsset.Annotations);
    }

    [Fact]
    public void Undo_ReversesCommand()
    {
        // Arrange
        var commandStack = new CommandStackService();
        var imageAsset = new ImageAsset { Id = Guid.NewGuid() };
        var annotation = new BBoxAnnotation
        {
            Id = Guid.NewGuid(),
            BoundingBox = new RectF(0, 0, 10, 10)
        };
        var command = new CreateAnnotationCommand(imageAsset, annotation);
        commandStack.ExecuteCommand(command);

        // Act
        commandStack.Undo();

        // Assert
        Assert.False(commandStack.CanUndo);
        Assert.True(commandStack.CanRedo);
        Assert.Empty(imageAsset.Annotations);
    }

    [Fact]
    public void Redo_ReappliesCommand()
    {
        // Arrange
        var commandStack = new CommandStackService();
        var imageAsset = new ImageAsset { Id = Guid.NewGuid() };
        var annotation = new BBoxAnnotation
        {
            Id = Guid.NewGuid(),
            BoundingBox = new RectF(0, 0, 10, 10)
        };
        var command = new CreateAnnotationCommand(imageAsset, annotation);
        commandStack.ExecuteCommand(command);
        commandStack.Undo();

        // Act
        commandStack.Redo();

        // Assert
        Assert.True(commandStack.CanUndo);
        Assert.False(commandStack.CanRedo);
        Assert.Single(imageAsset.Annotations);
    }

    [Fact]
    public void ExecuteCommand_ClearsRedoStack()
    {
        // Arrange
        var commandStack = new CommandStackService();
        var imageAsset = new ImageAsset { Id = Guid.NewGuid() };
        var annotation1 = new BBoxAnnotation { Id = Guid.NewGuid(), BoundingBox = new RectF(0, 0, 10, 10) };
        var annotation2 = new BBoxAnnotation { Id = Guid.NewGuid(), BoundingBox = new RectF(5, 5, 10, 10) };
        
        commandStack.ExecuteCommand(new CreateAnnotationCommand(imageAsset, annotation1));
        commandStack.Undo();

        // Act - execute new command after undo
        commandStack.ExecuteCommand(new CreateAnnotationCommand(imageAsset, annotation2));

        // Assert
        Assert.True(commandStack.CanUndo);
        Assert.False(commandStack.CanRedo);
    }
}
