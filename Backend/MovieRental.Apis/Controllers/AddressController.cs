using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

// Handles address creation, retrieval, and updates for stores, staff, and customers.
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Addresses.Read)] // Only Admin and Staff can manage addresses
public class AddressController : ControllerBase
{
    // Injected service handling address database operations
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    // Gets a paginated list of addresses with city and postal code filters.
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationInputDto pagination,
        [FromQuery] AddressFilterDto filter)
    {
        var result = await _addressService.GetAllAddressesAsync(pagination, filter);
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
    [Authorize(Policy = Permissions.Addresses.Create)]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
    {
        var result = await _addressService.CreateAddressAsync(dto);
        return Ok(result);
    }
}