namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate addresses returned by the API
public class AddressQueryParametersDto : QueryParametersDto
{
    // Filters addresses belonging to a specific city ID
    public int? CityId { get; set; }

    // Filters addresses by city name
    public string? City { get; set; }

    // Filters addresses by postal / zip code
    public string? PostalCode { get; set; }
}
