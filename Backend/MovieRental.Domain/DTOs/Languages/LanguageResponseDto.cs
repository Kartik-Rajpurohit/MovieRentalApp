namespace MovieRental.Domain.DTOs.Languages;

public class LanguageResponseDto
{
    public int LanguageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
    public int FilmCount { get; set; }
}

