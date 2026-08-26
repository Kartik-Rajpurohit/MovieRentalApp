namespace MovieRental.Domain.QueryParameters;

public class CityQueryParametersDto : QueryParametersDto
{
    public string? Name { get; set; }
    public int? CountryId { get; set; }
}
