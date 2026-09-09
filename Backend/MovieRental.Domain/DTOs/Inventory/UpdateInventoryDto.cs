namespace MovieRental.Domain.DTOs.Inventory;

// Request DTO sent by the client to update store assignment of an inventory copy
public class UpdateInventoryDto
{
    // Unique ID of the inventory copy to update
    public int InventoryId { get; set; }

    // New store ID where this copy will be reassigned
    public int? StoreId { get; set; }
}
