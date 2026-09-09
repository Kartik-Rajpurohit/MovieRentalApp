using MovieRental.Domain.DTOs.Dashboard;

namespace MovieRental.Services.Interfaces;

// Defines business operations for calculating and serving dashboard analytics.
public interface IDashboardService
{
    // Computes top-level system metrics and revenue statistics for administrators.
    Task<AdminDashboardDto> GetAdminDashboardAsync();

    // Computes store-level operational metrics and recent activity for staff members.
    Task<StaffDashboardDto> GetStaffDashboardAsync(int userId);

    // Computes personal rental stats, active rentals, and payment history for customers.
    Task<CustomerDashboardDto> GetCustomerDashboardAsync(int userId);
}