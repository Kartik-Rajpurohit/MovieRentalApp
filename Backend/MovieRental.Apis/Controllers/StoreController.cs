using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Stores;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles physical store locations, managers, and store-level metrics.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")] // Admin and Staff can view stores; only Admin can create
    public class StoreController : ControllerBase
    {
        // Injected service for store operations and metrics
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        // Gets a paginated list of stores with manager, address, and copy count summaries.
        // Query parameters: page, pageSize, search.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] StoreQueryParametersDto queryParams)
        {
            var result = await _storeService.GetAllStoresAsync(queryParams);
            return Ok(result);
        }

        // Gets complete store details by StoreId (address, manager, inventory count, customer count).
        // Returns 404 NotFound if store does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _storeService.GetStoreByIdAsync(id);
            return result is null ? NotFound($"Store {id} not found") : Ok(result);
        }

        // Creates a new store branch with manager and address.
        // Restricted to Admin role only.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateStoreDto dto)
        {
            var result = await _storeService.CreateStoreAsync(dto);
            return Ok(result);
        }
    }
}
