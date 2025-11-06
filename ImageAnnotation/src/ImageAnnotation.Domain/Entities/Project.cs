namespace ImageAnnotation.Domain.Entities;

/// <summary>
/// Represents an annotation project.
/// </summary>
public class Project
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the project file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last modified date.
    /// </summary>
    public DateTime ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the taxonomy (category definitions).
    /// </summary>
    public Taxonomy Taxonomy { get; set; } = new();

    /// <summary>
    /// Gets the image assets in this project.
    /// </summary>
    public List<ImageAsset> Images { get; set; } = new();

    /// <summary>
    /// Gets or sets project settings.
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new();
}
