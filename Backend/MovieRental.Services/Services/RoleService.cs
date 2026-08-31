using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Roles;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<PaginatedResponseDto<RoleResponseDto>> GetAllRolesAsync(
            int page, int pageSize, string? search)
        {
            var query = _roleRepository.GetAllRoles();

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();
                query = query.Where(r => r.RoleName.ToLower().Contains(lower));
            }

            var totalRecords = await query.CountAsync();

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
        public async Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto dto)
        {
            // Business logic — duplicate role check
            if (await _roleRepository.RoleExistsAsync(dto.RoleName))
                throw new InvalidOperationException("Role already exists");

            var role = new Role
            {
                RoleName = dto.RoleName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _roleRepository.CreateRoleAsync(role);

            return new RoleResponseDto
            {
                RoleId = created.RoleId,
                RoleName = created.RoleName,
                CreatedAt = created.CreatedAt
            };
        }
    }
}