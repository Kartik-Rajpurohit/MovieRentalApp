using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles inventory copy tracking, availability queries, and store assignments.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")] // Only Admin and Staff can manage physical copies
    public class InventoryController : ControllerBase
    {
        // Injected service for inventory business logic
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // Gets a paginated list of inventory copies with film title, store, and dynamic availability filters.
        // Query parameters: page, pageSize, search, storeId, isAvailable.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] InventoryQueryParametersDto queryParams)
        {
            var result = await _inventoryService.GetAllInventoryAsync(queryParams);
            return Ok(result);
        }

        // Gets single inventory copy details by InventoryId including its rental history count.
        // Returns 404 NotFound if inventory copy does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _inventoryService.GetInventoryByIdAsync(id);
            if (result == null) return NotFound($"Inventory with id {id} not found");
            return Ok(result);
        }

        // Adds a new physical copy of a film to a specific store.
        // Receives FilmId and StoreId in request body.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInventoryDto dto)
        {
            var result = await _inventoryService.CreateInventoryAsync(dto);
            return Ok(result);
        }

        // Updates an inventory item's store assignment.
        // Returns 404 NotFound if inventory copy does not exist.
        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] UpdateInventoryDto dto)
        {
            var result = await _inventoryService.UpdateInventoryAsync(dto);
            if (result == null) return NotFound($"Inventory with id {dto.InventoryId} not found");
            return Ok(result);
        }

        // Deletes an inventory copy by ID.
        // Returns 204 NoContent on success, or 404 NotFound if copy does not exist.
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _inventoryService.DeleteInventoryAsync(id);
            if (!result) return NotFound($"Inventory with id {id} not found");
            return NoContent();
        }
    }
}
