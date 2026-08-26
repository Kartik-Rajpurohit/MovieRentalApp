namespace MovieRental.Domain.DTOs.Payments;

public class PaymentResponseDto
{
    public int PaymentId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public int RentalId { get; set; }
    public string FilmTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
}
