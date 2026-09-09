namespace MovieRental.Domain.DTOs.Roles;

// Request DTO sent by the client to create a new user role
public class CreateRoleDto
{
    // Name of the new role (e.g. "Admin", "Staff", "Customer")
    public string RoleName { get; set; } = string.Empty;
}