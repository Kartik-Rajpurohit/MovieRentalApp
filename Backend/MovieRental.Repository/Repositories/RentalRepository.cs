using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to movie rentals.
    public class RentalRepository : IRentalRepository
    {
        // Receives the database context used to access rental records.
        private readonly AppDbContext _context;

        public RentalRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all rentals without tracking, loading inventory, film, customer, and staff details.
        public IQueryable<Rental> GetAllRentals()
            => _context.Rentals
                .AsNoTracking()
                .Include(r => r.Inventory).ThenInclude(i => i.Film)
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Staff).ThenInclude(s => s.User)
                .AsQueryable();

        // Finds a rental by ID, including film, customer, staff, and linked payment records.
        public async Task<Rental?> GetRentalByIdAsync(int id)
            => await _context.Rentals
                .Include(r => r.Inventory).ThenInclude(i => i.Film)
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Staff).ThenInclude(s => s.User)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.RentalId == id);

        // Inserts a new rental record and reloads all its relations.
        public async Task<Rental> CreateRentalAsync(Rental rental)
        {
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            // Reload with relations
            return await GetRentalByIdAsync(rental.RentalId) ?? rental;
        }

        // Stamps the return timestamp on a rental record.
        public async Task<Rental?> ReturnRentalAsync(int rentalId)
        {
            var rental = await _context.Rentals.FindAsync(rentalId);
            if (rental == null) return null;

            // Idempotent guard: if already returned, do not overwrite the original ReturnDate
            if (rental.ReturnDate != null)
            {
                return await GetRentalByIdAsync(rentalId);
            }

            rental.ReturnDate = DateTime.UtcNow;
            rental.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetRentalByIdAsync(rentalId);
        }
    }
}
