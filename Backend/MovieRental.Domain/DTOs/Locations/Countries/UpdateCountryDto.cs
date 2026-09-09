namespace MovieRental.Domain.DTOs.Locations.Countries;

// Request DTO containing data sent by the client to update an existing country
public class UpdateCountryDto
{
    // Unique ID of the country to update
    public int CountryId { get; set; }

    // Updated name of the country
    public string Name { get; set; } = string.Empty;
}
