namespace MovieRental.Domain.DTOs.Users
{
    public class UserResponseDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int AddressId { get; set; }
        public string? District { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? Street { get; set; }
        public string? CityName { get; set; }
        public string? CountryName { get; set; }
    }
}