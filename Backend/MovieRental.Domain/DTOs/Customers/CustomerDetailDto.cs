namespace MovieRental.Domain.DTOs.Customers;

// Response DTO containing complete details of a customer, including address and location info
public class CustomerDetailDto
{
    // Unique ID of the customer
    public int CustomerId { get; set; }

    // Customer's full name
    public string FullName { get; set; } = string.Empty;

    // Contact email address
    public string? Email { get; set; }

    // Whether the customer account is active
    public bool IsActive { get; set; }

    // Home store ID where customer is registered
    public int StoreId { get; set; }

    // Date when the customer was originally registered
    public DateOnly CreateDate { get; set; }

    // Address info from User → Address → City → Country
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? PostalCode { get; set; }
    public string? Phone { get; set; }
    public string? CityName { get; set; }
    public string? CountryName { get; set; }
}