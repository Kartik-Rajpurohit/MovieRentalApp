using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles administrative user account management, role assignments, and status toggles.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Protected by Admin role by default; specific dropdown lookups allow anonymous
    public class UserController : ControllerBase
    {
        // Injected service handling user business logic and cascading lookups
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Gets a paginated, filtered, and sorted list of users.
        // Query parameters: page, pageSize, sortField, sortOrder, name, email, roleId, search, isActive.
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParametersDto queryParams)
        {
            var users = await _userService.GetAllUsersAsync(queryParams);
            return Ok(users);
        }

        // Gets a single user profile by UserId.
        // Returns 404 NotFound if user does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound($"User with id {id} not found");
            return Ok(user);
        }

        // Creates a new user with hashed password and role assignment.
        // Returns 409 Conflict if email is already registered in the system.
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

        // Partially updates an existing user's details (name, email, role, active status).
        // Safely handles foreign key unlinking if role changes between Customer and Staff.
        // Returns 404 NotFound if user does not exist.
        [HttpPatch]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
        {
            var updatedUser = await _userService.UpdateUserAsync(dto);
            if (updatedUser == null) return NotFound($"User with id {dto.UserId} not found");
            return Ok(updatedUser);
        }

        // Toggles a user's active status between active (true) and inactive (false).
        // Returns 404 NotFound if user does not exist.
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var result = await _userService.ToggleUserStatusAsync(id);
            if (result == null) return NotFound($"User with id {id} not found");
            return Ok(result);
        }

        // Public dropdown lookup: Gets paginated countries for address selection in signup/profile forms.
        [HttpGet("countries")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCountries([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var countries = await _userService.GetAllCountriesAsync(page, pageSize);
            return Ok(countries);
        }

        // Public dropdown lookup: Gets cities belonging to a selected country.
        [HttpGet("cities/{countryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCities(int countryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var cities = await _userService.GetCitiesByCountryAsync(countryId, page, pageSize);
            return Ok(cities);
        }

        // Admin lookup: Gets all available system roles for user assignment.
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var roles = await _userService.GetAllRolesAsync(page, pageSize);
            return Ok(roles);
        }

        // Admin and Staff lookup: Gets available store branches for user/staff assignment.
        [HttpGet("stores")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetStores([FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var stores = await _userService.GetAllStoresAsync(page, pageSize);
            return Ok(stores);
        }

        // Public dropdown lookup: Gets existing addresses for a selected city.
        [HttpGet("addresses/{cityId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAddressesByCity(int cityId, [FromQuery] int page = 1, [FromQuery] int pageSize = 100)
        {
            var addresses = await _userService.GetAddressesByCityAsync(cityId, page, pageSize);
            return Ok(addresses);
        }
    }
}
