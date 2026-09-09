namespace MovieRental.Domain.DTOs.Movies;

// Response DTO containing full details of a movie, including categories, cast actors, and languages
public class MovieDetailDto
{
    // Unique ID of the film
    public int FilmId { get; set; }

    // Film title
    public string Title { get; set; } = string.Empty;

    // Full synopsis or description
    public string? Description { get; set; }

    // Release year
    public int? ReleaseYear { get; set; }

    // Primary language ID
    public int LanguageId { get; set; }

    // Primary language name
    public string LanguageName { get; set; } = string.Empty;

    // Original language ID (if translated or dubbed)
    public int? OriginalLanguageId { get; set; }

    // Original language name
    public string? OriginalLanguageName { get; set; }

    // Rental period in days
    public short RentalDuration { get; set; }

    // Cost to rent the movie
    public decimal RentalRate { get; set; }

    // Duration in minutes
    public short? Length { get; set; }

    // Replacement cost if lost
    public decimal ReplacementCost { get; set; }

    // Age rating (PG, PG-13, R, etc.)
    public string? Rating { get; set; }

    // Special features list
    public string[]? SpecialFeatures { get; set; }

    // Assigned categories with IDs for UI pre-fill
    public List<CategoryDto> Categories { get; set; } = new();

    // Assigned actors with IDs for UI pre-fill
    public List<ActorDto> Actors { get; set; } = new();

    // Total inventory copies across all stores
    public int TotalInventory { get; set; }
}

// Nested DTO representing a category associated with a movie
public class CategoryDto
{
    // Category ID
    public int CategoryId { get; set; }

    // Category name
    public string Name { get; set; } = string.Empty;
}

// Nested DTO representing an actor associated with a movie
public class ActorDto
{
    // Actor ID
    public int ActorId { get; set; }

    // Full name of the actor
    public string FullName { get; set; } = string.Empty;
}
