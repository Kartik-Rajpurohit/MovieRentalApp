using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only Admin can access user endpoints
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET api/user — returns paginated, filtered, sorted list of users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParametersDto queryParams)
        {
            var users = await _userService.GetAllUsersAsync(queryParams);
            return Ok(users);
        }

        // GET api/user/{id} — returns single user by UserId
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound($"User with id {id} not found");
            return Ok(user);
        }

        // POST api/user — creates a new user, returns 409 if email already exists
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            try
            {
                var createdUser = await _userService.CreateUserAsync(dto);
                return Ok(createdUser);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // PATCH api/user — partial update, only sent fields are updated
        [HttpPatch]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
        {
            var updatedUser = await _userService.UpdateUserAsync(dto);
            if (updatedUser == null) return NotFound($"User with id {dto.UserId} not found");
            return Ok(updatedUser);
        }

        // PATCH api/user/{id}/toggle-status — flips IsActive between true and false
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var result = await _userService.ToggleUserStatusAsync(id);
            if (result == null) return NotFound($"User with id {id} not found");
            return Ok(result);
        }

        // GET api/user/countries — public endpoint for dropdown in forms
        [HttpGet("countries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountries([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var countries = await _userService.GetAllCountriesAsync(page, pageSize);
            return Ok(countries);
        }

        // GET api/user/cities/{countryId} — public endpoint for dependent city dropdown
        [HttpGet("cities/{countryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCities(int countryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var cities = await _userService.GetCitiesByCountryAsync(countryId, page, pageSize);
            return Ok(cities);
        }

        // GET api/user/roles — public endpoint for role dropdown
        [HttpGet("roles")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var roles = await _userService.GetAllRolesAsync(page, pageSize);
            return Ok(roles);
        }

        // GET api/user/stores — public endpoint for store dropdown
        [HttpGet("stores")]
        [AllowAnonymous]
        public async Task<IActionResult> GetStores([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var stores = await _userService.GetAllStoresAsync(page, pageSize);
            return Ok(stores);
        }

        // GET api/user/addresses/{cityId} — public endpoint for address dropdown
        [HttpGet("addresses/{cityId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAddressesByCity(int cityId, [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var addresses = await _userService.GetAddressesByCityAsync(cityId, page, pageSize);
            return Ok(addresses);
        }
    }
}
