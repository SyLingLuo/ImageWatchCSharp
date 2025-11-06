using Serilog;
using Serilog.Events;

namespace ImageAnnotation.Infrastructure.Logging;

/// <summary>
/// Configures logging for the application.
/// </summary>
public static class LoggingConfiguration
{
    /// <summary>
    /// Configures Serilog.
    /// </summary>
    public static ILogger CreateLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}
