namespace MovieRental.Domain.DTOs.Languages;

// Response DTO containing detailed information about a language, including linked films count
public class LanguageDetailDto
{
    // Unique ID of the language
    public int LanguageId { get; set; }

    // Name of the language
    public string Name { get; set; } = string.Empty;

    // Timestamp when the language was last updated
    public DateTime LastUpdate { get; set; }

    // Total number of films in this language
    public int FilmCount { get; set; }
}
