using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.DTOs.Payments;

namespace MovieRental.Domain.DTOs.Dashboard;

// Response DTO containing system-wide statistics and summaries for the administrator dashboard
public class AdminDashboardDto
{
    // Row 1 — Key Numbers
    public int TotalUsers { get; set; }
    public int TotalFilms { get; set; }
    public int TotalRentals { get; set; }
    public decimal TotalRevenue { get; set; }

    // Row 2 — Current Status (active rentals, stock, customer count)
    public int ActiveRentals { get; set; }
    public int AvailableInventory { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalStaff { get; set; }

    // Row 3 — Lists (top rented movies, revenue per store, latest rental transactions)
    public List<TopFilmDto> TopRentedFilms { get; set; } = new();
    public List<StoreRevenueDto> RevenueByStore { get; set; } = new();
    public List<RentalResponseDto> RecentRentals { get; set; } = new();
}