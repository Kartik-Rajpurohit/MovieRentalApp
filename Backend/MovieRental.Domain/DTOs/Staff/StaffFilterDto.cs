namespace MovieRental.Domain.DTOs.Staff
{
    // Module-specific filter parameters for staff listings
    public class StaffFilterDto
    {
        // Filters staff by active account status
        public bool? IsActive { get; set; }

        // Filters staff assigned to a specific store ID
        public int? StoreId { get; set; }
    }
}
