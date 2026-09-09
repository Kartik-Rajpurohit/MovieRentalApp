using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for User accounts, authentication tokens, roles, and address cascade data.
    public interface IUserRepository
    {
        // Returns a queryable collection of users with role, customer, and staff links.
        IQueryable<User> GetAllUsers();

        // Finds a user by their ID with full role and profile data.
        Task<User?> GetUserByIdAsync(int id);

        // Checks whether an email is already registered in the users table.
        Task<bool> EmailExistsAsync(string email);

        // Adds a new user account to the database.
        Task<User> CreateUserAsync(User user);

        // Updates an existing user's information.
        Task<User?> UpdateUserAsync(User user);

        // Toggles a user's active status between true (active) and false (inactive).
        Task<User?> ToggleUserStatusAsync(int id);

        // Looks up the role name string using a roleId.
        Task<string?> GetRoleNameAsync(int roleId);

        // Creates a staff record linked to the given user account.
        Task CreateStaffAsync(int userId, int storeId);

        // Creates a customer record linked to the given user account.
        Task CreateCustomerAsync(int userId, int storeId);

        // Returns all countries for dropdown lists.
        IQueryable<Country> GetAllCountries();

        // Returns all cities in the specified country.
        IQueryable<City> GetCitiesByCountry(int countryId);

        // Returns all available system roles.
        IQueryable<Role> GetAllRoles();

        // Returns all physical stores for store assignment dropdowns.
        IQueryable<Store> GetAllStores();

        // Returns all addresses in the specified city.
        IQueryable<Address> GetAddressesByCity(int cityId);

        // Finds a user account by email address (used during login).
        Task<User?> GetUserByEmailAsync(string email);

        // Stores a generated refresh token and its expiry timestamp for a user.
        Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);

        // Finds a user matching an active, non-expired refresh token.
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);

        // Revokes and invalidates a refresh token so it cannot be used again.
        Task RevokeRefreshTokenAsync(string refreshToken, int? userId = null);

        // Revokes all active refresh tokens for a specific user ID upon logout.
        Task RevokeRefreshTokenByUserIdAsync(int userId);

        // Safely removes or unlinks a staff record when a user's role is changed.
        Task DeleteStaffByUserIdAsync(int userId);

        // Safely removes or unlinks a customer record when a user's role is changed.
        Task DeleteCustomerByUserIdAsync(int userId);

        // Adds a new address to the database and returns the generated AddressId.
        Task<int> CreateAddressAsync(Address address);
    }
}
