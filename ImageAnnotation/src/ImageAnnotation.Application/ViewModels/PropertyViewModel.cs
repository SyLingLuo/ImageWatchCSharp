using CommunityToolkit.Mvvm.ComponentModel;
using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Application.ViewModels;

/// <summary>
/// View model for the property panel.
/// </summary>
public partial class PropertyViewModel : ObservableObject
{
    [ObservableProperty]
    private Annotation? _selectedAnnotation;

    [ObservableProperty]
    private string _categoryName = string.Empty;

    [ObservableProperty]
    private string _annotationType = string.Empty;

    partial void OnSelectedAnnotationChanged(Annotation? value)
    {
        if (value != null)
        {
            CategoryName = value.CategoryName;
            AnnotationType = value.GetType().Name;
        }
        else
        {
            CategoryName = string.Empty;
            AnnotationType = string.Empty;
        }
    }
}
