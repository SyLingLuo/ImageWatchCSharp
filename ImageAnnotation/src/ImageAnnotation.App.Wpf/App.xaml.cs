using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using ImageAnnotation.Application.ViewModels;
using ImageAnnotation.Application.Services;
using ImageAnnotation.Infrastructure.Storage;
using ImageAnnotation.Infrastructure.Logging;
using Serilog;

namespace ImageAnnotation.App.Wpf;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure Serilog
        Log.Logger = LoggingConfiguration.CreateLogger();

        // Build the DI container
        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                // Register ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<CanvasViewModel>();
                services.AddSingleton<ToolbarViewModel>();
                services.AddSingleton<LayerViewModel>();
                services.AddSingleton<PropertyViewModel>();

                // Register Services
                services.AddSingleton<ProjectService>();
                services.AddSingleton<CommandStackService>();
                services.AddSingleton<SelectionService>();
                services.AddSingleton<EventAggregatorService>();

                // Register Infrastructure
                services.AddSingleton<ProjectRepository>();

                // Register main window
                services.AddSingleton<Views.MainWindow>();
            })
            .Build();

        // Show main window
        var mainWindow = _host.Services.GetRequiredService<Views.MainWindow>();
        mainWindow.Show();

        Log.Information("Application started");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("Application shutting down");
        _host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
