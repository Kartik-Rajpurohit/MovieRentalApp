using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    // Handles database operations related to films, including genres, actors, and languages.
    public class FilmRepository : IFilmRepository
    {
        // Receives the database context used to access film and media tables.
        private readonly AppDbContext _context;

        public FilmRepository(AppDbContext context)
        {
            _context = context;
        }

        // Reads all films without tracking, loading related language, categories, and actors.
        // Returns IQueryable so filtering, sorting, and pagination can be applied in the service.
        public IQueryable<Film> GetAllFilms()
        {
            return _context.Films
                .AsNoTracking()
                .Include(f => f.Language)
                .Include(f => f.FilmCategories)
                    .ThenInclude(fc => fc.Category)
                .Include(f => f.FilmActors)
                    .ThenInclude(fa => fa.Actor);
        }

        // Finds a film by ID with all related categories, actors, languages, and inventory items.
        public async Task<Film?> GetFilmByIdAsync(int id)
        {
            return await _context.Films
                .Include(f => f.Language)
                .Include(f => f.OriginalLanguage)
                .Include(f => f.FilmCategories)
                    .ThenInclude(fc => fc.Category)
                .Include(f => f.FilmActors)
                    .ThenInclude(fa => fa.Actor)
                .Include(f => f.Inventories)
                .FirstOrDefaultAsync(f => f.FilmId == id);
        }

        // Adds a new film to the database and re-fetches it with full relationships.
        public async Task<Film> CreateFilmAsync(Film film)
        {
            _context.Films.Add(film);
            await _context.SaveChangesAsync();

            // Reload relations after insert so service can map to DTO
            return await GetFilmByIdAsync(film.FilmId) ?? film;
        }

        // Updates film details, sets last-updated timestamp, and saves changes.
        public async Task<Film?> UpdateFilmAsync(Film film)
        {
            film.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload relations after update
            return await GetFilmByIdAsync(film.FilmId);
        }

        // Removes a film from the database if it exists.
        public async Task<bool> DeleteFilmAsync(int id)
        {
            var film = await _context.Films.FindAsync(id);
            if (film == null) return false;

            _context.Films.Remove(film);
            await _context.SaveChangesAsync();
            return true;
        }

        // Returns all languages without tracking for film form dropdowns.
        public IQueryable<Language> GetAllLanguages()
            => _context.Languages.AsNoTracking().AsQueryable();

        // Returns all categories without tracking for film genre selection.
        public IQueryable<Category> GetAllCategories()
            => _context.Categories.AsNoTracking().AsQueryable();

        // Returns all actors without tracking for film cast selection.
        public IQueryable<Actor> GetAllActors()
            => _context.Actors.AsNoTracking().AsQueryable();
    }
}
