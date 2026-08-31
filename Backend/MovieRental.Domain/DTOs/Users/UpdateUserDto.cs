namespace MovieRental.Domain.DTOs.Users
{
    public class UpdateUserDto
    {
        public int UserId { get; set; }       
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? RoleId { get; set; }
        public int? AddressId { get; set; }
        public int? StoreId { get; set; }
    }
}