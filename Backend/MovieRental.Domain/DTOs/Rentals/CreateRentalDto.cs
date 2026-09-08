using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Rentals;

public class CreateRentalDto
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid Inventory item ID is required.")]
    public int InventoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid Customer ID is required.")]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid Staff ID is required.")]
    public int StaffId { get; set; }
}
