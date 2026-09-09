namespace MovieRental.Domain.DTOs.Inventory;

// Response DTO containing inventory copy summary data returned in list endpoints
public class InventoryResponseDto
{
    // Unique ID of the inventory copy
    public int InventoryId { get; set; }

    // Film ID of the movie
    public int FilmId { get; set; }

    // Title of the movie
    public string FilmTitle { get; set; } = string.Empty;

    // Store ID where this copy is located
    public int StoreId { get; set; }

    // Indicates whether this copy is available to rent (true) or currently rented out (false)
    public bool IsAvailable { get; set; }

    // Timestamp of last modification
    public DateTime LastUpdate { get; set; }
}
