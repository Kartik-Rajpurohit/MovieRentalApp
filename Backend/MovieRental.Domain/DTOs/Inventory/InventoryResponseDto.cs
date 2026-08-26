namespace MovieRental.Domain.DTOs.Inventory;

// Returned in paginated list
public class InventoryResponseDto
{
    public int InventoryId { get; set; }
    public int FilmId { get; set; }
    public string FilmTitle { get; set; } = string.Empty;
    public int StoreId { get; set; }
    public bool IsAvailable { get; set; }  // true if no active rental exists
    public DateTime LastUpdate { get; set; }
}
