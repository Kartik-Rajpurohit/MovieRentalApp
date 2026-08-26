using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly AppDbContext _context;

    public AddressRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Address> GetAllAddresses()
        => _context.Addresses
            .Include(a => a.City).ThenInclude(c => c.Country)
            .AsQueryable();

    public async Task<Address?> GetAddressByIdAsync(int id)
        => await _context.Addresses
            .Include(a => a.City).ThenInclude(c => c.Country)
            .Include(a => a.Users)
            .Include(a => a.Stores)
            .FirstOrDefaultAsync(a => a.AddressId == id);

    public async Task<Address> CreateAddressAsync(Address address)
    {
        _context.Addresses.Add(address);
        await _context.SaveChangesAsync();
        return await GetAddressByIdAsync(address.AddressId) ?? address;
    }

    public async Task<Address?> UpdateAddressAsync(Address address)
    {
        var existing = await _context.Addresses.FindAsync(address.AddressId);
        if (existing == null) return null;
        existing.Street = address.Street;
        existing.Street2 = address.Street2;
        existing.District = address.District;
        existing.PostalCode = address.PostalCode;
        existing.Phone = address.Phone;
        existing.CityId = address.CityId;
        existing.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetAddressByIdAsync(existing.AddressId);
    }

    public async Task<bool> DeleteAddressAsync(int id)
    {
        var address = await _context.Addresses.FindAsync(id);
        if (address == null) return false;
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
        return true;
    }
}