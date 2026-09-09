namespace MovieRental.Domain.DTOs.Users
{
    // Response DTO containing user data returned in list and detail endpoints
    public class UserResponseDto
    {
        // Unique ID of the user
        public int UserId { get; set; }

        // First name of the user
        public string FirstName { get; set; } = string.Empty;

        // Last name of the user
        public string LastName { get; set; } = string.Empty;

        // Combined display name
        public string FullName => $"{FirstName} {LastName}".Trim();

        // Registered email address
        public string Email { get; set; } = string.Empty;

        // Account status (true = active, false = disabled)
        public bool IsActive { get; set; }

        // Assigned role ID
        public int? RoleId { get; set; }

        // Assigned role name (e.g. "Admin", "Staff", "Customer")
        public string RoleName { get; set; } = string.Empty;

        // Associated address ID
        public int? AddressId { get; set; }

        // Address postal code
        public string? PostalCode { get; set; }

        // Contact phone number
        public string? Phone { get; set; }

        // Street address
        public string? Street { get; set; }

        // City name
        public string? CityName { get; set; }

        // Country name
        public string? CountryName { get; set; }
    }
}