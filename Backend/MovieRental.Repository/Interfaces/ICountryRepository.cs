using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    public interface ICountryRepository
    {
        IQueryable<Country> GetAllCountries();
        Task<Country?> GetCountryByIdAsync(int id);
        Task<Country> CreateCountryAsync(Country country);
        Task<Country?> UpdateCountryAsync(Country country);
        Task<bool> DeleteCountryAsync(int id);
    }
}
