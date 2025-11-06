using ImageAnnotation.Domain.Entities;
using System.Text;

namespace ImageAnnotation.Infrastructure.Adapters;

/// <summary>
/// YOLO format exporter (one .txt file per image).
/// </summary>
public class YoloExporter : IExporter
{
    public string FormatName => "YOLO";
    public string FileExtension => ".txt";

    public async Task<ExportResult> ExportAsync(Project project, string outputDirectory)
    {
        try
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var annotationCount = 0;

            // Export class names
            var classesPath = Path.Combine(outputDirectory, "classes.txt");
            var classNames = project.Taxonomy.Categories
                .OrderBy(c => c.Id)
                .Select(c => c.Name);
            await File.WriteAllLinesAsync(classesPath, classNames);

            // Export annotations for each image
            foreach (var image in project.Images)
            {
                var fileName = Path.GetFileNameWithoutExtension(image.FilePath);
                var outputPath = Path.Combine(outputDirectory, $"{fileName}.txt");

                var lines = new List<string>();

                foreach (var annotation in image.Annotations.OfType<BBoxAnnotation>())
                {
                    // YOLO format: <class_id> <x_center> <y_center> <width> <height> (normalized 0-1)
                    var xCenter = (annotation.BoundingBox.X + annotation.BoundingBox.Width / 2) / image.Width;
                    var yCenter = (annotation.BoundingBox.Y + annotation.BoundingBox.Height / 2) / image.Height;
                    var width = annotation.BoundingBox.Width / image.Width;
                    var height = annotation.BoundingBox.Height / image.Height;

                    var classId = annotation.CategoryId - 1; // YOLO uses 0-based indexing
                    lines.Add($"{classId} {xCenter:F6} {yCenter:F6} {width:F6} {height:F6}");
                    annotationCount++;
                }

                await File.WriteAllLinesAsync(outputPath, lines);
            }

            return ExportResult.CreateSuccess(project.Images.Count, annotationCount);
        }
        catch (Exception ex)
        {
            return ExportResult.CreateFailure($"Export failed: {ex.Message}");
        }
    }
}
