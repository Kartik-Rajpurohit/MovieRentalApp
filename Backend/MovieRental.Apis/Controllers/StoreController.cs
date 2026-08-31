using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Stores;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")] // Admin and Staff can access store endpoints
    public class StoreController : ControllerBase
    {
        private readonly IStoreService _storeService;

        public StoreController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        // GET api/store — paginated, filtered, sorted list of stores
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] StoreQueryParametersDto queryParams)
        {
            var result = await _storeService.GetAllStoresAsync(queryParams);
            return Ok(result);
        }

        // GET api/store/{id} — full store detail
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _storeService.GetStoreByIdAsync(id);
            return result is null ? NotFound($"Store {id} not found") : Ok(result);
        }
        // POST api/store — create new store
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStoreDto dto)
        {
            var result = await _storeService.CreateStoreAsync(dto);
            return Ok(result);
        }
    }
}
