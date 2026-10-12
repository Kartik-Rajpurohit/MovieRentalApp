using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for user management, role transitions, and status toggles.
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        // Receives the user repository needed to perform user queries and state modifications.
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Gets a paginated, filtered, and sorted list of users.
        public async Task<PaginatedResponseDto<UserResponseDto>> GetAllUsersAsync(
            PaginationInputDto pagination, UserFilterDto filter)
        {
            // 1. Base query from repository
            var query = _userRepository.GetAllUsers();

            // 2. Search: checks full name, email, role name, and user ID
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var lower = pagination.Search.ToLower();
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(lower) ||
                    u.Email.ToLower().Contains(lower) ||
                    (u.Role != null && u.Role.RoleName.ToLower().Contains(lower)) ||
                    u.UserId.ToString().Contains(lower));
            }

            // 3. Module Filters: role ID, name, email, and active status
            if (filter.RoleId.HasValue)
                query = query.Where(u => u.RoleId == filter.RoleId.Value);

            if (!string.IsNullOrEmpty(filter.Name))
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(filter.Name.ToLower()));

            if (!string.IsNullOrEmpty(filter.Email))
                query = query.Where(u => u.Email.ToLower().Contains(filter.Email.ToLower()));

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            // 4. Sorting: dynamic sorting on allowed fields with deterministic UserId tie-breaker
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "fullname" or "name" => isDesc
                    ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName).ThenByDescending(u => u.UserId)
                    : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ThenBy(u => u.UserId),
                "firstname" => isDesc
                    ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.UserId)
                    : query.OrderBy(u => u.FirstName).ThenBy(u => u.UserId),
                "lastname" => isDesc
                    ? query.OrderByDescending(u => u.LastName).ThenByDescending(u => u.UserId)
                    : query.OrderBy(u => u.LastName).ThenBy(u => u.UserId),
                "email" => isDesc
                    ? query.OrderByDescending(u => u.Email).ThenByDescending(u => u.UserId)
                    : query.OrderBy(u => u.Email).ThenBy(u => u.UserId),
                "role" => isDesc
                    ? query.OrderByDescending(u => u.Role != null ? u.Role.RoleName : "").ThenByDescending(u => u.UserId)
                    : query.OrderBy(u => u.Role != null ? u.Role.RoleName : "").ThenBy(u => u.UserId),
                "id" or "userid" => isDesc
                    ? query.OrderByDescending(u => u.UserId)
                    : query.OrderBy(u => u.UserId),
                "active" or "isactive" => isDesc
                    ? query.OrderByDescending(u => u.IsActive).ThenByDescending(u => u.UserId)
                    : query.OrderBy(u => u.IsActive).ThenBy(u => u.UserId),
                _ => isDesc
                    ? query.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName).ThenByDescending(u => u.UserId)
                    : query.OrderBy(u => u.FirstName).ThenBy(u => u.LastName).ThenBy(u => u.UserId)
            };

            // 5. Total count before pagination
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // 6. Pagination & Materialization
            var users = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var data = users.Select(u => u.ToResponseDto()).ToList();

            return new PaginatedResponseDto<UserResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                Data = data
            };
        }

        // Gets user details by ID.
        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return null;
            return user.ToResponseDto();
        }

        // Updates user profile and handles role transitions without foreign key constraint violations.
        public async Task<UserResponseDto?> UpdateUserAsync(UpdateUserDto dto)
        {
            // Fetch existing entity — service applies PATCH logic
            var user = await _userRepository.GetUserByIdAsync(dto.UserId);
            if (user == null) return null;

            // Fetch old role name before update — needed for role change logic
            var oldRoleName = user.RoleId.HasValue
                ? await _userRepository.GetRoleNameAsync(user.RoleId.Value)
                : null;

            // Only update fields that were actually sent — PATCH behaviour
            if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
            if (dto.AddressId.HasValue) user.AddressId = dto.AddressId.Value;

            // Role change logic
            if (dto.RoleId.HasValue && dto.RoleId.Value != user.RoleId)
            {
                var newRoleName = await _userRepository.GetRoleNameAsync(dto.RoleId.Value);

                // Delete old staff/customer record
                if (oldRoleName?.ToLower() == "staff")
                    await _userRepository.DeleteStaffByUserIdAsync(user.UserId);

                if (oldRoleName?.ToLower() == "customer")
                    await _userRepository.DeleteCustomerByUserIdAsync(user.UserId);

                // Create new staff/customer record
                if (newRoleName?.ToLower() == "staff" && dto.StoreId.HasValue)
                    await _userRepository.CreateStaffAsync(user.UserId, dto.StoreId.Value);

                if (newRoleName?.ToLower() == "customer" && dto.StoreId.HasValue)
                    await _userRepository.CreateCustomerAsync(user.UserId, dto.StoreId.Value);

                // Update role on user entity
                user.RoleId = dto.RoleId.Value;
            }

            // Repository handles save only
            var updated = await _userRepository.UpdateUserAsync(user);
            if (updated == null) return null;

            // Reload relations after update for accurate DTO mapping
            var reloaded = await _userRepository.GetUserByIdAsync(updated.UserId);
            return reloaded?.ToResponseDto();
        }

        // Toggles a user's active status between enabled and disabled.
        public async Task<UserResponseDto?> ToggleUserStatusAsync(int id)
        {
            var user = await _userRepository.ToggleUserStatusAsync(id);
            if (user == null) return null;
            return user.ToResponseDto();
        }
    }
}
