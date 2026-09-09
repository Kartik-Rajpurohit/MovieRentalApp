namespace MovieRental.Domain.DTOs.Locations.Cities;

// Response DTO containing city information returned in list endpoints
public class CityResponseDto
{
    // Unique ID of the city
    public int CityId { get; set; }

    // Name of the city
    public string Name { get; set; } = string.Empty;

    // Country ID where the city is located
    public int CountryId { get; set; }

    // Name of the country
    public string CountryName { get; set; } = string.Empty;

    // Number of addresses registered in this city
    public int AddressCount { get; set; }

    // Timestamp when the record was last modified
    public DateTime LastUpdate { get; set; }
}