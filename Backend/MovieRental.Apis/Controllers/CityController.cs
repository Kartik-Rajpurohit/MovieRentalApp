using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.DTOs.Locations.Cities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

// Handles city records linked to countries.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")] // Only Admin and Staff can manage cities
public class CityController : ControllerBase
{
    // Injected service for city operations
    private readonly ICityService _cityService;
    // Injected service for retrieving addresses in a city
    private readonly IAddressService _addressService;

    public CityController(ICityService cityService, IAddressService addressService)
    {
        _cityService = cityService;
        _addressService = addressService;
    }

    // Gets a paginated list of cities with country information.
    // Query parameters: page, pageSize, search, countryId.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CityQueryParametersDto queryParams)
    {
        var result = await _cityService.GetAllCitiesAsync(queryParams);
        return Ok(result);
    }

    // Gets a single city by CityId.
    // Returns 404 NotFound if city does not exist.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _cityService.GetCityByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Gets all addresses located in this city.
    // Delegates to AddressService with CityId filter and pagination.
    [HttpGet("{id}/addresses")]
    public async Task<IActionResult> GetAddresses(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _addressService.GetAllAddressesAsync(new AddressQueryParametersDto
        {
            CityId = id,
            Page = page,
            PageSize = pageSize,
            Search = search
        });
        return Ok(result);
    }

    // Creates a new city under a country.
    // Restricted to Admin role.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCityDto dto)
    {
        var result = await _cityService.CreateCityAsync(dto);
        return Ok(result);
    }

    // Updates an existing city's name or country.
    // Restricted to Admin role.
    // Returns 404 NotFound if city does not exist.
    [HttpPatch]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateCityDto dto)
    {
        var result = await _cityService.UpdateCityAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Deletes a city by ID.
    // Restricted to Admin role.
    // Returns 204 NoContent on success, or 404 NotFound if city does not exist.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _cityService.DeleteCityAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
