namespace MovieRental.Domain.DTOs.Locations.Addresses;

// Request DTO containing data sent by the client to create a new address
public class CreateAddressDto
{
    // Street address
    public string Street { get; set; } = string.Empty;

    // Postal / zip code
    public string? PostalCode { get; set; }

    // Contact phone number
    public string Phone { get; set; } = string.Empty;

    // City ID for this address
    public int CityId { get; set; }
}