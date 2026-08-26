namespace MovieRental.Domain.DTOs.Staff;

public class StaffResponseDto
{
    public int StaffId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public int StoreId { get; set; }
    public string? StoreName { get; set; }
    public bool IsActive { get; set; }
}