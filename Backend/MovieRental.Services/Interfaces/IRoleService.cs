using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Roles;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for managing user roles and system permissions.
    public interface IRoleService
    {
        // Retrieves a paginated and searchable list of system roles.
        Task<PaginatedResponseDto<RoleResponseDto>> GetAllRolesAsync(int page, int pageSize, string? search);

        // Creates a new role in the system.
        Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto dto);
    }
}