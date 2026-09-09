namespace MovieRental.Domain.DTOs.Staff;

// Response DTO containing detailed information about a staff member including address
public class StaffDetailDto
{
    // Unique ID of the staff member
    public int StaffId { get; set; }

    // Full name of the staff member
    public string FullName { get; set; } = string.Empty;

    // Contact email address
    public string? Email { get; set; }

    // Whether the staff account is active
    public bool IsActive { get; set; }

    // Store ID where this staff member works
    public int StoreId { get; set; }

    // Address info from User → Address → City → Country
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? CityName { get; set; }
    public string? CountryName { get; set; }
}