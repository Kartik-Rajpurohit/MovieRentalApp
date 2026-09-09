namespace MovieRental.Domain.DTOs.Rentals;

// Response DTO containing detailed information about a rental transaction including payment summary
public class RentalDetailDto
{
    // Unique ID of the rental transaction
    public int RentalId { get; set; }

    // Date and time when the movie was rented
    public DateTime RentalDate { get; set; }

    // Date and time when the movie was returned (null if still rented out)
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

    // Total amount paid for this rental
    public decimal TotalPaid { get; set; }

    // Number of payment transactions recorded for this rental
    public int PaymentCount { get; set; }

    // Timestamp when the record was last modified
    public DateTime LastUpdate { get; set; }
}
