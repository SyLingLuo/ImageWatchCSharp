using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Application.ViewModels;

/// <summary>
/// Main view model for the application.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private Project? _currentProject;

    [ObservableProperty]
    private ImageAsset? _currentImage;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isProjectLoaded;

    /// <summary>
    /// Gets the canvas view model.
    /// </summary>
    public CanvasViewModel CanvasViewModel { get; }

    /// <summary>
    /// Gets the toolbar view model.
    /// </summary>
    public ToolbarViewModel ToolbarViewModel { get; }

    /// <summary>
    /// Gets the layer view model.
    /// </summary>
    public LayerViewModel LayerViewModel { get; }

    /// <summary>
    /// Gets the property view model.
    /// </summary>
    public PropertyViewModel PropertyViewModel { get; }

    public MainViewModel(
        CanvasViewModel canvasViewModel,
        ToolbarViewModel toolbarViewModel,
        LayerViewModel layerViewModel,
        PropertyViewModel propertyViewModel)
    {
        CanvasViewModel = canvasViewModel;
        ToolbarViewModel = toolbarViewModel;
        LayerViewModel = layerViewModel;
        PropertyViewModel = propertyViewModel;
    }

    [RelayCommand]
    private void NewProject()
    {
        CurrentProject = new Project
        {
            Id = Guid.NewGuid(),
            Name = "New Project",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };
        IsProjectLoaded = true;
        StatusMessage = "New project created";
    }

    [RelayCommand]
    private async Task OpenProjectAsync()
    {
        // Placeholder for file dialog
        await Task.CompletedTask;
        StatusMessage = "Open project";
    }

    [RelayCommand]
    private async Task SaveProjectAsync()
    {
        if (CurrentProject != null)
        {
            CurrentProject.ModifiedAt = DateTime.UtcNow;
            await Task.CompletedTask;
            StatusMessage = "Project saved";
        }
    }

    [RelayCommand]
    private async Task AddImageAsync()
    {
        // Placeholder for file dialog
        await Task.CompletedTask;
        StatusMessage = "Add image";
    }

    partial void OnCurrentProjectChanged(Project? value)
    {
        IsProjectLoaded = value != null;
    }

    partial void OnCurrentImageChanged(ImageAsset? value)
    {
        CanvasViewModel.SetCurrentImage(value);
    }
}
