using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Repository contract — raw DB operations only
    public interface IInventoryRepository
    {
        IQueryable<Inventory> GetAllInventory();
        Task<Inventory?> GetInventoryByIdAsync(int id);
        Task<Inventory> CreateInventoryAsync(CreateInventoryDto dto);
        Task<Inventory?> UpdateInventoryAsync(UpdateInventoryDto dto);
        Task<bool> DeleteInventoryAsync(int id);
    }
}
