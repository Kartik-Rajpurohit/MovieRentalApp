using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MovieRental.Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff,Customer")] // All logged-in users can access dashboard
    public class DashboardController : ControllerBase
    {
        // GET api/dashboard — placeholder for dashboard stats
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { message = "Welcome to the dashboard" });
        }
    }
}
