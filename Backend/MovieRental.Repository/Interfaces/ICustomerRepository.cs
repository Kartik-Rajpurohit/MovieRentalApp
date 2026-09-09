using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for Customer entities linked to User, Address, and Rentals.
    public interface ICustomerRepository
    {
        // Returns a queryable collection of customers with linked user profiles.
        IQueryable<Customer> GetAllCustomers();

        // Finds a customer by their ID, loading full profile, address, and rental records.
        Task<Customer?> GetCustomerByIdAsync(int id);
    }
}