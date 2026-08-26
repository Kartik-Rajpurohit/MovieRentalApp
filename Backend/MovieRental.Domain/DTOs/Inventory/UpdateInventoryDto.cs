namespace MovieRental.Domain.DTOs.Inventory;

// Used when updating store assignment of an inventory copy
public class UpdateInventoryDto
{
    public int InventoryId { get; set; }
    public int? StoreId { get; set; }
}
