using MovieRental.Domain.DTOs.Movies;

namespace MovieRental.Domain.DTOs.Categories;

// Used in category detail page — includes list of movies in this category
public class CategoryDetailDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
    public int FilmCount { get; set; }
}