using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces
{
    // Defines database operations for Movie entities, categories, actors, and dropdown datasets.
    public interface IMovieRepository
    {
        // Returns a queryable collection of movies with languages, categories, and actors loaded.
        IQueryable<Movie> GetAllMovies();

        // Finds a movie by its ID with full relationship data.
        Task<Movie?> GetMovieByIdAsync(int id);

        // Adds a new movie record to the database.
        Task<Movie> CreateMovieAsync(Movie movie);

        // Updates an existing movie's details.
        Task<Movie?> UpdateMovieAsync(Movie movie);

        // Deletes a movie by ID; returns true if deleted, false if not found.
        Task<bool> DeleteMovieAsync(int id);

        // Returns all languages for movie form dropdowns.
        IQueryable<Language> GetAllLanguages();

        // Returns all categories for movie genre selection.
        IQueryable<Category> GetAllCategories();

        // Returns all actors for movie cast selection.
        IQueryable<Actor> GetAllActors();
    }
}
