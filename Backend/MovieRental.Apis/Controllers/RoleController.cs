using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Roles;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Only Admin can access role endpoints
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // GET api/role?page=1&pageSize=10&search=admin
        [HttpGet]
        public async Task<IActionResult> GetAllRoles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var result = await _roleService.GetAllRolesAsync(page, pageSize, search);
            return Ok(result);
        }
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
