using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

// Handles address creation, retrieval, and updates for stores, staff, and customers.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    // Gets a paginated list of addresses with city and postal code filters.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AddressQueryParametersDto queryParams)
    {
        var result = await _addressService.GetAllAddressesAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _addressService.GetAddressByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
    {
        var result = await _addressService.CreateAddressAsync(dto);
        return Ok(result);
    }

    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] UpdateAddressDto dto)
    {
        var result = await _addressService.UpdateAddressAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _addressService.DeleteAddressAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}