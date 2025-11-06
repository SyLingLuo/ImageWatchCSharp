using ImageAnnotation.Domain.Entities;
using ImageAnnotation.Infrastructure.Storage;
using System.IO;

namespace ImageAnnotation.Integration.Tests;

public class ProjectRepositoryTests
{
    [Fact]
    public async Task SaveAndLoad_PreservesProjectData()
    {
        // Arrange
        var repository = new ProjectRepository();
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.json");

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow
        };

        project.Taxonomy.AddCategory(new Category
        {
            Id = 1,
            Name = "Person",
            Color = "#FF0000FF"
        });

        var imageAsset = new ImageAsset
        {
            Id = Guid.NewGuid(),
            FilePath = "test.jpg",
            Width = 1920,
            Height = 1080,
            DateAdded = DateTime.UtcNow
        };

        project.Images.Add(imageAsset);

        try
        {
            // Act - Save
            await repository.SaveAsync(project, tempFile);

            // Assert - File exists
            Assert.True(File.Exists(tempFile));

            // Act - Load
            var loadedProject = await repository.LoadAsync(tempFile);

            // Assert - Data preserved
            Assert.NotNull(loadedProject);
            Assert.Equal(project.Id, loadedProject.Id);
            Assert.Equal(project.Name, loadedProject.Name);
            Assert.Equal(project.Description, loadedProject.Description);
            Assert.Single(loadedProject.Taxonomy.Categories);
            Assert.Equal("Person", loadedProject.Taxonomy.Categories[0].Name);
            Assert.Single(loadedProject.Images);
            Assert.Equal("test.jpg", loadedProject.Images[0].FilePath);
            Assert.Equal(1920, loadedProject.Images[0].Width);
            Assert.Equal(1080, loadedProject.Images[0].Height);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task LoadAsync_ReturnsNullForNonExistentFile()
    {
        // Arrange
        var repository = new ProjectRepository();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.json");

        // Act
        var result = await repository.LoadAsync(nonExistentFile);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void Exists_ReturnsTrueForExistingFile()
    {
        // Arrange
        var repository = new ProjectRepository();
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.txt");
        File.WriteAllText(tempFile, "test");

        try
        {
            // Act
            var result = repository.Exists(tempFile);

            // Assert
            Assert.True(result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Exists_ReturnsFalseForNonExistentFile()
    {
        // Arrange
        var repository = new ProjectRepository();
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.txt");

        // Act
        var result = repository.Exists(nonExistentFile);

        // Assert
        Assert.False(result);
    }
}
