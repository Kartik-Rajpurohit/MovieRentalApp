namespace MovieRental.Domain.DTOs.Categories;

// Used in paginated list view
public class CategoryResponseDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }

    // Total films linked to this category
    public int FilmCount { get; set; }
}
