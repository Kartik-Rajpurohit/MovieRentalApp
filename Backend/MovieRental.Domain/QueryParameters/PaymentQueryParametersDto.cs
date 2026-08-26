namespace MovieRental.Domain.QueryParameters;

public class PaymentQueryParametersDto : QueryParametersDto
{
    public int? CustomerId { get; set; }
    public int? StaffId { get; set; }
    public int? RentalId { get; set; }
}
