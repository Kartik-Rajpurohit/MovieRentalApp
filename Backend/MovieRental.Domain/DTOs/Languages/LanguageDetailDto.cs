namespace MovieRental.Domain.DTOs.Languages;

public class LanguageDetailDto
{
    public int LanguageId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime LastUpdate { get; set; }
    public int FilmCount { get; set; }
}
