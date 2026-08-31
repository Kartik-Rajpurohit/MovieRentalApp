using MovieRental.Domain.QueryParameters;

namespace MovieRental.Domain.QueryParameters;

// Film-specific query parameters — extends base pagination/sort/search
public class MovieQueryParametersDto : QueryParametersDto
{
    // Filter by language ID
    public int? LanguageId { get; set; }

    // Filter by category ID
    public int? CategoryId { get; set; }

    // Filter by MPAA rating: G, PG, PG-13, R, NC-17
    public string? Rating { get; set; }

    // Filter by release year
    public int? ReleaseYear { get; set; }
    // Filter by rental rate range
    public decimal? MinRentalRate { get; set; }
    public decimal? MaxRentalRate { get; set; }

    // Filter by length range (in minutes)
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
}
