using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Domain.Interfaces;

/// <summary>
/// Service interface for annotation operations.
/// </summary>
public interface IAnnotationService
{
    /// <summary>
    /// Creates a new annotation.
    /// </summary>
    Annotation CreateAnnotation(Guid imageAssetId, int categoryId, string annotationType);

    /// <summary>
    /// Validates an annotation.
    /// </summary>
    bool ValidateAnnotation(Annotation annotation);

    /// <summary>
    /// Gets all annotations for an image asset.
    /// </summary>
    IEnumerable<Annotation> GetAnnotationsByImageId(Guid imageAssetId);

    /// <summary>
    /// Deletes an annotation.
    /// </summary>
    void DeleteAnnotation(Guid annotationId);
}
