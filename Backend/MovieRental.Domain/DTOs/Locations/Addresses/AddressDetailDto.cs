namespace MovieRental.Domain.DTOs.Locations.Addresses;

// Response DTO containing detailed address information with linked city, country, and usage counts
public class AddressDetailDto
{
    // Unique ID of the address
    public int AddressId { get; set; }

    // Street address line
    public string Street { get; set; } = string.Empty;

    // Postal / zip code
    public string? PostalCode { get; set; }

    // Contact telephone number
    public string Phone { get; set; } = string.Empty;

    // City ID where address is located
    public int CityId { get; set; }

    // Name of the city
    public string CityName { get; set; } = string.Empty;

    // Name of the country
    public string CountryName { get; set; } = string.Empty;

    // Number of users residing at this address
    public int UserCount { get; set; }

    // Number of stores located at this address
    public int StoreCount { get; set; }

    // Timestamp when the record was last modified
    public DateTime LastUpdate { get; set; }
}