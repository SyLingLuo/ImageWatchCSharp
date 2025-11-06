using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageAnnotation.Application.ViewModels;

/// <summary>
/// View model for the toolbar.
/// </summary>
public partial class ToolbarViewModel : ObservableObject
{
    [ObservableProperty]
    private string _selectedTool = "Select";

    [ObservableProperty]
    private int _selectedCategoryId = 1;

    /// <summary>
    /// Gets the available tools.
    /// </summary>
    public string[] AvailableTools { get; } = new[]
    {
        "Select",
        "Rectangle",
        "Polygon",
        "Keypoint"
    };

    [RelayCommand]
    private void SelectTool(string toolName)
    {
        SelectedTool = toolName;
    }

    [RelayCommand]
    private void Undo()
    {
        // Command stack undo
    }

    [RelayCommand]
    private void Redo()
    {
        // Command stack redo
    }
}
