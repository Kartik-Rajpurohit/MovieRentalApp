namespace MovieRental.Domain.DTOs.Locations.Cities;

// Request DTO containing data sent by the client to update an existing city
public class UpdateCityDto
{
    // Unique ID of the city to update
    public int CityId { get; set; }

    // Updated name of the city
    public string Name { get; set; } = string.Empty;

    // Updated country ID
    public int CountryId { get; set; }
}