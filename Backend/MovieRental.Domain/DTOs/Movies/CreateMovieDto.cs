namespace MovieRental.Domain.DTOs.Movies;

// Used when adding a new film
public class CreateMovieDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }
    public int LanguageId { get; set; }
    public int? OriginalLanguageId { get; set; }
    public short RentalDuration { get; set; } = 3;
    public decimal RentalRate { get; set; } = 4.99m;
    public short? Length { get; set; }
    public decimal ReplacementCost { get; set; } = 19.99m;
    public string? Rating { get; set; }
    public string[]? SpecialFeatures { get; set; }

    // IDs of categories to link
    public List<int> CategoryIds { get; set; } = new();

    // IDs of actors to link
    public List<int> ActorIds { get; set; } = new();
}
