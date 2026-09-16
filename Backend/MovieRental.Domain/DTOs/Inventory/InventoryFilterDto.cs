namespace MovieRental.Domain.DTOs.Inventory
{
    // Module-specific filter parameters for inventory copy listings
    public class InventoryFilterDto
    {
        // Filters inventory copies of a specific movie ID
        public int? MovieId { get; set; }

        // Filters inventory copies located at a specific store ID
        public int? StoreId { get; set; }

        // Filters inventory copies by availability (true = available, false = rented out)
        public bool? IsAvailable { get; set; }
    }
}
