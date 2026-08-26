namespace MovieRental.Domain.DTOs.Movies;

// Used when editing a film — all fields optional except FilmId
public class UpdateMovieDto
{
    public int FilmId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int? LanguageId { get; set; }
    public int? OriginalLanguageId { get; set; }
    public short? RentalDuration { get; set; }
    public decimal? RentalRate { get; set; }
    public short? Length { get; set; }
    public decimal? ReplacementCost { get; set; }
    public string? Rating { get; set; }
    public string[]? SpecialFeatures { get; set; }

    // If provided, replaces all existing category links
    public List<int>? CategoryIds { get; set; }

    // If provided, replaces all existing actor links
    public List<int>? ActorIds { get; set; }
}
