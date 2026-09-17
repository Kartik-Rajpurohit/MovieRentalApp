using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.DTOs.Locations.Cities;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

// Handles city records linked to countries.
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Cities.Read)] // Only Admin and Staff can manage cities
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
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationInputDto pagination,
        [FromQuery] CityFilterDto filter)
    {
        var result = await _cityService.GetAllCitiesAsync(pagination, filter);
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
        [FromQuery] PaginationInputDto pagination)
    {
        var result = await _addressService.GetAllAddressesAsync(
            pagination,
            new AddressFilterDto { CityId = id });
        return Ok(result);
    }

    // Creates a new city under a country.
    // Restricted to Admin role.
    [HttpPost]
    [Authorize(Policy = Permissions.Cities.Create)]
    public async Task<IActionResult> Create([FromBody] CreateCityDto dto)
    {
        var result = await _cityService.CreateCityAsync(dto);
        return Ok(result);
    }
}
