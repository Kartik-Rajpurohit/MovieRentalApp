using MovieRental.Domain.DTOs.Rentals;

namespace MovieRental.Domain.DTOs.Dashboard;

public class CustomerDashboardDto
{
    public int ActiveRentals { get; set; }
    public int TotalRentals { get; set; }
    public decimal TotalSpent { get; set; }
    public List<RentalResponseDto> MyActiveRentals { get; set; } = new();
}