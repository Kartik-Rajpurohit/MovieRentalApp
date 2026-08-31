using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Repository contract — raw DB operations only
    public interface IUserRepository
    {
        IQueryable<User> GetAllUsers();
        Task<User?> GetUserByIdAsync(int id);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<User?> UpdateUserAsync(User user);
        Task<User?> ToggleUserStatusAsync(int id);
        Task<string?> GetRoleNameAsync(int roleId);
        Task CreateStaffAsync(int userId, int storeId);
        Task CreateCustomerAsync(int userId, int storeId);
        IQueryable<Country> GetAllCountries();
        IQueryable<City> GetCitiesByCountry(int countryId);
        IQueryable<Role> GetAllRoles();
        IQueryable<Store> GetAllStores();
        IQueryable<Address> GetAddressesByCity(int cityId);
        Task<User?> GetUserByEmailAsync(string email);
        Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
        Task<User?> GetUserByRefreshTokenAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
        Task DeleteStaffByUserIdAsync(int userId);
        Task DeleteCustomerByUserIdAsync(int userId);
        Task<int> CreateAddressAsync(Address address); // Creates new address, returns AddressId
    }
}
