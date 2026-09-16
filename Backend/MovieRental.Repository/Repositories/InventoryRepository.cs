using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to inventory copies of movies.
    public class InventoryRepository : IInventoryRepository
    {
        // Receives the database context used to access inventory data.
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all active inventory copies without tracking, loading related Movie and Rentals for availability checks.
        public IQueryable<Inventory> GetAllInventory()
        {
            return _context.Inventories
                .AsNoTracking()
                .Where(i => !i.IsDeleted && !i.Movie.IsDeleted)
                .Include(i => i.Movie)
                .Include(i => i.Rentals);
        }

        // Finds a specific active inventory copy by ID along with its movie and rental records.
        public async Task<Inventory?> GetInventoryByIdAsync(int id)
        {
            return await _context.Inventories
                .Where(i => !i.IsDeleted)
                .Include(i => i.Movie)
                .Include(i => i.Rentals)
                .FirstOrDefaultAsync(i => i.InventoryId == id);
        }

        // Adds a new inventory item to the database and explicitly loads its movie and rentals.
        public async Task<Inventory> CreateInventoryAsync(Inventory inventory)
        {
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Reload with relations for service mapping
            await _context.Entry(inventory).Reference(i => i.Movie).LoadAsync();
            await _context.Entry(inventory).Collection(i => i.Rentals).LoadAsync();

            return inventory;
        }

        // Updates an inventory item's store assignment and last update timestamp.
        public async Task<Inventory?> UpdateInventoryAsync(Inventory inventory)
        {
            var existing = await _context.Inventories
                .Include(i => i.Movie)
                .Include(i => i.Rentals)
                .FirstOrDefaultAsync(i => i.InventoryId == inventory.InventoryId && !i.IsDeleted);

            if (existing == null) return null;

            existing.StoreId = inventory.StoreId;
            if (inventory.MovieId != 0) existing.MovieId = inventory.MovieId;
            existing.LastUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        // Soft-deletes an inventory copy record by setting IsDeleted = true.
        public async Task<bool> DeleteInventoryAsync(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null || inventory.IsDeleted) return false;

            inventory.IsDeleted = true;
            inventory.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        // Checks whether an active movie exists by ID.
        public async Task<bool> MovieExistsAsync(int movieId)
            => await _context.Movies.AnyAsync(m => m.MovieId == movieId && !m.IsDeleted);

        // Checks whether an active store exists by ID.
        public async Task<bool> StoreExistsAsync(int storeId)
            => await _context.Stores.AnyAsync(s => s.StoreId == storeId && !s.IsDeleted);
    }
}
