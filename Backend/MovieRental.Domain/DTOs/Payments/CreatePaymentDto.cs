using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Payments;

// Request DTO containing data sent by the client to record a new payment
public class CreatePaymentDto
{
    // Customer ID making the payment
    [Range(1, int.MaxValue, ErrorMessage = "A valid Customer ID is required.")]
    public int CustomerId { get; set; }

    // Staff member ID receiving or processing the payment
    [Range(1, int.MaxValue, ErrorMessage = "A valid Staff ID is required.")]
    public int StaffId { get; set; }

    // Rental ID this payment is linked to
    [Range(1, int.MaxValue, ErrorMessage = "A valid Rental ID is required.")]
    public int RentalId { get; set; }

    // Payment amount (must be positive)
    [Range(0.01, 1000000.00, ErrorMessage = "Payment amount must be greater than zero.")]
    public decimal Amount { get; set; }
}
