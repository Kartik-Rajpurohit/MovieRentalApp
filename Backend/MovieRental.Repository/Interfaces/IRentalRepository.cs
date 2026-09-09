using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for Customer film Rentals.
    public interface IRentalRepository
    {
        // Returns a queryable collection of rentals with customer, staff, and film details.
        IQueryable<Rental> GetAllRentals();

        // Finds a rental record by its ID with full details.
        Task<Rental?> GetRentalByIdAsync(int id);

        // Creates a new rental record for an available inventory item.
        Task<Rental> CreateRentalAsync(Rental rental);

        // Marks a rental record as returned by setting its return date.
        Task<Rental?> ReturnRentalAsync(int rentalId);
    }
}
