namespace MovieRental.Domain.DTOs.Dashboard;

// Response DTO containing revenue metrics for a single store
public class StoreRevenueDto
{
    // Unique ID of the store
    public int StoreId { get; set; }

    // Store name or location descriptor
    public string StoreName { get; set; } = string.Empty;

    // Total revenue amount earned by this store
    public decimal Revenue { get; set; }
}