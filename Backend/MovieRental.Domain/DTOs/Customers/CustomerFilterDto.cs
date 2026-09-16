namespace MovieRental.Domain.DTOs.Customers
{
    // Module-specific filter parameters for customer listings
    public class CustomerFilterDto
    {
        // Filters customers by active status
        public bool? IsActive { get; set; }

        // Filters customers assigned to a primary store ID
        public int? StoreId { get; set; }
    }
}
