using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    public interface IRentalRepository
    {
        IQueryable<Rental> GetAllRentals();
        Task<Rental?> GetRentalByIdAsync(int id);
        Task<Rental> CreateRentalAsync(Rental rental);
        Task<Rental?> ReturnRentalAsync(int rentalId);
    }
}
