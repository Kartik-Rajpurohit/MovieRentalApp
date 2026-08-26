namespace MovieRental.Domain.DTOs.Locations.Cities;

public class CreateCityDto
{
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
}