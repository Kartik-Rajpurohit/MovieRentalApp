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

        // Reads all active rentals without tracking, loading inventory, movie, customer, and staff details.
        public IQueryable<Rental> GetAllRentals()
            => _context.Rentals
                .AsNoTracking()
                .Where(r => !r.IsDeleted)
                .Include(r => r.Inventory).ThenInclude(i => i.Movie)
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Staff).ThenInclude(s => s.User)
                .AsQueryable();

        // Finds an active rental by ID, including movie, customer, staff, and linked payment records.
        public async Task<Rental?> GetRentalByIdAsync(int id)
            => await _context.Rentals
                .Where(r => !r.IsDeleted)
                .Include(r => r.Inventory).ThenInclude(i => i.Movie)
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

        // Checks whether an active customer exists by ID.
        public async Task<bool> CustomerExistsAsync(int customerId)
            => await _context.Customers.AnyAsync(c => c.CustomerId == customerId && !c.IsDeleted);

        // Checks whether an active staff member exists by ID.
        public async Task<bool> StaffExistsAsync(int staffId)
            => await _context.Staff.AnyAsync(s => s.StaffId == staffId && !s.IsDeleted);
    }
}
