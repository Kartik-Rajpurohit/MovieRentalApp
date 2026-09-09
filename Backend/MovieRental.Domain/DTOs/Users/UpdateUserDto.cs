namespace MovieRental.Domain.DTOs.Users
{
    // Request DTO containing data sent by the client to update an existing user
    public class UpdateUserDto
    {
        // Unique ID of the user to update
        public int UserId { get; set; }       

        // Updated first name
        public string? FirstName { get; set; }

        // Updated last name
        public string? LastName { get; set; }

        // Updated role ID
        public int? RoleId { get; set; }

        // Updated address ID
        public int? AddressId { get; set; }

        // Updated store ID
        public int? StoreId { get; set; }
    }
}