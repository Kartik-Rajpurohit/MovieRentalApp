namespace MovieRental.Domain.DTOs.Stores;

// Used in detail page — full store info
public class StoreDetailDto
{
    public int StoreId { get; set; }

    // Manager info
    public int ManagerStaffId { get; set; }
    public string ManagerName { get; set; } = string.Empty;

    // Address info
    public int AddressId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string? Street2 { get; set; }
    public string District { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string CountryName { get; set; } = string.Empty;

    // Stats
    public int TotalStaff { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalInventory { get; set; }

    public DateTime LastUpdate { get; set; }
}
