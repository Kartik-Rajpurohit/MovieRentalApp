namespace MovieRental.Domain.DTOs.Locations.Countries;

// Request DTO containing data sent by the client to create a new country
public class CreateCountryDto
{
    // Name of the new country
    public string Name { get; set; } = string.Empty;
}
