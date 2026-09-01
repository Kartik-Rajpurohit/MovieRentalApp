namespace MovieRental.Domain.DTOs.Locations.Addresses;

public class AddressDetailDto
{
    public int AddressId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Phone { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int StoreCount { get; set; }
    public DateTime LastUpdate { get; set; }
}