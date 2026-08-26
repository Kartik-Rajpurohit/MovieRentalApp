using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        // Returns IQueryable with Film and Rentals loaded — service applies filters
        public IQueryable<Inventory> GetAllInventory()
        {
            return _context.Inventories
                .Include(i => i.Film)
                .Include(i => i.Rentals);
        }

        public async Task<Inventory?> GetInventoryByIdAsync(int id)
        {
            return await _context.Inventories
                .Include(i => i.Film)
                .Include(i => i.Rentals)
                .FirstOrDefaultAsync(i => i.InventoryId == id);
        }

        public async Task<Inventory> CreateInventoryAsync(CreateInventoryDto dto)
        {
            var inventory = new Inventory
            {
                FilmId = dto.FilmId,
                StoreId = dto.StoreId,
                LastUpdate = DateTime.UtcNow
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            // Reload with relations for service mapping
            await _context.Entry(inventory).Reference(i => i.Film).LoadAsync();
            await _context.Entry(inventory).Collection(i => i.Rentals).LoadAsync();

            return inventory;
        }

        public async Task<Inventory?> UpdateInventoryAsync(UpdateInventoryDto dto)
        {
            var inventory = await _context.Inventories
                .Include(i => i.Film)
                .Include(i => i.Rentals)
                .FirstOrDefaultAsync(i => i.InventoryId == dto.InventoryId);

            if (inventory == null) return null;

            // Only update StoreId if provided — PATCH behaviour
            if (dto.StoreId.HasValue) inventory.StoreId = dto.StoreId.Value;
            inventory.LastUpdate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<bool> DeleteInventoryAsync(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null) return false;

            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
