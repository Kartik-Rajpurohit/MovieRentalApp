using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    // Contract for user business logic — implemented by UserService
    public interface IUserService
    {
        Task<PaginatedResponseDto<UserResponseDto>> GetAllUsersAsync(UserQueryParametersDto queryParams);
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<UserResponseDto> CreateUserAsync(CreateUserDto dto);
        Task<UserResponseDto?> UpdateUserAsync(UpdateUserDto dto);
        Task<UserResponseDto?> ToggleUserStatusAsync(int id);
        Task<IEnumerable<DropdownDto>> GetAllCountriesAsync(int page, int pageSize);
        Task<IEnumerable<DropdownDto>> GetCitiesByCountryAsync(int countryId, int page, int pageSize);
        Task<IEnumerable<DropdownDto>> GetAllRolesAsync(int page, int pageSize);
        Task<IEnumerable<DropdownDto>> GetAllStoresAsync(int page, int pageSize);
        Task<IEnumerable<DropdownDto>> GetAddressesByCityAsync(int cityId, int page, int pageSize);
    }
}