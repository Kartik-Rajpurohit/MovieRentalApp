using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Rentals;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class RentalController : ControllerBase
    {
        private readonly IRentalService _rentalService;

        public RentalController(IRentalService rentalService)
        {
            _rentalService = rentalService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] RentalQueryParametersDto queryParams)
        {
            var result = await _rentalService.GetAllRentalsAsync(queryParams);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _rentalService.GetRentalByIdAsync(id);
            if (result == null) return NotFound($"Rental {id} not found");
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRentalDto dto)
        {
            var result = await _rentalService.CreateRentalAsync(dto);
            return Ok(result);
        }

        [HttpPatch("{id}/return")]
        public async Task<IActionResult> Return(int id)
        {
            var result = await _rentalService.ReturnRentalAsync(id);
            if (result == null) return NotFound($"Rental {id} not found");
            return Ok(result);
        }
    }
}
