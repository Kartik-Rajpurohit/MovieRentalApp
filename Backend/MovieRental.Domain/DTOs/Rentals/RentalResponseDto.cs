namespace MovieRental.Domain.DTOs.Rentals;

// Response DTO containing rental summary data returned in list endpoints
public class RentalResponseDto
{
    // Unique ID of the rental transaction
    public int RentalId { get; set; }

    // Date and time when the movie was rented
    public DateTime RentalDate { get; set; }

    // Date and time when the movie was returned (null if currently active)
    public DateTime? ReturnDate { get; set; }

    // Indicates whether the film has been returned
    public bool IsReturned => ReturnDate.HasValue;

    // Inventory copy ID
    public int InventoryId { get; set; }

    // Film ID
    public int FilmId { get; set; }

    // Film title
    public string FilmTitle { get; set; } = string.Empty;

    // Customer ID
    public int CustomerId { get; set; }

    // Customer full name
    public string CustomerName { get; set; } = string.Empty;

    // Staff ID
    public int StaffId { get; set; }

    // Staff full name
    public string StaffName { get; set; } = string.Empty;

    // Timestamp when the record was last modified
    public DateTime LastUpdate { get; set; }

    // Base rental rate for the movie
    public decimal RentalRate { get; set; }

    // Suggested payment amount based on rental rate and any late fees
    public decimal SuggestedAmount { get; set; }
}
