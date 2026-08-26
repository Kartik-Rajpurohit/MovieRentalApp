namespace MovieRental.Domain.DTOs.Rentals;

public class CreateRentalDto
{
    public int InventoryId { get; set; }
    public int CustomerId { get; set; }
    public int StaffId { get; set; }
}
