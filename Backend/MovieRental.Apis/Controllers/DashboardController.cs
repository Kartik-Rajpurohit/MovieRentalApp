using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Services.Interfaces;
using System.Security.Claims;

namespace MovieRental.Apis.Controllers;

// Handles role-based dashboard metric requests.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff,Customer")] // All roles have custom dashboard metrics
public class DashboardController : ControllerBase
{
    // Injected service for computing dashboard KPI metrics
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService)
        => _dashboardService = dashboardService;

    // Returns role-specific dashboard metrics based on the caller's JWT claims:
    // - Admin: System-wide revenue, total movies, active customers, overdue rentals.
    // - Staff: Assigned store metrics, processed rentals, pending returns.
    // - Customer: Active rentals, total payments, rental history summary.
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        // Extract role and user identifier from JWT claims
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdStr, out var id) ? id : 0;

        return role switch
        {
            "Admin" => Ok(await _dashboardService.GetAdminDashboardAsync()),
            "Staff" => Ok(await _dashboardService.GetStaffDashboardAsync(userId)),
            "Customer" => Ok(await _dashboardService.GetCustomerDashboardAsync(userId)),
            _ => Forbid()
        };
    }
}