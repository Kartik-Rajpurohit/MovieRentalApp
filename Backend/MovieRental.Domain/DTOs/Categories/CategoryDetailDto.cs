using MovieRental.Domain.DTOs.Movies;

namespace MovieRental.Domain.DTOs.Categories;

// Response DTO containing detailed information about a movie category/genre
public class CategoryDetailDto
{
    // Unique ID of the category
    public int CategoryId { get; set; }

    // Category or genre name (e.g. "Action", "Comedy")
    public string Name { get; set; } = string.Empty;

    // Timestamp when the category was last updated
    public DateTime LastUpdate { get; set; }

    // Total number of films in this category
    public int FilmCount { get; set; }
}