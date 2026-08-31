using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class StoreRepository : IStoreRepository
    {
        private readonly AppDbContext _context;

        public StoreRepository(AppDbContext context)
        {
            _context = context;
        }

        // Used for both list and detail — counts computed via DB subquery in service
        public IQueryable<Store> GetAllStores()
        {
            return _context.Stores
                .Include(s => s.ManagerStaff)
                    .ThenInclude(st => st!.User)
                .Include(s => s.Address)
                    .ThenInclude(a => a!.City)
                        .ThenInclude(c => c!.Country);
        }

        public async Task<Store> CreateStoreAsync(Store store)
        {
            store.LastUpdate = DateTime.UtcNow;
            _context.Stores.Add(store);
            await _context.SaveChangesAsync();
            // Re-fetch with all includes so service can map to DTO — same pattern as CategoryRepository
            return await GetAllStores()
                .FirstAsync(s => s.StoreId == store.StoreId);
        }
    }
}
