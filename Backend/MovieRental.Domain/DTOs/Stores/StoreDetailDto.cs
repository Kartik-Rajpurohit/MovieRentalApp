namespace MovieRental.Domain.DTOs.Stores;

// Response DTO containing detailed information about a store location
public class StoreDetailDto
{
    // Unique ID of the store
    public int StoreId { get; set; }

    // Manager staff ID
    public int ManagerStaffId { get; set; }

    // Full name of the store manager
    public string ManagerName { get; set; } = string.Empty;

    // Address ID
    public int AddressId { get; set; }

    // Street address
    public string Street { get; set; } = string.Empty;

    // Optional second street line
    public string? Street2 { get; set; }

    // District or region
    public string District { get; set; } = string.Empty;

    // Postal / zip code
    public string? PostalCode { get; set; }

    // Phone number
    public string Phone { get; set; } = string.Empty;

    // City name
    public string CityName { get; set; } = string.Empty;

    // Country name
    public string CountryName { get; set; } = string.Empty;

    // Total staff members working at this store
    public int TotalStaff { get; set; }

    // Total customers registered at this store
    public int TotalCustomers { get; set; }

    // Total film inventory copies available at this store
    public int TotalInventory { get; set; }

    // Timestamp when store was last modified
    public DateTime LastUpdate { get; set; }
}
