namespace MovieRental.Domain.DTOs.Locations.Countries;

public class CountryResponseDto
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CityCount { get; set; }
    public DateTime LastUpdate { get; set; }
}
