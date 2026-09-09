namespace MovieRental.Domain.DTOs.Payments;

// Response DTO containing payment summary data returned in list endpoints
public class PaymentResponseDto
{
    // Unique ID of the payment
    public int PaymentId { get; set; }

    // Customer ID who paid
    public int CustomerId { get; set; }

    // Full name of the customer
    public string CustomerName { get; set; } = string.Empty;

    // Staff ID who processed the payment
    public int StaffId { get; set; }

    // Full name of the staff member
    public string StaffName { get; set; } = string.Empty;

    // Associated rental transaction ID
    public int RentalId { get; set; }

    // Title of the rented film
    public string FilmTitle { get; set; } = string.Empty;

    // Amount paid
    public decimal Amount { get; set; }

    // Timestamp when payment was completed
    public DateTime PaymentDate { get; set; }
}
