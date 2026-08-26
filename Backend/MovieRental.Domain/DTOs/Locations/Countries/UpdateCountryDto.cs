namespace MovieRental.Domain.DTOs.Locations.Countries;

public class UpdateCountryDto
{
    public int CountryId { get; set; }
    public string Name { get; set; } = string.Empty;
}
