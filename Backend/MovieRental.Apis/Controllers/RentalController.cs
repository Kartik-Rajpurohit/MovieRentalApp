using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles rental bookings, queries, and return operations.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Permissions.Rentals.Read)] // Customers can view their own rentals; Admin and Staff can manage all
    public class RentalController : ControllerBase
    {
        // Injected service for rental operations and business rules
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        // Gets a paginated list of rentals with optional filters (returned status, overdue, date ranges).
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationInputDto pagination,
            [FromQuery] RentalFilterDto filter)
        {
            var result = await _rentalService.GetAllRentalsAsync(pagination, filter);
            return Ok(result);
        }

        // Gets rental details by RentalId including customer name, movie title, and payment status.
        // Returns 404 NotFound if rental does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _rentalService.GetRentalByIdAsync(id);
            if (result == null) return NotFound($"Rental {id} not found");
            return Ok(result);
        }

        // Creates a new rental booking for an available movie copy.
        // Restricted to Rentals.Create permission.
        // Returns 400 BadRequest if inventory copy is currently rented or customer is invalid.
        [HttpPost]
        [Authorize(Policy = Permissions.Rentals.Create)]
        public async Task<IActionResult> Create([FromBody] CreateRentalDto dto)
        {
            try
            {
                var result = await _rentalService.CreateRentalAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Marks a rental copy as returned and automatically restores inventory availability.
        // Restricted to Rentals.Return permission.
        // Returns 404 NotFound if rental does not exist, or 400 BadRequest if already returned.
        [HttpPatch("{id}/return")]
        [Authorize(Policy = Permissions.Rentals.Return)]
        public async Task<IActionResult> Return(int id)//TODO- Correc tthe naming convention of the method to be more descriptive, e.g., ReturnRental
        {
            try
            {
                var result = await _rentalService.ReturnRentalAsync(id);
                if (result == null) return NotFound($"Rental {id} not found");
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
