namespace MovieRental.Domain.DTOs.Payments;

public class CreatePaymentDto
{
    public int CustomerId { get; set; }
    public int StaffId { get; set; }
    public int RentalId { get; set; }
    public decimal Amount { get; set; }
}
