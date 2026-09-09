namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate cities returned by the API
public class CityQueryParametersDto : QueryParametersDto
{
    // Filters cities by name
    public string? Name { get; set; }

    // Filters cities belonging to a specific country ID
    public int? CountryId { get; set; }
}
