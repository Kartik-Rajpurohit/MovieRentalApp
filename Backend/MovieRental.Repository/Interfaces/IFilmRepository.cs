using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Repository contract — raw DB operations only
    public interface IFilmRepository
    {
        // Returns IQueryable with all relations — service applies filters on top
        IQueryable<Film> GetAllFilms();

        Task<Film?> GetFilmByIdAsync(int id);

        Task<Film> CreateFilmAsync(Film film);

        Task<Film?> UpdateFilmAsync(Film film);

        Task<bool> DeleteFilmAsync(int id);

        // Dropdowns for add/edit form
        IQueryable<Language> GetAllLanguages();
        IQueryable<Category> GetAllCategories();
        IQueryable<Actor> GetAllActors();
    }
}
