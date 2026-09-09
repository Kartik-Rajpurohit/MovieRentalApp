using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to countries.
    public class CountryRepository : ICountryRepository
    {
        // Receives the database context used to access country records.
        private readonly AppDbContext _context;

        public CountryRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all countries from the database without tracking.
        public IQueryable<Country> GetAllCountries()
            => _context.Countries.AsNoTracking().AsQueryable();

        // Finds a country by its ID, loading its associated cities.
        public async Task<Country?> GetCountryByIdAsync(int id)
            => await _context.Countries
                .Include(c => c.Cities)
                .FirstOrDefaultAsync(c => c.CountryId == id);

        // Adds a new country record to the database and saves changes.
        public async Task<Country> CreateCountryAsync(Country country)
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
            return country;
        }

        // Updates an existing country's name and last-updated timestamp.
        public async Task<Country?> UpdateCountryAsync(Country country)
        {
            var existing = await _context.Countries.FindAsync(country.CountryId);
            if (existing == null) return null;
            existing.Name = country.Name;
            existing.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        // Removes the country from the database if found.
        public async Task<bool> DeleteCountryAsync(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null) return false;
            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
