namespace MovieRental.Domain.DTOs.Movies;

// Request DTO sent by the client to update an existing movie/film
public class UpdateMovieDto
{
    // Unique ID of the film to update
    public int FilmId { get; set; }

    // Updated movie title
    public string? Title { get; set; }

    // Updated plot description
    public string? Description { get; set; }

    // Updated release year
    public int? ReleaseYear { get; set; }

    // Updated primary language ID
    public int? LanguageId { get; set; }

    // Updated original language ID
    public int? OriginalLanguageId { get; set; }

    // Updated rental duration in days
    public short? RentalDuration { get; set; }

    // Updated rental price
    public decimal? RentalRate { get; set; }

    // Updated runtime in minutes
    public short? Length { get; set; }

    // Updated replacement cost
    public decimal? ReplacementCost { get; set; }

    // Updated age rating
    public string? Rating { get; set; }

    // Updated special features
    public string[]? SpecialFeatures { get; set; }

    // If provided, replaces all existing category links
    public List<int>? CategoryIds { get; set; }

    // If provided, replaces all existing actor links
    public List<int>? ActorIds { get; set; }
}
