using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]  // Only Admin and Staff can access inventory
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET api/inventory — paginated list with filters
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] InventoryQueryParametersDto queryParams)
        {
            var result = await _inventoryService.GetAllInventoryAsync(queryParams);
            return Ok(result);
        }

        // GET api/inventory/{id} — detail with rental count
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _inventoryService.GetInventoryByIdAsync(id);
            if (result == null) return NotFound($"Inventory with id {id} not found");
            return Ok(result);
        }

        // POST api/inventory — add a new copy of a film to a store
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateInventoryDto dto)
        {
            var result = await _inventoryService.CreateInventoryAsync(dto);
            return Ok(result);
        }

        // PATCH api/inventory — update store assignment
        [HttpPatch]
        public async Task<IActionResult> Update([FromBody] UpdateInventoryDto dto)
        {
            var result = await _inventoryService.UpdateInventoryAsync(dto);
            if (result == null) return NotFound($"Inventory with id {dto.InventoryId} not found");
            return Ok(result);
        }

        // DELETE api/inventory/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _inventoryService.DeleteInventoryAsync(id);
            if (!result) return NotFound($"Inventory with id {id} not found");
            return NoContent();
        }
    }
}
