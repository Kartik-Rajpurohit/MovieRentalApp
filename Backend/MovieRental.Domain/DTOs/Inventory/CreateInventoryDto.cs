namespace MovieRental.Domain.DTOs.Inventory;

// Used when creating a new inventory copy
public class CreateInventoryDto
{
    public int FilmId { get; set; }
    public int StoreId { get; set; }
}
