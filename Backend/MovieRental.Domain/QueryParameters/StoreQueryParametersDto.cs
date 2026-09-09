using MovieRental.Domain.QueryParameters;

namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate stores returned by the API
public class StoreQueryParametersDto : QueryParametersDto
{
    // Filter by city name
    public string? City { get; set; }

    // Filter by country name
    public string? Country { get; set; }
}
