namespace MovieRental.Domain.DTOs.Locations.Addresses
{
    // Module-specific filter parameters for address listings
    public class AddressFilterDto
    {
        // Filters addresses belonging to a specific city ID
        public int? CityId { get; set; }

        // Filters addresses by city name
        public string? City { get; set; }

        // Filters addresses by postal / zip code
        public string? PostalCode { get; set; }
    }
}
