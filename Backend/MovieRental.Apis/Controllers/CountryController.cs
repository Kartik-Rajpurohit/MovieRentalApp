using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Locations.Countries;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles country records for address configurations.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")] // Only Admin and Staff can manage countries
    public class CountryController : ControllerBase
    {
        // Injected service for country database queries
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        // Gets a paginated and sorted list of countries.
        // Query parameters: page, pageSize, search, sortField, and sortOrder.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortField = null,
            [FromQuery] string? sortOrder = null)
            => Ok(await _countryService.GetAllCountriesAsync(page, pageSize, search, sortField, sortOrder));

        // Gets a country by CountryId with its associated cities.
        // Returns 404 NotFound if country does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _countryService.GetCountryByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        // Adds a new country name.
        // Restricted to Admin role only.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCountryDto dto)
            => Ok(await _countryService.CreateCountryAsync(dto));

        // Updates an existing country's name.
        // Restricted to Admin role only.
        // Returns 404 NotFound if country does not exist.
        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateCountryDto dto)
        {
            var result = await _countryService.UpdateCountryAsync(dto);
            return result is null ? NotFound() : Ok(result);
        }

        // Deletes a country by ID.
        // Restricted to Admin role only.
        // Returns 200 OK on success, or 404 NotFound if country does not exist.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _countryService.DeleteCountryAsync(id);
            return result ? Ok() : NotFound();
        }
    }
}