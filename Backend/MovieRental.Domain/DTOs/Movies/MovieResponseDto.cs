namespace MovieRental.Domain.DTOs.Movies;

// Response DTO containing essential movie information returned in list endpoints
public class MovieResponseDto
{
    // Unique ID of the film
    public int FilmId { get; set; }

    // Film title
    public string Title { get; set; } = string.Empty;

    // Short plot synopsis
    public string? Description { get; set; }

    // Release year
    public int? ReleaseYear { get; set; }

    // Primary language ID
    public int LanguageId { get; set; }

    // Primary language name
    public string LanguageName { get; set; } = string.Empty;

    // Rental period in days
    public short RentalDuration { get; set; }

    // Cost to rent the movie
    public decimal RentalRate { get; set; }

    // Runtime in minutes
    public short? Length { get; set; }

    // Replacement fee
    public decimal ReplacementCost { get; set; }

    // MPAA rating: G, PG, PG-13, R, NC-17
    public string? Rating { get; set; }

    // Category/genre names
    public List<string> Categories { get; set; } = new();

    // Cast actor names
    public List<string> Actors { get; set; } = new();
}
