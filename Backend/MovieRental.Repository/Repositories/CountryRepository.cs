using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly AppDbContext _context;

        public CountryRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Country> GetAllCountries()
            => _context.Countries.AsQueryable();

        public async Task<Country?> GetCountryByIdAsync(int id)
            => await _context.Countries
                .Include(c => c.Cities)
                .FirstOrDefaultAsync(c => c.CountryId == id);

        public async Task<Country> CreateCountryAsync(Country country)
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
            return country;
        }

        public async Task<Country?> UpdateCountryAsync(Country country)
        {
            var existing = await _context.Countries.FindAsync(country.CountryId);
            if (existing == null) return null;
            existing.Name = country.Name;
            existing.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

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
