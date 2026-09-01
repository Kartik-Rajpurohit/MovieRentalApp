namespace MovieRental.Domain.QueryParameters;

public class AddressQueryParametersDto : QueryParametersDto
{
    public int? CityId { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
}
