using ImageAnnotation.Domain.Entities;
using System.Text.Json;

namespace ImageAnnotation.Infrastructure.Adapters;

/// <summary>
/// COCO format exporter.
/// </summary>
public class CocoExporter : IExporter
{
    public string FormatName => "COCO";
    public string FileExtension => ".json";

    public async Task<ExportResult> ExportAsync(Project project, string filePath)
    {
        try
        {
            var cocoData = new CocoFormat
            {
                Images = new List<CocoImage>(),
                Annotations = new List<CocoAnnotation>(),
                Categories = new List<CocoCategory>()
            };

            // Export categories
            foreach (var category in project.Taxonomy.Categories)
            {
                cocoData.Categories.Add(new CocoCategory
                {
                    Id = category.Id,
                    Name = category.Name
                });
            }

            var annotationId = 1;
            var imageId = 1;
            var annotationCount = 0;

            // Export images and annotations
            foreach (var image in project.Images)
            {
                cocoData.Images.Add(new CocoImage
                {
                    Id = imageId,
                    FileName = Path.GetFileName(image.FilePath),
                    Width = image.Width,
                    Height = image.Height
                });

                foreach (var annotation in image.Annotations.OfType<BBoxAnnotation>())
                {
                    cocoData.Annotations.Add(new CocoAnnotation
                    {
                        Id = annotationId++,
                        ImageId = imageId,
                        CategoryId = annotation.CategoryId,
                        BBox = new[]
                        {
                            annotation.BoundingBox.X,
                            annotation.BoundingBox.Y,
                            annotation.BoundingBox.Width,
                            annotation.BoundingBox.Height
                        },
                        Area = annotation.BoundingBox.Area
                    });
                    annotationCount++;
                }

                imageId++;
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(cocoData, options);
            await File.WriteAllTextAsync(filePath, json);

            return ExportResult.CreateSuccess(project.Images.Count, annotationCount);
        }
        catch (Exception ex)
        {
            return ExportResult.CreateFailure($"Export failed: {ex.Message}");
        }
    }

    // COCO format DTOs
    private class CocoFormat
    {
        public List<CocoImage> Images { get; set; } = new();
        public List<CocoAnnotation> Annotations { get; set; } = new();
        public List<CocoCategory> Categories { get; set; } = new();
    }

    private class CocoImage
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    private class CocoAnnotation
    {
        public int Id { get; set; }
        public int ImageId { get; set; }
        public int CategoryId { get; set; }
        public double[] BBox { get; set; } = Array.Empty<double>();
        public double Area { get; set; }
    }

    private class CocoCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
