using CommunityToolkit.Mvvm.ComponentModel;
using ImageAnnotation.Domain.Entities;
using System.Collections.ObjectModel;

namespace ImageAnnotation.Application.ViewModels;

/// <summary>
/// View model for the layer panel.
/// </summary>
public partial class LayerViewModel : ObservableObject
{
    [ObservableProperty]
    private Annotation? _selectedAnnotation;

    /// <summary>
    /// Gets the annotations (layers).
    /// </summary>
    public ObservableCollection<Annotation> Annotations { get; } = new();

    /// <summary>
    /// Sets the annotations to display.
    /// </summary>
    public void SetAnnotations(IEnumerable<Annotation> annotations)
    {
        Annotations.Clear();
        foreach (var annotation in annotations)
        {
            Annotations.Add(annotation);
        }
    }

    /// <summary>
    /// Toggles annotation visibility.
    /// </summary>
    public void ToggleVisibility(Annotation annotation)
    {
        annotation.IsVisible = !annotation.IsVisible;
    }

    /// <summary>
    /// Deletes an annotation.
    /// </summary>
    public void DeleteAnnotation(Annotation annotation)
    {
        Annotations.Remove(annotation);
    }
}
