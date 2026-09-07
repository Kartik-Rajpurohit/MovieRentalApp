using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Services.Interfaces;
using System.Security.Claims;

namespace MovieRental.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff,Customer")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    public DashboardController(IDashboardService dashboardService)
        => _dashboardService = dashboardService;

    // GET api/dashboard — role se decide karo kya return karna hai
    [HttpGet]
    public async Task<IActionResult> Get()
    {
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