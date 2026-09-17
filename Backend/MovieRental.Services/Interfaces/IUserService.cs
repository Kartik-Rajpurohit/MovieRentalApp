using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Users;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for user accounts, status management, and dropdown options.
    public interface IUserService
    {
        // Retrieves a paginated, filtered, and sorted list of user accounts.
        Task<PaginatedResponseDto<UserResponseDto>> GetAllUsersAsync(PaginationInputDto pagination, UserFilterDto filter);

        // Retrieves a single user account by its ID.
        Task<UserResponseDto?> GetUserByIdAsync(int id);

        // Updates user profile information.
        Task<UserResponseDto?> UpdateUserAsync(UpdateUserDto dto);

        // Activates or deactivates a user account.
        Task<UserResponseDto?> ToggleUserStatusAsync(int id);
    }
}