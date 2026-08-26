namespace MovieRental.Domain.DTOs.Locations.Cities;

public class UpdateCityDto
{
    public int CityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
}