using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")] // Admin and Staff can access staff endpoints
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staffService;
        public StaffController(IStaffService staffService) => _staffService = staffService;

        // GET api/staff — returns paginated, filtered list of staff
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null)
            => Ok(await _staffService.GetAllStaffAsync(page, pageSize, search, isActive));

        // GET api/staff/{id} — returns single staff member by StaffId
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _staffService.GetStaffByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
