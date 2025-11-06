using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Infrastructure.Adapters;

/// <summary>
/// Interface for data import.
/// </summary>
public interface IImporter
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
    /// Imports annotations from a file.
    /// </summary>
    Task<ImportResult> ImportAsync(string filePath, Project project);
}

/// <summary>
/// Result of an import operation.
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Gets whether the import succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the number of images imported.
    /// </summary>
    public int ImagesImported { get; init; }

    /// <summary>
    /// Gets the number of annotations imported.
    /// </summary>
    public int AnnotationsImported { get; init; }

    /// <summary>
    /// Gets the error message if import failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful import result.
    /// </summary>
    public static ImportResult CreateSuccess(int images, int annotations)
        => new() { Success = true, ImagesImported = images, AnnotationsImported = annotations };

    /// <summary>
    /// Creates a failed import result.
    /// </summary>
    public static ImportResult CreateFailure(string error)
        => new() { Success = false, ErrorMessage = error };
}
