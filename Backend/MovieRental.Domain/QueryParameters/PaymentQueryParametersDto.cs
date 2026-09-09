namespace MovieRental.Domain.QueryParameters;

public class PaymentQueryParametersDto : QueryParametersDto
{
    public int? CustomerId { get; set; }
    public int? StaffId { get; set; }
    public int? RentalId { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
