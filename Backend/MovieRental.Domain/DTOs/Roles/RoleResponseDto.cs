namespace MovieRental.Domain.DTOs.Roles;

// Response DTO containing role data returned in list endpoints
public class RoleResponseDto
{
    // Unique ID of the role
    public int RoleId { get; set; }

    // Name of the role
    public string RoleName { get; set; } = string.Empty;

    // Timestamp when the role was created
    public DateTime CreatedAt { get; set; }
}