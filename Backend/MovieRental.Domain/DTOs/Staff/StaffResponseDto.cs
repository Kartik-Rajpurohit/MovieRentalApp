namespace MovieRental.Domain.DTOs.Staff;

// Response DTO containing staff member summary data returned in list endpoints
public class StaffResponseDto
{
    // Unique ID of the staff member
    public int StaffId { get; set; }

    // Full name of the staff member
    public string FullName { get; set; } = string.Empty;

    // Contact email address
    public string? Email { get; set; }

    // Store ID where this staff member is assigned
    public int StoreId { get; set; }

    // Name of the store where the staff member works
    public string? StoreName { get; set; }

    // Whether the staff account is currently active
    public bool IsActive { get; set; }
}