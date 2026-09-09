namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate countries returned by the API
public class CountryQueryParametersDto : QueryParametersDto
{
    // Filters countries by country name
    public string? Name { get; set; }
}
