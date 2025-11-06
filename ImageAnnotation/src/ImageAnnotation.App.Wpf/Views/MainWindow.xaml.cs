using ImageAnnotation.Application.ViewModels;
using System.Windows;

namespace ImageAnnotation.App.Wpf.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "Image Annotation Tool\nVersion 1.0.0\n\nA production-grade WPF annotation application.",
            "About",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
