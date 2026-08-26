using MovieRental.Domain.DTOs.Users;

namespace MovieRental.Domain.DTOs.Roles;

public class RoleDetailDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IEnumerable<UserResponseDto> Users { get; set; } = new List<UserResponseDto>();
}