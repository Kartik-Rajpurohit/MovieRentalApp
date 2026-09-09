namespace MovieRental.Domain.DTOs.Rentals;

// Request DTO sent by the client to record the return of a rented film
public class ReturnRentalDto
{
    // Unique ID of the rental transaction being closed/returned
    public int RentalId { get; set; }
}
