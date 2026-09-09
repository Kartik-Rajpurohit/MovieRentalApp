using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Users;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace MovieRental.Repository.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        // Returns IQueryable with all relations loaded â€” UserService applies filters on top
        public IQueryable<User> GetAllUsers()
        {
            return _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .Include(u => u.Address)
                    .ThenInclude(a => a!.City)
                        .ThenInclude(c => c!.Country);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Customer)
                .Include(u => u.Staff)
                .Include(u => u.Address)
                    .ThenInclude(a => a!.City)
                        .ThenInclude(c => c!.Country)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        // Check if email already exists â€” used by service before creating a user
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        // Insert user record â€” service passes fully built entity
        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Load relations so service can map to DTO
            await _context.Entry(user).Reference(u => u.Role).LoadAsync();
            await _context.Entry(user).Reference(u => u.Address).LoadAsync();
            if (user.Address != null)
            {
                await _context.Entry(user.Address).Reference(a => a.City).LoadAsync();
                if (user.Address.City != null)
                    await _context.Entry(user.Address.City).Reference(c => c.Country).LoadAsync();
            }

            return user;
        }

        // Update user record â€” service passes fully updated entity
        public async Task<User?> UpdateUserAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> ToggleUserStatusAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Address)
                    .ThenInclude(a => a!.City)
                        .ThenInclude(c => c!.Country)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return null;

            // Flip IsActive â€” true becomes false, false becomes true
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return user;
        }

        // Fetch role name â€” used by service to decide staff/customer creation
        public async Task<string?> GetRoleNameAsync(int roleId)
        {
            return await _context.Roles
                .Where(r => r.RoleId == roleId)
                .Select(r => r.RoleName)
                .FirstOrDefaultAsync();
        }

        // Insert staff record â€” called by service after user creation
        public async Task CreateStaffAsync(int userId, int storeId)
        {
            var staff = new Staff { UserId = userId, StoreId = storeId };
            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();
        }

        // Insert customer record â€” called by service after user creation
        public async Task CreateCustomerAsync(int userId, int storeId)
        {
            var customer = new Customer
            {
                UserId = userId,
                StoreId = storeId,
                CreateDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Active = 1,
            };
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        // Raw IQueryable â€” service applies pagination and maps to DropdownDto
        public IQueryable<Country> GetAllCountries()
            => _context.Countries.AsQueryable();

        public IQueryable<City> GetCitiesByCountry(int countryId)
            => _context.Cities.Where(c => c.CountryId == countryId);

        public IQueryable<Role> GetAllRoles()
            => _context.Roles.AsQueryable();

        public IQueryable<Store> GetAllStores()
            => _context.Stores.AsQueryable();

        public IQueryable<Address> GetAddressesByCity(int cityId)
            => _context.Addresses.Where(a => a.CityId == cityId);

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Customer)
                .Include(u => u.Staff)
                .Include(u => u.Address)
                    .ThenInclude(a => a!.City)
                        .ThenInclude(c => c!.Country)
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        private static string HashRefreshToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }

        public async Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;
            user.RefreshToken = HashRefreshToken(refreshToken);
            user.RefreshTokenExpiry = expiry;
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            var hashed = HashRefreshToken(refreshToken);
            return await _context.Users
                .Include(u => u.Role)
                .Include(u => u.Customer)
                .Include(u => u.Staff)
                .FirstOrDefaultAsync(u => u.RefreshToken == hashed);
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, int? userId = null)
        {
            var hashed = HashRefreshToken(refreshToken);
            var query = _context.Users
                .Where(u => u.RefreshToken == hashed);
            if (userId.HasValue)
            {
                query = query.Where(u => u.UserId == userId.Value);
            }
            var user = await query.FirstOrDefaultAsync();
            if (user == null) return;
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenByUserIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteStaffByUserIdAsync(int userId)
        {
            var staff = await _context.Staff
                .Include(s => s.Rentals)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (staff != null)
            {
                if (staff.Rentals.Any() || staff.Payments.Any())
                {
                    // Has historical transactions: unlink user account to preserve financial ledger
                    staff.UserId = null;
                }
                else
                {
                    _context.Staff.Remove(staff);
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteCustomerByUserIdAsync(int userId)
        {
            var customer = await _context.Customers
                .Include(c => c.Rentals)
                .Include(c => c.Payments)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (customer != null)
            {
                if (customer.Rentals.Any() || customer.Payments.Any())
                {
                    // Has historical transactions: unlink user account and deactivate to preserve ledger
                    customer.UserId = null;
                    customer.Active = 0;
                }
                else
                {
                    _context.Customers.Remove(customer);
                }
                await _context.SaveChangesAsync();
            }
        }

        // Insert new address record â€” returns AddressId for user assignment
        public async Task<int> CreateAddressAsync(Address address)
        {
            address.LastUpdate = DateTime.UtcNow;
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
            return address.AddressId;
        }
    }
}
