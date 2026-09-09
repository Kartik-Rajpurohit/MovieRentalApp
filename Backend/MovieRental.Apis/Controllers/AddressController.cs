using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

// Handles address creation, retrieval, and updates for stores, staff, and customers.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff")] // Only Admin and Staff can manage addresses
public class AddressController : ControllerBase
{
    // Injected service handling address database operations
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    // Gets a paginated list of addresses with city and postal code filters.
    // Query parameters: page, pageSize, search, cityId.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AddressQueryParametersDto queryParams)
    {
        var result = await _addressService.GetAllAddressesAsync(queryParams);
        return Ok(result);
    }

    // Gets a single address by AddressId.
    // Returns 404 NotFound if no address matches the ID.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _addressService.GetAddressByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Creates a new address record.
    // Receives street address, cityId, postalCode, and phone in request body.
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
    {
        var result = await _addressService.CreateAddressAsync(dto);
        return Ok(result);
    }

    // Updates an existing address.
    // Returns 404 NotFound if the address does not exist.
    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] UpdateAddressDto dto)
    {
        var result = await _addressService.UpdateAddressAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Deletes an address by ID.
    // Restricted to Admin role only.
    // Returns 204 NoContent on success, or 404 NotFound if address not found.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _addressService.DeleteAddressAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}