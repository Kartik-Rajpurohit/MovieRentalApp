namespace MovieRental.Domain.DTOs.Stores
{
    // Module-specific filter parameters for store branch listings
    public class StoreFilterDto
    {
        // Filters stores by city name
        public string? City { get; set; }

        // Filters stores by country name
        public string? Country { get; set; }
    }
}
