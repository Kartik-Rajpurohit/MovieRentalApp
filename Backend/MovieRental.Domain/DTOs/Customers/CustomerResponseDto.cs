namespace MovieRental.Domain.DTOs.Customers;

// Response DTO containing customer summary data returned in customer listings
public class CustomerResponseDto
{
    // Unique ID of the customer
    public int CustomerId { get; set; }

    // Customer's full name
    public string FullName { get; set; } = string.Empty;

    // Contact email address
    public string? Email { get; set; }

    // Store ID where the customer is registered
    public int StoreId { get; set; }

    // Account status (true = active, false = inactive)
    public bool IsActive { get; set; }

    // Registration date
    public DateOnly CreateDate { get; set; }
}