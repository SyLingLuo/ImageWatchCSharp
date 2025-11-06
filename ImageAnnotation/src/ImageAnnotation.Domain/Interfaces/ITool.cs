using ImageAnnotation.Domain.ValueObjects;

namespace ImageAnnotation.Domain.Interfaces;

/// <summary>
/// Interface for annotation tools.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets the tool name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the current tool state.
    /// </summary>
    ToolState State { get; }

    /// <summary>
    /// Handles mouse down event.
    /// </summary>
    void OnMouseDown(Point2D position, IToolContext context);

    /// <summary>
    /// Handles mouse move event.
    /// </summary>
    void OnMouseMove(Point2D position, IToolContext context);

    /// <summary>
    /// Handles mouse up event.
    /// </summary>
    void OnMouseUp(Point2D position, IToolContext context);

    /// <summary>
    /// Handles key press event.
    /// </summary>
    void OnKeyPress(string key, IToolContext context);

    /// <summary>
    /// Resets the tool to its initial state.
    /// </summary>
    void Reset();
}

/// <summary>
/// Tool state enumeration.
/// </summary>
public enum ToolState
{
    Idle,
    Drawing,
    Editing,
    Confirming
}

/// <summary>
/// Context interface for tool operations.
/// </summary>
public interface IToolContext
{
    /// <summary>
    /// Gets the current image asset ID.
    /// </summary>
    Guid CurrentImageAssetId { get; }

    /// <summary>
    /// Gets the current category ID.
    /// </summary>
    int CurrentCategoryId { get; }

    /// <summary>
    /// Adds an annotation.
    /// </summary>
    void AddAnnotation(Entities.Annotation annotation);

    /// <summary>
    /// Requests a canvas redraw.
    /// </summary>
    void RequestRedraw();
}
