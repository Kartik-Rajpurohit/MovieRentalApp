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

    // Reads all cities without tracking, loading associated Country and Addresses.
    public IQueryable<City> GetAllCities()
        => _context.Cities
            .AsNoTracking()
            .Include(c => c.Country)
            .Include(c => c.Addresses)
            .AsQueryable();

    // Finds a city by its ID with country and address information.
    public async Task<City?> GetCityByIdAsync(int id)
        => await _context.Cities
            .Include(c => c.Country)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.CityId == id);

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
        if (existing == null) return null;
        existing.Name = city.Name;
        existing.CountryId = city.CountryId;
        existing.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return await GetCityByIdAsync(existing.CityId);
    }

    // Deletes the city record from the database if found.
    public async Task<bool> DeleteCityAsync(int id)
    {
        var city = await _context.Cities.FindAsync(id);
        if (city == null) return false;
        _context.Cities.Remove(city);
        await _context.SaveChangesAsync();
        return true;
    }
}