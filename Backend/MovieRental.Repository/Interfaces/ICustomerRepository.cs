using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Repository contract — raw DB operations only
    public interface ICustomerRepository
    {
        IQueryable<Customer> GetAllCustomers();
        Task<Customer?> GetCustomerByIdAsync(int id); // Returns raw entity — service handles mapping
    }
}