using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to store locations.
    public class StoreRepository : IStoreRepository
    {
        // Receives the database context used to access store records.
        private readonly AppDbContext _context;

        public StoreRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all active stores without tracking, loading manager staff, address, city, and country details.
        public IQueryable<Store> GetAllStores()
        {
            return _context.Stores
                .AsNoTracking()
                .Include(s => s.ManagerStaff)
                    .ThenInclude(st => st!.User)
                .Include(s => s.Address)
                    .ThenInclude(a => a!.City)
                        .ThenInclude(c => c!.Country);
        }

        // Adds a new store location to the database and reloads its relationships.
        public async Task<Store> CreateStoreAsync(Store store)
        {
            store.LastUpdate = DateTime.UtcNow;
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();
            // Re-fetch with all includes so service can map to DTO — same pattern as CategoryRepository
            return await GetAllStores()
                .FirstAsync(s => s.StoreId == store.StoreId);
        }

        // Checks whether an active address exists by ID.
        public async Task<bool> AddressExistsAsync(int addressId)
            => await _context.Addresses.AnyAsync(a => a.AddressId == addressId);

        // Checks whether an active staff member exists by ID.
        public async Task<bool> StaffExistsAsync(int staffId)
            => await _context.Staff.AnyAsync(s => s.StaffId == staffId);
    }
}
