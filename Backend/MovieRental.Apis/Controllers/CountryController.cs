using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Locations.Countries;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles country records for address configurations.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class CountryController : ControllerBase
    {
        private readonly ICountryService _countryService;

        public CountryController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? sortField = null,
            [FromQuery] string? sortOrder = null)
            => Ok(await _countryService.GetAllCountriesAsync(page, pageSize, search, sortField, sortOrder));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _countryService.GetCountryByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCountryDto dto)
            => Ok(await _countryService.CreateCountryAsync(dto));

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateCountryDto dto)
        {
            var result = await _countryService.UpdateCountryAsync(dto);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _countryService.DeleteCountryAsync(id);
            return result ? Ok() : NotFound();
        }
    }
}