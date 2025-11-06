using ImageAnnotation.Domain.Entities;
using System.Text.Json;

namespace ImageAnnotation.Infrastructure.Storage;

/// <summary>
/// Repository for managing projects.
/// </summary>
public class ProjectRepository
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ProjectRepository()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <summary>
    /// Saves a project to a file.
    /// </summary>
    public async Task SaveAsync(Project project, string filePath)
    {
        project.FilePath = filePath;
        project.ModifiedAt = DateTime.UtcNow;

        var json = JsonSerializer.Serialize(project, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Loads a project from a file.
    /// </summary>
    public async Task<Project?> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        var json = await File.ReadAllTextAsync(filePath);
        var project = JsonSerializer.Deserialize<Project>(json, _jsonOptions);

        if (project != null)
        {
            project.FilePath = filePath;
        }

        return project;
    }

    /// <summary>
    /// Checks if a project file exists.
    /// </summary>
    public bool Exists(string filePath)
    {
        return File.Exists(filePath);
    }
}
