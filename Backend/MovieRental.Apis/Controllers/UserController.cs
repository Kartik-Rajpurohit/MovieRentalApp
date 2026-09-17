using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles administrative user account management, role assignments, and status toggles.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Permissions.Users.Read)] // Protected by Admin role by default; specific dropdown lookups allow anonymous
    public class UserController : ControllerBase
    {
        // Injected service handling user business logic and cascading lookups
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Gets a paginated, filtered, and sorted list of users.
        [HttpGet]
        public async Task<IActionResult> GetAllUsers(
            [FromQuery] PaginationInputDto pagination,
            [FromQuery] UserFilterDto filter)
        {
            var users = await _userService.GetAllUsersAsync(pagination, filter);
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

        // Partially updates an existing user's details (name, email, role, active status).
        // Safely handles foreign key unlinking if role changes between Customer and Staff.
        // Returns 404 NotFound if user does not exist.
        [HttpPatch]
        [Authorize(Policy = Permissions.Users.Update)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto dto)
        {
            var updatedUser = await _userService.UpdateUserAsync(dto);
            if (updatedUser == null) return NotFound($"User with id {dto.UserId} not found");
            return Ok(updatedUser);
        }

        // Toggles a user's active status between active (true) and inactive (false).
        // Returns 404 NotFound if user does not exist.
        [HttpPatch("{id}/toggle-status")]
        [Authorize(Policy = Permissions.Users.Update)]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var result = await _userService.ToggleUserStatusAsync(id);
            if (result == null) return NotFound($"User with id {id} not found");
            return Ok(result);
        }
    }
}
