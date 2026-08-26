using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _context;
        public CustomerRepository(AppDbContext context) => _context = context;

        // Returns IQueryable with User relation loaded — service applies filters on top
        public IQueryable<Customer> GetAllCustomers()
        {
            return _context.Customers
                .Include(c => c.User)
                .AsQueryable();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            // Load all relations needed for DTO mapping in service
            return await _context.Customers
                .Include(c => c.User)
                    .ThenInclude(u => u!.Role)
                .Include(c => c.User)
                    .ThenInclude(u => u!.Address)
                        .ThenInclude(a => a.City)
                            .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }
    }
}
