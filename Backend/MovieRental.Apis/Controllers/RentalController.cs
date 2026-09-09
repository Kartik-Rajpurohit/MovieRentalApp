using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles rental bookings, queries, and return operations.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff,Customer")] // Customer can view their own rentals
    public class RentalController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        // Gets a paginated list of rentals with optional filters.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] RentalQueryParametersDto queryParams)
        {
            var result = await _rentalService.GetAllRentalsAsync(queryParams);
            return Ok(result);
        }

        // Gets rental details by rental ID.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _rentalService.GetRentalByIdAsync(id);
            if (result == null) return NotFound($"Rental {id} not found");
            return Ok(result);
        }

        // Creates a new rental for an available inventory copy (Admin and Staff only).
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
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

        // Marks a rental as returned and updates inventory availability (Admin and Staff only).
        [HttpPatch("{id}/return")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Return(int id)
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
