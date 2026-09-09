namespace MovieRental.Domain.DTOs.Movies;

// Request DTO sent by the client to create a new movie/film
public class CreateMovieDto
{
    // Movie title
    public string Title { get; set; } = string.Empty;

    // Plot description or synopsis
    public string? Description { get; set; }

    // Release year
    public int? ReleaseYear { get; set; }

    // Language ID for the primary audio track
    public int LanguageId { get; set; }

    // Optional original language ID
    public int? OriginalLanguageId { get; set; }

    // Default rental period in days
    public short RentalDuration { get; set; } = 3;

    // Cost to rent the movie
    public decimal RentalRate { get; set; } = 4.99m;

    // Movie runtime in minutes
    public short? Length { get; set; }

    // Charge if the DVD/disc is lost or damaged
    public decimal ReplacementCost { get; set; } = 19.99m;

    // MPAA age rating (e.g. PG, PG-13, R)
    public string? Rating { get; set; }

    // Special disc features (e.g. Trailers, Commentaries, Deleted Scenes)
    public string[]? SpecialFeatures { get; set; }

    // IDs of categories/genres to associate with this film
    public List<int> CategoryIds { get; set; } = new();

    // IDs of cast actors to associate with this film
    public List<int> ActorIds { get; set; } = new();
}
