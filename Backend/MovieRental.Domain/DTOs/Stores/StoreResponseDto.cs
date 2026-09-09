namespace MovieRental.Domain.DTOs.Stores;

// Response DTO containing store summary data returned in list endpoints
public class StoreResponseDto
{
    // Unique ID of the store
    public int StoreId { get; set; }

    // Manager staff ID
    public int ManagerStaffId { get; set; }

    // Full name of the store manager
    public string ManagerName { get; set; } = string.Empty;

    // Street address
    public string Street { get; set; } = string.Empty;

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

    // Total staff count
    public int TotalStaff { get; set; }

    // Total customer count
    public int TotalCustomers { get; set; }

    // Total inventory copy count
    public int TotalInventory { get; set; }
}
