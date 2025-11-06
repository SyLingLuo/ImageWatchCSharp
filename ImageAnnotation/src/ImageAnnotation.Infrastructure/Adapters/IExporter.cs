using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Infrastructure.Adapters;

/// <summary>
/// Interface for data export.
/// </summary>
public interface IExporter
{
    /// <summary>
    /// Gets the format name.
    /// </summary>
    string FormatName { get; }

    /// <summary>
    /// Gets the file extension.
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Exports annotations to a file.
    /// </summary>
    Task<ExportResult> ExportAsync(Project project, string filePath);
}

/// <summary>
/// Result of an export operation.
/// </summary>
public class ExportResult
{
    /// <summary>
    /// Gets whether the export succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the number of images exported.
    /// </summary>
    public int ImagesExported { get; init; }

    /// <summary>
    /// Gets the number of annotations exported.
    /// </summary>
    public int AnnotationsExported { get; init; }

    /// <summary>
    /// Gets the error message if export failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful export result.
    /// </summary>
    public static ExportResult CreateSuccess(int images, int annotations)
        => new() { Success = true, ImagesExported = images, AnnotationsExported = annotations };

    /// <summary>
    /// Creates a failed export result.
    /// </summary>
    public static ExportResult CreateFailure(string error)
        => new() { Success = false, ErrorMessage = error };
}
