using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories;

// Handles database operations related to addresses.
public class AddressRepository : IAddressRepository
{
    // Receives the database context used to access address and location tables.
    private readonly AppDbContext _context;

    public AddressRepository(AppDbContext context)
    {
        _context = context;
    }

    // Reads all addresses without tracking, loading related City and Country records.
    public IQueryable<Address> GetAllAddresses()
        => _context.Addresses
            .AsNoTracking()
            .Include(a => a.City).ThenInclude(c => c.Country)
            .AsQueryable();

    // Finds an address by ID, loading related City, Country, Users, and Stores.
    public async Task<Address?> GetAddressByIdAsync(int id)
        => await _context.Addresses
            .Include(a => a.City).ThenInclude(c => c.Country)
            .Include(a => a.Users)
            .Include(a => a.Stores)
            .FirstOrDefaultAsync(a => a.AddressId == id);

    // Adds a new address to the database and re-fetches it with related entities.
    public async Task<Address> CreateAddressAsync(Address address)
    {
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
        return await GetAddressByIdAsync(address.AddressId) ?? address;
    }

    // Updates an existing address record and refreshes its city navigation property.
    public async Task<Address?> UpdateAddressAsync(Address address)
    {
        var existing = await _context.Addresses.FindAsync(address.AddressId);
        if (existing == null) return null;
        existing.Street = address.Street;
        existing.PostalCode = address.PostalCode;
        existing.Phone = address.Phone;
        existing.CityId = address.CityId;
        existing.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetAddressByIdAsync(existing.AddressId);
    }

    // Deletes the address from the database if it exists.
    public async Task<bool> DeleteAddressAsync(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address == null) return false;
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
        return true;
    }
}