using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Payments;

public class CreatePaymentDto
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid Customer ID is required.")]
    public int CustomerId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid Staff ID is required.")]
    public int StaffId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A valid Rental ID is required.")]
    public int RentalId { get; set; }

    [Range(0.01, 1000000.00, ErrorMessage = "Payment amount must be greater than zero.")]
    public decimal Amount { get; set; }
}
