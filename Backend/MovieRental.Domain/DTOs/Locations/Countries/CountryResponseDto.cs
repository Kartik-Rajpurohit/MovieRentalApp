namespace MovieRental.Domain.DTOs.Locations.Countries;

// Response DTO containing country information returned in list endpoints
public class CountryResponseDto
{
    // Unique ID of the country
    public int CountryId { get; set; }

    // Name of the country
    public string Name { get; set; } = string.Empty;

    // Number of cities registered in this country
    public int CityCount { get; set; }

    // Timestamp when the record was last modified
    public DateTime LastUpdate { get; set; }
}
