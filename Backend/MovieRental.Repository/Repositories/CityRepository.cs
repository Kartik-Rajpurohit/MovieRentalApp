using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories;

public class CityRepository : ICityRepository
{
    private readonly AppDbContext _context;

    public CityRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<City> GetAllCities()
        => _context.Cities
            .Include(c => c.Country)
            .Include(c => c.Addresses)
            .AsQueryable();

    public async Task<City?> GetCityByIdAsync(int id)
        => await _context.Cities
            .Include(c => c.Country)
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.CityId == id);

    public async Task<City> CreateCityAsync(City city)
    {
        _context.Cities.Add(city);
        await _context.SaveChangesAsync();
        return await GetCityByIdAsync(city.CityId) ?? city;
    }

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

    public async Task<bool> DeleteCityAsync(int id)
    {
        var city = await _context.Cities.FindAsync(id);
        if (city == null) return false;
        _context.Cities.Remove(city);
        await _context.SaveChangesAsync();
        return true;
    }
}