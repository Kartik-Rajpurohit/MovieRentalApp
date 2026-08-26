namespace MovieRental.Domain.DTOs.Inventory;

// Returned in detail view — includes rental history count
public class InventoryDetailDto
{
    public int InventoryId { get; set; }
    public int FilmId { get; set; }
    public string FilmTitle { get; set; } = string.Empty;
    public int StoreId { get; set; }
    public bool IsAvailable { get; set; }
    public int TotalRentals { get; set; }   // how many times this copy was rented
    public DateTime LastUpdate { get; set; }
}
