using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
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

        // GET api/rental — paginated list
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] RentalQueryParametersDto queryParams)
        {
            var result = await _rentalService.GetAllRentalsAsync(queryParams);
            return Ok(result);
        }

        // GET api/rental/{id} — single rental detail
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _rentalService.GetRentalByIdAsync(id);
            if (result == null) return NotFound($"Rental {id} not found");
            return Ok(result);
        }

        // POST api/rental — create rental (Admin and Staff only)
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateRentalDto dto)
        {
            var result = await _rentalService.CreateRentalAsync(dto);
            return Ok(result);
        }

        // PATCH api/rental/{id}/return — mark as returned (Admin and Staff only)
        [HttpPatch("{id}/return")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Return(int id)
        {
            var result = await _rentalService.ReturnRentalAsync(id);
            if (result == null) return NotFound($"Rental {id} not found");
            return Ok(result);
        }
    }
}
