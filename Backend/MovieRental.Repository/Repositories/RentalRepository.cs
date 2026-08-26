using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly AppDbContext _context;

        public RentalRepository(AppDbContext context)
        {
            _context = context;
        }

        public IQueryable<Rental> GetAllRentals()
            => _context.Rentals
                .Include(r => r.Inventory).ThenInclude(i => i.Film)
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Staff).ThenInclude(s => s.User)
                .AsQueryable();

        public async Task<Rental?> GetRentalByIdAsync(int id)
            => await _context.Rentals
                .Include(r => r.Inventory).ThenInclude(i => i.Film)
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Staff).ThenInclude(s => s.User)
                .Include(r => r.Payments)
                .FirstOrDefaultAsync(r => r.RentalId == id);

        public async Task<Rental> CreateRentalAsync(Rental rental)
        {
            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            // Reload with relations
            return await GetRentalByIdAsync(rental.RentalId) ?? rental;
        }

        public async Task<Rental?> ReturnRentalAsync(int rentalId)
        {
            var rental = await _context.Rentals.FindAsync(rentalId);
            if (rental == null) return null;

            rental.ReturnDate = DateTime.UtcNow;
            rental.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetRentalByIdAsync(rentalId);
        }
    }
}
