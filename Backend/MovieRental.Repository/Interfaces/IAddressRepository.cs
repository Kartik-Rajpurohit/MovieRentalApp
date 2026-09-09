using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces;

// Defines database operations for Address entities linked to cities and countries.
public interface IAddressRepository
{
    // Returns a queryable collection of all addresses with city and country details.
    IQueryable<Address> GetAllAddresses();

    // Finds an address by its ID.
    Task<Address?> GetAddressByIdAsync(int id);

    // Adds a new address record to the database.
    Task<Address> CreateAddressAsync(Address address);

    // Updates an existing address record.
    Task<Address?> UpdateAddressAsync(Address address);

    // Deletes an address by ID; returns true if deleted, false if not found.
    Task<bool> DeleteAddressAsync(int id);
}