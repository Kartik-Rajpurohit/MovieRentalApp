namespace MovieRental.Domain.DTOs.Users
{
    // Request DTO containing data sent by the client to create a new user
    public class CreateUserDto
    {
        // User's first name
        public string FirstName { get; set; } = string.Empty;

        // User's last name
        public string LastName { get; set; } = string.Empty;

        // User's login email address
        public string Email { get; set; } = string.Empty;

        // User's initial password in plain text (will be hashed)
        public string Password { get; set; } = string.Empty;

        // Role ID to assign (e.g. 1=Admin, 2=Staff, 3=Customer)
        public int? RoleId { get; set; }

        // Associated address ID
        public int? AddressId { get; set; }

        // Store ID if user is staff/manager
        public int? StoreId { get; set; }
    }
}