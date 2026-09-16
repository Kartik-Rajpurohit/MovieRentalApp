using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Staff;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles staff profile and store assignment queries.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Permissions.Staff.Read)] // Admin and Staff can view staff records
    public class StaffController : ControllerBase
    {
        // Injected service for staff lookups and store assignment info
        private readonly IStaffService _staffService;
        public StaffController(IStaffService staffService) => _staffService = staffService;

        // Gets a paginated, filtered, and sorted list of staff members.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationInputDto pagination,
            [FromQuery] StaffFilterDto filter)
            => Ok(await _staffService.GetAllStaffAsync(pagination, filter));

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
