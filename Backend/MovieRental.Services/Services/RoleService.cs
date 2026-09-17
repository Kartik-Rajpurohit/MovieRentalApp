using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Roles;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for user roles and permission management.
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        // Receives the role repository needed to perform role queries and creation.
        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        // Retrieves a paginated and searchable list of system roles.
        public async Task<PaginatedResponseDto<RoleResponseDto>> GetAllRolesAsync(
            PaginationInputDto pagination,
            RoleFilterDto filter)
        {
            // Get the base query from the repository.
            var query = _roleRepository.GetAllRoles();

            // 1. Search - filter roles by name when search keyword is supplied.
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var lower = pagination.Search.Trim().ToLower();
                query = query.Where(r => r.RoleName.ToLower().Contains(lower));
            }

            // 2. Sorting
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "rolename" or "name" => isDesc
                    ? query.OrderByDescending(r => r.RoleName)
                    : query.OrderBy(r => r.RoleName),
                "createdat" => isDesc
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),
                "id" or "roleid" => isDesc
                    ? query.OrderByDescending(r => r.RoleId)
                    : query.OrderBy(r => r.RoleId),
                _ => isDesc
                    ? query.OrderByDescending(r => r.RoleId)
                    : query.OrderBy(r => r.RoleId)
            };

            // 3. Count
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // 4. Paginate and project role entities into response DTOs.
            var data = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(r => new RoleResponseDto
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    CreatedAt = r.CreatedAt,
                })
                .ToListAsync();

            return new PaginatedResponseDto<RoleResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                Data = data
            };
        }

        // Validates role uniqueness and saves a new system role.
        public async Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto dto)
        {
            // Business rule: Prevent creating duplicate roles with the same name.
            if (await _roleRepository.RoleExistsAsync(dto.RoleName))
                throw new InvalidOperationException("Role already exists");

            // Map request DTO to database entity with UTC creation timestamps.
            var role = new Role
            {
                RoleName = dto.RoleName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save the new role via the repository.
            var created = await _roleRepository.CreateRoleAsync(role);

            // Convert created entity to response DTO.
            return created.ToResponseDto();
        }
    }
}