namespace MovieRental.Domain.DTOs.Roles;

public class RoleResponseDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}