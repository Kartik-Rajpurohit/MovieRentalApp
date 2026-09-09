using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Repository contract — raw DB operations only
    public interface IInventoryRepository
    {
        IQueryable<Inventory> GetAllInventory();
        Task<Inventory?> GetInventoryByIdAsync(int id);
        Task<Inventory> CreateInventoryAsync(Inventory inventory);
        Task<Inventory?> UpdateInventoryAsync(Inventory inventory);
        Task<bool> DeleteInventoryAsync(int id);
    }
}
