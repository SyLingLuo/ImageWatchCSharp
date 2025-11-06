namespace ImageAnnotation.Domain.Entities;

/// <summary>
/// Represents the taxonomy (category system) for a project.
/// </summary>
public class Taxonomy
{
    /// <summary>
    /// Gets the categories in this taxonomy.
    /// </summary>
    public List<Category> Categories { get; set; } = new();

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    public Category? GetCategoryById(int id)
        => Categories.FirstOrDefault(c => c.Id == id);

    /// <summary>
    /// Gets a category by name.
    /// </summary>
    public Category? GetCategoryByName(string name)
        => Categories.FirstOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Adds a new category.
    /// </summary>
    public void AddCategory(Category category)
    {
        if (Categories.Any(c => c.Id == category.Id))
            throw new InvalidOperationException($"Category with ID {category.Id} already exists");

        Categories.Add(category);
    }
}

/// <summary>
/// Represents a category in the taxonomy.
/// </summary>
public class Category
{
    /// <summary>
    /// Gets or sets the category ID.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category color (ARGB hex string, e.g., "#FFFF0000").
    /// </summary>
    public string Color { get; set; } = "#FF0000FF";

    /// <summary>
    /// Gets or sets the parent category ID (for hierarchical categories).
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new();
}
