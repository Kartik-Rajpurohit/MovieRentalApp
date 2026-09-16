using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for Inventory physical movie copies.
    public interface IInventoryRepository
    {
        // Returns a queryable collection of inventory items with movie, store, and rental history.
        IQueryable<Inventory> GetAllInventory();

        // Finds an inventory copy by its ID with full movie, store, and rental details.
        Task<Inventory?> GetInventoryByIdAsync(int id);

        // Adds a new physical copy of a movie to a store.
        Task<Inventory> CreateInventoryAsync(Inventory inventory);

        // Updates an inventory item's store assignment.
        Task<Inventory?> UpdateInventoryAsync(Inventory inventory);

        // Deletes an inventory copy by ID; returns true if deleted, false if not found.
        Task<bool> DeleteInventoryAsync(int id);
    }
}
