namespace MovieRental.Domain.DTOs.Locations.Cities;

// Request DTO containing data sent by the client to create a new city
public class CreateCityDto
{
    // Name of the new city
    public string Name { get; set; } = string.Empty;

    // Country ID where the city belongs
    public int CountryId { get; set; }
}