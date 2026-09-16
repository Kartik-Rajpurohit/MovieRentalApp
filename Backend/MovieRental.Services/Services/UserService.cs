using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
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

            // 4. Sorting: dynamic sorting on allowed fields
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "email" => isDesc
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "role" => isDesc
                    ? query.OrderByDescending(u => u.Role != null ? u.Role.RoleName : "")
                    : query.OrderBy(u => u.Role != null ? u.Role.RoleName : ""),
                "id" or "userid" => isDesc
                    ? query.OrderByDescending(u => u.UserId)
                    : query.OrderBy(u => u.UserId),
                "active" or "isactive" => isDesc
                    ? query.OrderByDescending(u => u.IsActive)
                    : query.OrderBy(u => u.IsActive),
                "lastname" => isDesc
                    ? query.OrderByDescending(u => u.LastName)
                    : query.OrderBy(u => u.LastName),
                _ => isDesc
                    ? query.OrderByDescending(u => u.FirstName)
                    : query.OrderBy(u => u.FirstName)
            };

            // 5. Total count before pagination
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // 6. Pagination & Materialization
            var users = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            var data = users.Select(MapToDto).ToList();

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
            return MapToDto(user);
        }

        // Creates a new user with hashed password and associated staff/customer profile.
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
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

            if (roleName?.Equals("Staff", StringComparison.OrdinalIgnoreCase) == true && dto.StoreId.HasValue)
                await _userRepository.CreateStaffAsync(created.UserId, dto.StoreId.Value);
            else if (roleName?.Equals("Customer", StringComparison.OrdinalIgnoreCase) == true && dto.StoreId.HasValue)
                await _userRepository.CreateCustomerAsync(created.UserId, dto.StoreId.Value);

            var fullUser = await _userRepository.GetUserByIdAsync(created.UserId);
            return MapToDto(fullUser ?? created);
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
            return reloaded == null ? null : MapToDto(reloaded);
        }

        // Toggles a user's active status between enabled and disabled.
        public async Task<UserResponseDto?> ToggleUserStatusAsync(int id)
        {
            var user = await _userRepository.ToggleUserStatusAsync(id);
            if (user == null) return null;
            return MapToDto(user);
        }

        // Retrieves countries formatted as dropdown options for user registration and address forms.
        public async Task<IEnumerable<DropdownDto>> GetAllCountriesAsync(int page, int pageSize)
        {
            return await _userRepository.GetAllCountries()
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new DropdownDto { Id = c.CountryId, Name = c.Name })
                .ToListAsync();
        }

        // Retrieves cities belonging to a selected country for cascading dropdowns.
        public async Task<IEnumerable<DropdownDto>> GetCitiesByCountryAsync(int countryId, int page, int pageSize)
        {
            return await _userRepository.GetCitiesByCountry(countryId)
                .OrderBy(c => c.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new DropdownDto { Id = c.CityId, Name = c.Name })
                .ToListAsync();
        }

        // Retrieves all user roles formatted as dropdown options.
        public async Task<IEnumerable<DropdownDto>> GetAllRolesAsync(int page, int pageSize)
        {
            return await _userRepository.GetAllRoles()
                .OrderBy(r => r.RoleName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new DropdownDto { Id = r.RoleId, Name = r.RoleName })
                .ToListAsync();
        }

        // Retrieves all stores formatted as dropdown options.
        public async Task<IEnumerable<DropdownDto>> GetAllStoresAsync(int page, int pageSize)
        {
            return await _userRepository.GetAllStores()
                .OrderBy(s => s.StoreId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new DropdownDto { Id = s.StoreId, Name = $"Store {s.StoreId}" })
                .ToListAsync();
        }

        // Retrieves addresses within a city for cascading address dropdowns.
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
            AddressId = u.AddressId,
            Street = u.Address?.Street,   
            PostalCode = u.Address?.PostalCode,
            Phone = u.Address?.Phone,
            CityName = u.Address?.City?.Name,
            CountryName = u.Address?.City?.Country?.Name,
        };
    }
}
