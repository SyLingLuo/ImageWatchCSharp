using ImageAnnotation.Domain.Entities;
using System.Text.Json;

namespace ImageAnnotation.Infrastructure.Adapters;

/// <summary>
/// COCO format importer.
/// </summary>
public class CocoImporter : IImporter
{
    public string FormatName => "COCO";
    public string FileExtension => ".json";

    public async Task<ImportResult> ImportAsync(string filePath, Project project)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var cocoData = JsonSerializer.Deserialize<CocoFormat>(json);

            if (cocoData == null)
                return ImportResult.CreateFailure("Failed to parse COCO JSON");

            var annotationCount = 0;

            // Import categories
            if (cocoData.Categories != null)
            {
                foreach (var cat in cocoData.Categories)
                {
                    if (!project.Taxonomy.Categories.Any(c => c.Id == cat.Id))
                    {
                        project.Taxonomy.AddCategory(new Category
                        {
                            Id = cat.Id,
                            Name = cat.Name ?? $"Category{cat.Id}"
                        });
                    }
                }
            }

            // Import images and annotations
            if (cocoData.Images != null)
            {
                var imageDict = cocoData.Images.ToDictionary(img => img.Id);

                foreach (var img in cocoData.Images)
                {
                    var imageAsset = new ImageAsset
                    {
                        Id = Guid.NewGuid(),
                        FilePath = img.FileName ?? string.Empty,
                        Width = img.Width,
                        Height = img.Height,
                        DateAdded = DateTime.UtcNow
                    };

                    // Add annotations for this image
                    if (cocoData.Annotations != null)
                    {
                        var imageAnnotations = cocoData.Annotations.Where(a => a.ImageId == img.Id);

                        foreach (var ann in imageAnnotations)
                        {
                            if (ann.BBox != null && ann.BBox.Length >= 4)
                            {
                                var bbox = new BBoxAnnotation
                                {
                                    Id = Guid.NewGuid(),
                                    ImageAssetId = imageAsset.Id,
                                    CategoryId = ann.CategoryId,
                                    CategoryName = project.Taxonomy.GetCategoryById(ann.CategoryId)?.Name ?? "Unknown",
                                    BoundingBox = new Domain.ValueObjects.RectF(
                                        ann.BBox[0], ann.BBox[1], ann.BBox[2], ann.BBox[3]),
                                    CreatedAt = DateTime.UtcNow,
                                    ModifiedAt = DateTime.UtcNow
                                };

                                imageAsset.Annotations.Add(bbox);
                                annotationCount++;
                            }
                        }
                    }

                    project.Images.Add(imageAsset);
                }
            }

            return ImportResult.CreateSuccess(project.Images.Count, annotationCount);
        }
        catch (Exception ex)
        {
            return ImportResult.CreateFailure($"Import failed: {ex.Message}");
        }
    }

    // COCO format DTOs
    private class CocoFormat
    {
        public List<CocoImage>? Images { get; set; }
        public List<CocoAnnotation>? Annotations { get; set; }
        public List<CocoCategory>? Categories { get; set; }
    }

    private class CocoImage
    {
        public int Id { get; set; }
        public string? FileName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    private class CocoAnnotation
    {
        public int Id { get; set; }
        public int ImageId { get; set; }
        public int CategoryId { get; set; }
        public double[]? BBox { get; set; }
        public double Area { get; set; }
    }

    private class CocoCategory
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
