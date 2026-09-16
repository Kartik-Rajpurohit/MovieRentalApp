namespace MovieRental.Domain.DTOs.Locations.Cities
{
    // Module-specific filter parameters for city listings
    public class CityFilterDto
    {
        // Filters cities by name
        public string? Name { get; set; }

        // Filters cities belonging to a specific country ID
        public int? CountryId { get; set; }
    }
}
