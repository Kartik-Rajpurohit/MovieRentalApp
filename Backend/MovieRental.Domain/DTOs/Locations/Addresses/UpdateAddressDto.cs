namespace MovieRental.Domain.DTOs.Locations.Addresses;

// Request DTO containing data sent by the client to update an existing address
public class UpdateAddressDto
{
    // Unique ID of the address to update
    public int AddressId { get; set; }

    // Updated street address
    public string Street { get; set; } = string.Empty;

    // Updated postal / zip code
    public string? PostalCode { get; set; }

    // Updated phone number
    public string Phone { get; set; } = string.Empty;

    // Updated city ID
    public int CityId { get; set; }
}