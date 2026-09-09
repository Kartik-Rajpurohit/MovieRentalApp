using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Roles;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles role queries and role creation for administrators.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only Admin can manage system roles
    public class RoleController : ControllerBase
    {
        // Injected service for role management
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // Gets a paginated list of system roles (Admin, Staff, Customer).
        // Query parameters: page, pageSize, search name.
        [HttpGet]
        public async Task<IActionResult> GetAllRoles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var result = await _roleService.GetAllRolesAsync(page, pageSize, search);
            return Ok(result);
        }

        // Creates a new user role in the system.
        // Returns 409 Conflict if a role with the same name already exists.
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                var result = await _roleService.CreateRoleAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}
