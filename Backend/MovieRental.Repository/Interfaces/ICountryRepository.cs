using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for Country entities.
    public interface ICountryRepository
    {
        // Returns a queryable collection of all countries.
        IQueryable<Country> GetAllCountries();

        // Finds a country by its ID, including its cities.
        Task<Country?> GetCountryByIdAsync(int id);

        // Adds a new country record to the database.
        Task<Country> CreateCountryAsync(Country country);

        // Updates an existing country's name.
        Task<Country?> UpdateCountryAsync(Country country);

        // Deletes a country by ID; returns true if deleted, false if not found.
        Task<bool> DeleteCountryAsync(int id);
    }
}
