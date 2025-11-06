using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Application.Services;

/// <summary>
/// Service for managing annotation selections.
/// </summary>
public class SelectionService
{
    private Annotation? _selectedAnnotation;

    /// <summary>
    /// Occurs when the selection changes.
    /// </summary>
    public event EventHandler<Annotation?>? SelectionChanged;

    /// <summary>
    /// Gets the currently selected annotation.
    /// </summary>
    public Annotation? SelectedAnnotation => _selectedAnnotation;

    /// <summary>
    /// Selects an annotation.
    /// </summary>
    public void Select(Annotation? annotation)
    {
        if (_selectedAnnotation != null)
            _selectedAnnotation.IsSelected = false;

        _selectedAnnotation = annotation;

        if (_selectedAnnotation != null)
            _selectedAnnotation.IsSelected = true;

        SelectionChanged?.Invoke(this, _selectedAnnotation);
    }

    /// <summary>
    /// Clears the selection.
    /// </summary>
    public void ClearSelection()
    {
        Select(null);
    }
}
