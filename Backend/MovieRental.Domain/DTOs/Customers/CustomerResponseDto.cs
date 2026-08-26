namespace MovieRental.Domain.DTOs.Customers;

public class CustomerResponseDto
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int StoreId { get; set; }
    public bool IsActive { get; set; }
    public DateOnly CreateDate { get; set; }
}