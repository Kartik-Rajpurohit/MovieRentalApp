using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Staff;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for staff management and store assignments.
    public interface IStaffService
    {
        // Retrieves a paginated and filtered list of staff members.
        Task<PaginatedResponseDto<StaffResponseDto>> GetAllStaffAsync(int page, int pageSize, string? search, bool? isActive, int? storeId = null);

        // Retrieves detailed staff information by ID, including assigned store and address.
        Task<StaffDetailDto?> GetStaffByIdAsync(int id);
    }
}