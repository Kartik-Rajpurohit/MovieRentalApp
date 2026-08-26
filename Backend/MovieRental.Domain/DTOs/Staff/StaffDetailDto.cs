namespace MovieRental.Domain.DTOs.Staff;

public class StaffDetailDto
{
    public int StaffId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; }
    public int StoreId { get; set; }

    // Address info from User → Address → City → Country
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? CityName { get; set; }
    public string? CountryName { get; set; }
}