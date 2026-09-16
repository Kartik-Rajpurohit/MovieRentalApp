using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to movies, including genres, actors, and languages.
    public class MovieRepository : IMovieRepository
    {
        // Receives the database context used to access movie and media tables.
        private readonly AppDbContext _context;

        public MovieRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all movies without tracking, loading related language, categories, and actors.
        // Returns IQueryable so filtering, sorting, and pagination can be applied in the service.
        public IQueryable<Movie> GetAllMovies()
        {
            return _context.Movies
                .AsNoTracking()
                .Include(m => m.Language)
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor);
        }

        // Finds a movie by ID with all related categories, actors, languages, and inventory items.
        public async Task<Movie?> GetMovieByIdAsync(int id)
        {
            return await _context.Movies
                .Include(m => m.Language)
                .Include(m => m.OriginalLanguage)
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.Inventories)
                .FirstOrDefaultAsync(m => m.MovieId == id);
        }

        // Adds a new movie to the database and re-fetches it with full relationships.
        public async Task<Movie> CreateMovieAsync(Movie movie)
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            // Reload relations after insert so service can map to DTO
            return await GetMovieByIdAsync(movie.MovieId) ?? movie;
        }

        // Updates movie details, sets last-updated timestamp, and saves changes.
        public async Task<Movie?> UpdateMovieAsync(Movie movie)
        {
            movie.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload relations after update
            return await GetMovieByIdAsync(movie.MovieId);
        }

        // Removes a movie from the database if it exists.
        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return false;

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return true;
        }

        // Returns all languages without tracking for movie form dropdowns.
        public IQueryable<Language> GetAllLanguages()
            => _context.Languages.AsNoTracking().AsQueryable();

        // Returns all categories without tracking for movie genre selection.
        public IQueryable<Category> GetAllCategories()
            => _context.Categories.AsNoTracking().AsQueryable();

        // Returns all actors without tracking for movie cast selection.
        public IQueryable<Actor> GetAllActors()
            => _context.Actors.AsNoTracking().AsQueryable();
    }
}
