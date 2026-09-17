using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories;

// Handles database operations related to cities.
public class CityRepository : ICityRepository
{
    // Receives the database context used to access city records.
    private readonly AppDbContext _context;

    public CityRepository(AppDbContext context)
    {
        _context = context;
    }

    // Reads all active cities without tracking, loading associated Country and Addresses.
    public IQueryable<City> GetAllCities()
        => _context.Cities
            .AsNoTracking()
            .Where(c => !c.IsDeleted && !c.Country.IsDeleted)
            .Include(c => c.Country)
            .Include(c => c.Addresses.Where(a => !a.IsDeleted))
            .AsQueryable();

    // Finds an active city by its ID with country and address information.
    public async Task<City?> GetCityByIdAsync(int id)
        => await _context.Cities
            .Where(c => !c.IsDeleted)
            .Include(c => c.Country)
            .Include(c => c.Addresses.Where(a => !a.IsDeleted))
            .FirstOrDefaultAsync(c => c.CityId == id);

    // Finds an active city by name and country ID with case-insensitive matching.
    public async Task<City?> GetCityByNameAndCountryIdAsync(string name, int countryId)
    {
        var trimmed = name.Trim();
        return await _context.Cities
            .FirstOrDefaultAsync(c => !c.IsDeleted && c.CountryId == countryId && EF.Functions.ILike(c.Name, trimmed));
    }

    // Adds a new city to the database and re-fetches it with related entities.
    public async Task<City> CreateCityAsync(City city)
    {
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();
        return await GetCityByIdAsync(city.CityId) ?? city;
    }

    // Updates city name and country assignment.
    public async Task<City?> UpdateCityAsync(City city)
    {
        var existing = await _context.Cities.FindAsync(city.CityId);
        if (existing == null || existing.IsDeleted) return null;
        existing.Name = city.Name;
        existing.CountryId = city.CountryId;
        existing.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetCityByIdAsync(existing.CityId);
    }

    // Soft-deletes the city record from the database if found.
    public async Task<bool> DeleteCityAsync(int id)
    {
        var city = await _context.Cities.FindAsync(id);
        if (city == null || city.IsDeleted) return false;
        city.IsDeleted = true;
        city.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Checks whether an active country exists by ID.
    public async Task<bool> CountryExistsAsync(int countryId)
        => await _context.Countries.AnyAsync(c => c.CountryId == countryId && !c.IsDeleted);
}