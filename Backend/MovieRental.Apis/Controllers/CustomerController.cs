using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Customers;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles customer profile and rental history queries for staff and administrators.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Permissions.Customers.Read)] // Only Admin and Staff can view full customer listings
    public class CustomerController : ControllerBase
    {
        // Injected service for customer lookups and profile retrieval
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService) => _customerService = customerService;

        // Gets a paginated, filtered, and sorted list of customers.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationInputDto pagination,
            [FromQuery] CustomerFilterDto filter)
            => Ok(await _customerService.GetAllCustomersAsync(pagination, filter));

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
