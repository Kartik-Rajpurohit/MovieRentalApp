using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Staff;

namespace MovieRental.Services.Interfaces
{
    public interface IStaffService
    {
        Task<PaginatedResponseDto<StaffResponseDto>> GetAllStaffAsync(int page, int pageSize, string? search, bool? isActive, int? storeId = null);
        Task<StaffDetailDto?> GetStaffByIdAsync(int id);
    }
}