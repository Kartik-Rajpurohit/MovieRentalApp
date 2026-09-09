using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Rentals;

// Request DTO sent by the client to create a new movie rental transaction
public class CreateRentalDto
{
    // Inventory ID of the physical film copy being rented
    [Range(1, int.MaxValue, ErrorMessage = "A valid Inventory item ID is required.")]
    public int InventoryId { get; set; }

    // Customer ID renting the movie
    [Range(1, int.MaxValue, ErrorMessage = "A valid Customer ID is required.")]
    public int CustomerId { get; set; }

    // Staff ID processing the rental checkout
    [Range(1, int.MaxValue, ErrorMessage = "A valid Staff ID is required.")]
    public int StaffId { get; set; }
}
