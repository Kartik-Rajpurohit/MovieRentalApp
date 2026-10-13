namespace MovieRental.Domain.DTOs.Rentals;

// Request DTO sent by the client to return a rental with an optional custom return date
public class ReturnRentalDto
{
    // Optional return timestamp (defaults to UtcNow if not provided)
    public DateTime? ReturnDate { get; set; }
}
