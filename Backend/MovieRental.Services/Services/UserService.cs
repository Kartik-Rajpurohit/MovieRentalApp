using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<PaginatedResponseDto<UserResponseDto>> GetAllUsersAsync(UserQueryParametersDto queryParams)
        {
            var query = _userRepository.GetAllUsers();

            // Filter by role ID if provided
            if (queryParams.RoleId.HasValue)
                query = query.Where(u => u.RoleId == queryParams.RoleId.Value);

            // Filter by name — matches against FirstName + LastName combined
            if (!string.IsNullOrEmpty(queryParams.Name))
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(queryParams.Name.ToLower()));

            // Filter by email
            if (!string.IsNullOrEmpty(queryParams.Email))
                query = query.Where(u => u.Email.ToLower().Contains(queryParams.Email.ToLower()));

            // Filter by active status
            if (queryParams.IsActive.HasValue)
                query = query.Where(u => u.IsActive == queryParams.IsActive.Value);

            // Global search — checks full name, email, role name, and user ID
            if (!string.IsNullOrEmpty(queryParams.Search))
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).ToLower().Contains(queryParams.Search.ToLower()) ||
                    u.Email.ToLower().Contains(queryParams.Search.ToLower()) ||
                    (u.Role != null && u.Role.RoleName.ToLower().Contains(queryParams.Search.ToLower())) ||
                    u.UserId.ToString().Contains(queryParams.Search));

            // Sorting — applied per field, supports asc/desc
            if (!string.IsNullOrEmpty(queryParams.SortField))
            {
                query = queryParams.SortField.ToLower() switch
                {
                    "email" => queryParams.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.Email)
                        : query.OrderBy(u => u.Email),

                    // Sort by role name, not role ID
                    "role" => queryParams.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.Role != null ? u.Role.RoleName : "")
                        : query.OrderBy(u => u.Role != null ? u.Role.RoleName : ""),

                    "id" => queryParams.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.UserId)
                        : query.OrderBy(u => u.UserId),

                    // Default — sort by first name
                    _ => queryParams.SortOrder?.ToLower() == "desc"
                        ? query.OrderByDescending(u => u.FirstName)
                        : query.OrderBy(u => u.FirstName)
                };
            }
            else
            {
                // No sort field provided — default to ascending UserId
                query = query.OrderBy(u => u.UserId);
            }

            // Total count before pagination — needed for frontend paginator
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

            // Fetch only the current page
            var data = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .Select(u => MapToDto(u))
                .ToListAsync();

            return new PaginatedResponseDto<UserResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = data
            };
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return null;
            return MapToDto(user);
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
        {
            // Business logic — reject duplicate email before hitting DB
            if (await _userRepository.EmailExistsAsync(dto.Email))
                throw new InvalidOperationException("Email already exists");

            // RoleId is required — validated here before building entity
            if (!dto.RoleId.HasValue)
                throw new InvalidOperationException("RoleId is required");

            // Build entity from DTO — service responsibility
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = dto.Password,
                RoleId = dto.RoleId.Value,
                AddressId = dto.AddressId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Repository handles insert only
            var created = await _userRepository.CreateUserAsync(user);

            // Business logic — auto-create staff/customer record based on role
            // RoleId.Value is safe here — already validated above
            var roleName = await _userRepository.GetRoleNameAsync(dto.RoleId.Value);

            return MapToDto(created);
        }

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
            return reloaded == null ? null : MapToDto(reloaded);
        }

        public async Task<UserResponseDto?> ToggleUserStatusAsync(int id)
        {
            var user = await _userRepository.ToggleUserStatusAsync(id);
            if (user == null) return null;
            return MapToDto(user);
        }

        public async Task<IEnumerable<DropdownDto>> GetAllCountriesAsync(int page, int pageSize)
        {
            return await _userRepository.GetAllCountries()
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new DropdownDto { Id = c.CountryId, Name = c.Name })
                .ToListAsync();
        }

        public async Task<IEnumerable<DropdownDto>> GetCitiesByCountryAsync(int countryId, int page, int pageSize)
        {
            return await _userRepository.GetCitiesByCountry(countryId)
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new DropdownDto { Id = c.CityId, Name = c.Name })
                .ToListAsync();
        }

        public async Task<IEnumerable<DropdownDto>> GetAllRolesAsync(int page, int pageSize)
        {
            return await _userRepository.GetAllRoles()
                .OrderBy(r => r.RoleName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new DropdownDto { Id = r.RoleId, Name = r.RoleName })
                .ToListAsync();
        }

        public async Task<IEnumerable<DropdownDto>> GetAllStoresAsync(int page, int pageSize)
        {
            return await _userRepository.GetAllStores()
                .OrderBy(s => s.StoreId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new DropdownDto { Id = s.StoreId, Name = $"Store {s.StoreId}" })
                .ToListAsync();
        }

        public async Task<IEnumerable<DropdownDto>> GetAddressesByCityAsync(int cityId, int page, int pageSize)
        {
            return await _userRepository.GetAddressesByCity(cityId)
                .OrderBy(a => a.Street)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new DropdownDto { Id = a.AddressId, Name = a.Street })
                .ToListAsync();
        }

        // Private helper — maps User entity to UserResponseDto
        private static UserResponseDto MapToDto(User u) => new UserResponseDto
        {
            UserId = u.UserId,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            IsActive = u.IsActive,
            RoleId = u.RoleId,
            RoleName = u.Role?.RoleName ?? "Unassigned",
            AddressId = u.AddressId ?? 0,
            Street = u.Address?.Street,
            CityName = u.Address?.City?.Name,
            CountryName = u.Address?.City?.Country?.Name,
        };
    }
}
