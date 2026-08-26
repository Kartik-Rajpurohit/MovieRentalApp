namespace MovieRental.Domain.DTOs.Users
{
    public class CreateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public int? AddressId { get; set; }
        public int? StoreId { get; set; }
    }
}