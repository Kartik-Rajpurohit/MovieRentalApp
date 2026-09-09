namespace MovieRental.Domain.DTOs.Inventory;

// Request DTO sent by the client to add a physical movie copy to a store's inventory
public class CreateInventoryDto
{
    // Film ID of the movie being added
    public int FilmId { get; set; }

    // Store ID where this copy will be located
    public int StoreId { get; set; }
}
