namespace MovieRental.Domain.DTOs.Locations.Addresses;

// Response DTO containing address information returned to the client
public class AddressResponseDto
{
    // Unique ID of the address
    public int AddressId { get; set; }

    // Street address line
    public string Street { get; set; } = string.Empty;

    // Postal / zip code
    public string? PostalCode { get; set; }

    // Contact phone number
    public string Phone { get; set; } = string.Empty;

    // City ID where address is located
    public int CityId { get; set; }

    // Name of the city
    public string CityName { get; set; } = string.Empty;

    // Name of the country
    public string CountryName { get; set; } = string.Empty;

    // Timestamp when the record was last modified
    public DateTime LastUpdate { get; set; }
}