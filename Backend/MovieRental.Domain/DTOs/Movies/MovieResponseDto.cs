namespace MovieRental.Domain.DTOs.Movies;

// Used in paginated list view — shows essential film info
public class MovieResponseDto
{
    public int FilmId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }

    // Language
    public int LanguageId { get; set; }
    public string LanguageName { get; set; } = string.Empty;

    // Rental info
    public short RentalDuration { get; set; }
    public decimal RentalRate { get; set; }
    public short? Length { get; set; }
    public decimal ReplacementCost { get; set; }

    // MPAA rating: G, PG, PG-13, R, NC-17
    public string? Rating { get; set; }

    // Categories — comma separated e.g. "Action, Comedy"
    public List<string> Categories { get; set; } = new();

    // Actors — full names e.g. "Tom Hanks, Brad Pitt"
    public List<string> Actors { get; set; } = new();
}
