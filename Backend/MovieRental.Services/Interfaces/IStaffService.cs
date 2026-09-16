using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Staff;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for staff management and store assignments.
    public interface IStaffService
    {
        // Retrieves a paginated, filtered, and sorted list of staff members.
        Task<PaginatedResponseDto<StaffResponseDto>> GetAllStaffAsync(PaginationInputDto pagination, StaffFilterDto filter);

        // Retrieves detailed staff information by ID, including assigned store and address.
        Task<StaffDetailDto?> GetStaffByIdAsync(int id);
    }
}