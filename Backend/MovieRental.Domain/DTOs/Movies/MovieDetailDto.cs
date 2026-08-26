namespace MovieRental.Domain.DTOs.Movies;

// Used in detail page — full film info including all relations
public class MovieDetailDto
{
    public int FilmId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ReleaseYear { get; set; }

    // Primary language
    public int LanguageId { get; set; }
    public string LanguageName { get; set; } = string.Empty;

    // Original language — optional
    public int? OriginalLanguageId { get; set; }
    public string? OriginalLanguageName { get; set; }

    // Rental info
    public short RentalDuration { get; set; }
    public decimal RentalRate { get; set; }
    public short? Length { get; set; }
    public decimal ReplacementCost { get; set; }
    public string? Rating { get; set; }
    public string[]? SpecialFeatures { get; set; }

    // Categories with IDs — for edit form pre-fill
    public List<CategoryDto> Categories { get; set; } = new();

    // Actors with IDs — for edit form pre-fill
    public List<ActorDto> Actors { get; set; } = new();

    // Total inventory copies across all stores
    public int TotalInventory { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class ActorDto
{
    public int ActorId { get; set; }
    public string FullName { get; set; } = string.Empty;
}
