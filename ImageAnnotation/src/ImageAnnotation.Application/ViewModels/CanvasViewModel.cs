using CommunityToolkit.Mvvm.ComponentModel;
using ImageAnnotation.Domain.Entities;
using ImageAnnotation.Domain.ValueObjects;
using System.Collections.ObjectModel;

namespace ImageAnnotation.Application.ViewModels;

/// <summary>
/// View model for the annotation canvas.
/// </summary>
public partial class CanvasViewModel : ObservableObject
{
    [ObservableProperty]
    private ImageAsset? _currentImage;

    [ObservableProperty]
    private double _zoomLevel = 1.0;

    [ObservableProperty]
    private Point2D _panOffset = new(0, 0);

    [ObservableProperty]
    private Annotation? _selectedAnnotation;

    /// <summary>
    /// Gets the annotations to display.
    /// </summary>
    public ObservableCollection<Annotation> Annotations { get; } = new();

    /// <summary>
    /// Gets or sets the current tool.
    /// </summary>
    public string CurrentTool { get; set; } = "Select";

    /// <summary>
    /// Sets the current image to display.
    /// </summary>
    public void SetCurrentImage(ImageAsset? image)
    {
        CurrentImage = image;
        Annotations.Clear();

        if (image != null)
        {
            foreach (var annotation in image.Annotations)
            {
                Annotations.Add(annotation);
            }
        }

        ResetView();
    }

    /// <summary>
    /// Resets the view to default zoom and pan.
    /// </summary>
    public void ResetView()
    {
        ZoomLevel = 1.0;
        PanOffset = new Point2D(0, 0);
    }

    /// <summary>
    /// Zooms in the canvas.
    /// </summary>
    public void ZoomIn()
    {
        ZoomLevel = Math.Min(ZoomLevel * 1.2, 10.0);
    }

    /// <summary>
    /// Zooms out the canvas.
    /// </summary>
    public void ZoomOut()
    {
        ZoomLevel = Math.Max(ZoomLevel / 1.2, 0.1);
    }

    /// <summary>
    /// Adds an annotation to the canvas.
    /// </summary>
    public void AddAnnotation(Annotation annotation)
    {
        Annotations.Add(annotation);
        CurrentImage?.Annotations.Add(annotation);
    }

    /// <summary>
    /// Removes an annotation from the canvas.
    /// </summary>
    public void RemoveAnnotation(Annotation annotation)
    {
        Annotations.Remove(annotation);
        CurrentImage?.Annotations.Remove(annotation);
    }

    /// <summary>
    /// Selects an annotation.
    /// </summary>
    public void SelectAnnotation(Annotation? annotation)
    {
        if (SelectedAnnotation != null)
            SelectedAnnotation.IsSelected = false;

        SelectedAnnotation = annotation;

        if (SelectedAnnotation != null)
            SelectedAnnotation.IsSelected = true;
    }
}
