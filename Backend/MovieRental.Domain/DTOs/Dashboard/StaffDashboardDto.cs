using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.DTOs.Payments;

namespace MovieRental.Domain.DTOs.Dashboard;

// Response DTO containing store-level metrics and activity for the staff dashboard
public class StaffDashboardDto
{
    // The store ID where this staff member operates
    public int StoreId { get; set; }

    // Number of currently active (unreturned) rentals at this store
    public int ActiveRentals { get; set; }

    // Number of available film copies in this store's inventory
    public int AvailableInventory { get; set; }

    // Total registered customers at this store
    public int TotalCustomers { get; set; }

    // Total revenue collected today
    public decimal TodaysPayments { get; set; }

    // Recent rental transactions processed at this store
    public List<RentalResponseDto> RecentRentals { get; set; } = new();

    // Recent payments collected at this store
    public List<PaymentResponseDto> RecentPayments { get; set; } = new();
}