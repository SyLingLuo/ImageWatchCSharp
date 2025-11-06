using ImageAnnotation.Domain.Entities;

namespace ImageAnnotation.Application.Services;

/// <summary>
/// Service for managing projects.
/// </summary>
public class ProjectService
{
    /// <summary>
    /// Creates a new project.
    /// </summary>
    public Project CreateProject(string name, string description = "")
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
            Taxonomy = new Taxonomy
            {
                Categories = new List<Category>
                {
                    new() { Id = 1, Name = "Default", Color = "#FF0000FF" }
                }
            }
        };
    }

    /// <summary>
    /// Adds an image to a project.
    /// </summary>
    public ImageAsset AddImage(Project project, string filePath, int width, int height)
    {
        var image = new ImageAsset
        {
            Id = Guid.NewGuid(),
            FilePath = filePath,
            Width = width,
            Height = height,
            DateAdded = DateTime.UtcNow
        };

        project.Images.Add(image);
        project.ModifiedAt = DateTime.UtcNow;

        return image;
    }

    /// <summary>
    /// Removes an image from a project.
    /// </summary>
    public void RemoveImage(Project project, ImageAsset image)
    {
        project.Images.Remove(image);
        project.ModifiedAt = DateTime.UtcNow;
    }
}
