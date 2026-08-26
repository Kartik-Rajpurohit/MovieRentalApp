namespace MovieRental.Domain.DTOs.Stores;

// Used in paginated list view
public class StoreResponseDto
{
    public int StoreId { get; set; }

    // Manager info from Staff → User
    public int ManagerStaffId { get; set; }
    public string ManagerName { get; set; } = string.Empty;

    // Address info
    public string Street { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;

    // Stats
    public int TotalStaff { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalInventory { get; set; }
}
