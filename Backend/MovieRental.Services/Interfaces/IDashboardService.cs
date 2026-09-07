using MovieRental.Domain.DTOs.Dashboard;

namespace MovieRental.Services.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync();
    Task<StaffDashboardDto> GetStaffDashboardAsync(int userId);
    Task<CustomerDashboardDto> GetCustomerDashboardAsync(int userId);
}