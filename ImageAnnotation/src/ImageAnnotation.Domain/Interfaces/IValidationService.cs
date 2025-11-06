using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Domain.Interfaces;

/// <summary>
/// Service interface for validation operations.
/// </summary>
public interface IValidationService
{
    /// <summary>
    /// Validates a project.
    /// </summary>
    ValidationResult ValidateProject(Project project);

    /// <summary>
    /// Validates an annotation.
    /// </summary>
    ValidationResult ValidateAnnotation(Annotation annotation);

    /// <summary>
    /// Validates an image asset.
    /// </summary>
    ValidationResult ValidateImageAsset(ImageAsset imageAsset);
}

/// <summary>
/// Represents the result of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets whether the validation succeeded.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Gets the validation warnings.
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static ValidationResult Failure(params string[] errors)
        => new() { IsValid = false, Errors = errors.ToList() };
}
