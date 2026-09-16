namespace MovieRental.Domain.DTOs.Categories;

// Response DTO containing category data returned to the client
public class CategoryResponseDto
{
    // Unique ID of the category
    public int CategoryId { get; set; }

    // Category or genre name
    public string Name { get; set; } = string.Empty;

    // Timestamp when the category was last updated
    public DateTime LastUpdate { get; set; }

    // Total movies linked to this category
    public int MovieCount { get; set; }
}
