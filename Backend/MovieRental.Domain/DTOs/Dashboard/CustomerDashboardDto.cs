using MovieRental.Domain.DTOs.Rentals;

namespace MovieRental.Domain.DTOs.Dashboard;

// Response DTO containing customer portal summary data
public class CustomerDashboardDto
{
    // Number of movies currently rented out by this customer
    public int ActiveRentals { get; set; }

    // Total number of all-time rentals made by this customer
    public int TotalRentals { get; set; }

    // Total amount spent on rentals and late fees
    public decimal TotalSpent { get; set; }

    // List of currently active rental records for this customer
    public List<RentalResponseDto> MyActiveRentals { get; set; } = new();
}