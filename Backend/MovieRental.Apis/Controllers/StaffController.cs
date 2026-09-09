using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles staff profile and store assignment queries.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")] // Admin and Staff can view staff records
    public class StaffController : ControllerBase
    {
        // Injected service for staff lookups and store assignment info
        private readonly IStaffService _staffService;
        public StaffController(IStaffService staffService) => _staffService = staffService;

        // Gets a paginated list of staff members with active status, store, and name search filters.
        // Query parameters: page, pageSize, search, isActive, storeId.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? storeId = null)
            => Ok(await _staffService.GetAllStaffAsync(page, pageSize, search, isActive, storeId));

        // Gets a single staff member's details by StaffId (including store and user info).
        // Returns 404 NotFound if staff member does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _staffService.GetStaffByIdAsync(id);
            return result is null ? NotFound() : Ok(result);
        }
    }
}
