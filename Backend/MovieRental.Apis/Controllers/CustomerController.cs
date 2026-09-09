using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles customer profile and rental history queries for staff and administrators.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")] // Only Admin and Staff can view full customer listings
    public class CustomerController : ControllerBase
    {
        // Injected service for customer lookups and profile retrieval
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService) => _customerService = customerService;

        // Gets a paginated list of customers with search, active status, and store filters.
        // Query parameters: page, pageSize, search name, isActive, storeId.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? storeId = null)
            => Ok(await _customerService.GetAllCustomersAsync(page, pageSize, search, isActive, storeId));

        // Gets detailed customer profile by CustomerId including address and rental counts.
        // Returns 404 NotFound if customer does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
