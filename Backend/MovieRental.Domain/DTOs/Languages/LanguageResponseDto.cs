namespace MovieRental.Domain.DTOs.Languages;

// Response DTO containing language data returned in list endpoints
public class LanguageResponseDto
{
    // Unique ID of the language
    public int LanguageId { get; set; }

    // Name of the language
    public string Name { get; set; } = string.Empty;

    // Timestamp when the language was last updated
    public DateTime LastUpdate { get; set; }

    // Total films linked to this language
    public int FilmCount { get; set; }
}

