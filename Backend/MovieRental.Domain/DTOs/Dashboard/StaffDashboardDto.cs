using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.DTOs.Payments;

namespace MovieRental.Domain.DTOs.Dashboard;

public class StaffDashboardDto
{
    public int StoreId { get; set; }
    public int ActiveRentals { get; set; }
    public int AvailableInventory { get; set; }
    public int TotalCustomers { get; set; }
    public decimal TodaysPayments { get; set; }
    public List<RentalResponseDto> RecentRentals { get; set; } = new();
    public List<PaymentResponseDto> RecentPayments { get; set; } = new();
}