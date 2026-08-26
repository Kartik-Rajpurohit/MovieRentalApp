namespace MovieRental.Domain.DTOs.Locations.Cities;

public class CityDetailDto
{
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public string CountryName { get; set; } = string.Empty;
    public int AddressCount { get; set; }
    public DateTime LastUpdate { get; set; }
}