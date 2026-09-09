namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate payment records returned by the API
public class PaymentQueryParametersDto : QueryParametersDto
{
    // Filters payments made by a specific customer ID
    public int? CustomerId { get; set; }

    // Filters payments processed by a specific staff member ID
    public int? StaffId { get; set; }

    // Filters payments linked to a specific rental transaction ID
    public int? RentalId { get; set; }

    // Minimum payment amount filter
    public decimal? MinAmount { get; set; }

    // Maximum payment amount filter
    public decimal? MaxAmount { get; set; }

    // Start date filter for payment date range
    public DateTime? FromDate { get; set; }

    // End date filter for payment date range
    public DateTime? ToDate { get; set; }
}
