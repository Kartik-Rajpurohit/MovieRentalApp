using MovieRental.Domain.DTOs.Users;

namespace MovieRental.Domain.DTOs.Roles;

// Response DTO containing detailed information about a role, including all users assigned to it
public class RoleDetailDto
{
    // Unique ID of the role
    public int RoleId { get; set; }

    // Name of the role
    public string RoleName { get; set; } = string.Empty;

    // Timestamp when the role was created
    public DateTime CreatedAt { get; set; }

    // List of users assigned to this role
    public IEnumerable<UserResponseDto> Users { get; set; } = new List<UserResponseDto>();
}