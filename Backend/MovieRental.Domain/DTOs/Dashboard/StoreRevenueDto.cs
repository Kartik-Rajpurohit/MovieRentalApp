namespace MovieRental.Domain.DTOs.Dashboard;

public class StoreRevenueDto
{
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
}