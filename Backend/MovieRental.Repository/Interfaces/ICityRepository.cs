using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces;

public interface ICityRepository
{
    IQueryable<City> GetAllCities();
    Task<City?> GetCityByIdAsync(int id);
    Task<City> CreateCityAsync(City city);
    Task<City?> UpdateCityAsync(City city);
    Task<bool> DeleteCityAsync(int id);
}