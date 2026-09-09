using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for physical Store branch locations.
    public interface IStoreRepository
    {
        // Returns a queryable collection of stores with manager and address details.
        IQueryable<Store> GetAllStores();

        // Adds a new store branch to the database.
        Task<Store> CreateStoreAsync(Store store);
    }
}
