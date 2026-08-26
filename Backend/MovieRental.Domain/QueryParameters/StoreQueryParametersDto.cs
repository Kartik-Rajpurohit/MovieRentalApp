using MovieRental.Domain.QueryParameters;

namespace MovieRental.Domain.QueryParameters;

// Store-specific query parameters
public class StoreQueryParametersDto : QueryParametersDto
{
    // Filter by city name
    public string? City { get; set; }

    // Filter by country name
    public string? Country { get; set; }
}
