namespace MovieRental.Domain.DTOs.Payments
{
    // Module-specific filter parameters for payment record listings
    public class PaymentFilterDto
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
}
