using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Roles;

namespace MovieRental.Services.Interfaces
{
    public interface IRoleService
    {
        Task<PaginatedResponseDto<RoleResponseDto>> GetAllRolesAsync(int page, int pageSize, string? search);
    }
}