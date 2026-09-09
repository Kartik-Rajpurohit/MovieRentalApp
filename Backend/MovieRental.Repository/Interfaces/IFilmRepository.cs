using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for Film entities, categories, actors, and dropdown datasets.
    public interface IFilmRepository
    {
        // Returns a queryable collection of films with languages, categories, and actors loaded.
        IQueryable<Film> GetAllFilms();

        // Finds a film by its ID with full relationship data.
        Task<Film?> GetFilmByIdAsync(int id);

        // Adds a new film record to the database.
        Task<Film> CreateFilmAsync(Film film);

        // Updates an existing film's details.
        Task<Film?> UpdateFilmAsync(Film film);

        // Deletes a film by ID; returns true if deleted, false if not found.
        Task<bool> DeleteFilmAsync(int id);

        // Returns all languages for film form dropdowns.
        IQueryable<Language> GetAllLanguages();

        // Returns all categories for film genre selection.
        IQueryable<Category> GetAllCategories();

        // Returns all actors for film cast selection.
        IQueryable<Actor> GetAllActors();
    }
}
