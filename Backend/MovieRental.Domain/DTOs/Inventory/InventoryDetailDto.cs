namespace MovieRental.Domain.DTOs.Inventory;

// Response DTO containing detailed information about a physical inventory copy and rental history
public class InventoryDetailDto
{
    // Unique ID of the inventory copy
    public int InventoryId { get; set; }

    // Film ID of the movie
    public int FilmId { get; set; }

    // Title of the movie
    public string FilmTitle { get; set; } = string.Empty;

    // Store ID where this copy belongs
    public int StoreId { get; set; }

    // Whether this copy is currently available to rent
    public bool IsAvailable { get; set; }

    // How many times this copy was rented all-time
    public int TotalRentals { get; set; }

    // Timestamp of last modification
    public DateTime LastUpdate { get; set; }
}
