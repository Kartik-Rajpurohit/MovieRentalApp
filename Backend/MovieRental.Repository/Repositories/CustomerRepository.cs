using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to customers.
    public class CustomerRepository : ICustomerRepository
    {
        // Receives the database context used to access customer data.
        private readonly AppDbContext _context;
        public CustomerRepository(AppDbContext context) => _context = context;

        // Reads all customers without tracking, including linked User account details.
        public IQueryable<Customer> GetAllCustomers()
        {
            return _context.Customers
                .AsNoTracking()
                .Include(c => c.User)
                .AsQueryable();
        }

        // Finds a customer by ID, loading full User, Role, Address, City, and Country hierarchies.
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.User)
                    .ThenInclude(u => u!.Role)
                .Include(c => c.User)
                    .ThenInclude(u => u!.Address)
                        .ThenInclude(a => a!.City)
                            .ThenInclude(c => c!.Country)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }
    }
}
