using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Roles;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
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
            int page, int pageSize, string? search)
        {
            // Get the base query from the repository.
            var query = _roleRepository.GetAllRoles();

            // Filter roles by name when search keyword is supplied.
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(r => r.RoleName.ToLower().Contains(lower));
            }

            var totalRecords = await query.CountAsync();

            // Paginate and project role entities into response DTOs.
            var data = await query
                .OrderBy(r => r.RoleId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
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
            return new RoleResponseDto
            {
                RoleId = created.RoleId,
                RoleName = created.RoleName,
                CreatedAt = created.CreatedAt
            };
        }
    }
}