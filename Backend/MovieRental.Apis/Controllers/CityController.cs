using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.DTOs.Locations.Cities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class CityController : ControllerBase
{
    private readonly ICityService _cityService;
    private readonly IAddressService _addressService;

    public CityController(ICityService cityService, IAddressService addressService)
    {
        _cityService = cityService;
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CityQueryParametersDto queryParams)
    {
        var result = await _cityService.GetAllCitiesAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _cityService.GetCityByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Addresses for this city — delegates to AddressService with CityId filter
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCityDto dto)
    {
        var result = await _cityService.CreateCityAsync(dto);
        return Ok(result);
    }

    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] UpdateCityDto dto)
    {
        var result = await _cityService.UpdateCityAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _cityService.DeleteCityAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
