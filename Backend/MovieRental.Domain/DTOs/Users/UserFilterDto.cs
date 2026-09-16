namespace MovieRental.Domain.DTOs.Users
{
    // Module-specific filter parameters for user account listings
    public class UserFilterDto
    {
        // Filters users by assigned role ID
        public int? RoleId { get; set; }

        // Filters users by name (first or last)
        public string? Name { get; set; }

        // Filters users by email address
        public string? Email { get; set; }

        // Filters users by active status
        public bool? IsActive { get; set; }
    }
}
