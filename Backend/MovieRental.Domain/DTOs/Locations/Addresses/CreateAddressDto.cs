namespace MovieRental.Domain.DTOs.Locations.Addresses;

public class CreateAddressDto
{
    public string Street { get; set; } = string.Empty;
    public string? Street2 { get; set; }
    public string District { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Phone { get; set; } = string.Empty;
    public int CityId { get; set; }
}