using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces;

// Defines database operations for City entities associated with Countries.
public interface ICityRepository
{
    // Returns a queryable collection of all cities with country and address links.
    IQueryable<City> GetAllCities();

    // Finds a city by its ID.
    Task<City?> GetCityByIdAsync(int id);

    // Adds a new city to the database.
    Task<City> CreateCityAsync(City city);

    // Updates an existing city's details.
    Task<City?> UpdateCityAsync(City city);

    // Deletes a city by ID; returns true if deleted, false if not found.
    Task<bool> DeleteCityAsync(int id);
}