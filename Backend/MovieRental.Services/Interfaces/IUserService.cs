using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for user accounts, status management, and dropdown options.
    public interface IUserService
    {
        // Retrieves a paginated and filtered list of user accounts.
        Task<PaginatedResponseDto<UserResponseDto>> GetAllUsersAsync(UserQueryParametersDto queryParams);

        // Retrieves a single user account by its ID.
        Task<UserResponseDto?> GetUserByIdAsync(int id);

        // Validates input, hashes the password, and creates a new user.
        Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);

        // Updates user profile information.
        Task<UserResponseDto?> UpdateUserAsync(UpdateUserDto dto);

        // Activates or deactivates a user account.
        Task<UserResponseDto?> ToggleUserStatusAsync(int id);

        // Retrieves countries formatted as dropdown options for user forms.
        Task<IEnumerable<DropdownDto>> GetAllCountriesAsync(int page, int pageSize);

        // Retrieves cities belonging to a country for cascading dropdowns.
        Task<IEnumerable<DropdownDto>> GetCitiesByCountryAsync(int countryId, int page, int pageSize);

        // Retrieves system roles formatted as dropdown options.
        Task<IEnumerable<DropdownDto>> GetAllRolesAsync(int page, int pageSize);

        // Retrieves stores formatted as dropdown options.
        Task<IEnumerable<DropdownDto>> GetAllStoresAsync(int page, int pageSize);

        // Retrieves addresses belonging to a city for cascading dropdowns.
        Task<IEnumerable<DropdownDto>> GetAddressesByCityAsync(int cityId, int page, int pageSize);
    }
}