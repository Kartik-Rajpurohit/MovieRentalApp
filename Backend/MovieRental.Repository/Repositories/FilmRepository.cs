using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories
{
    public class FilmRepository : IFilmRepository
    {
        private readonly AppDbContext _context;

        public FilmRepository(AppDbContext context)
        {
            _context = context;
        }

        // Returns IQueryable with all relations loaded — service applies filters on top
        public IQueryable<Film> GetAllFilms()
        {
            return _context.Films
                .Include(f => f.Language)
                .Include(f => f.FilmCategories)
                    .ThenInclude(fc => fc.Category)
                .Include(f => f.FilmActors)
                    .ThenInclude(fa => fa.Actor);
        }

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

        public async Task<Film> CreateFilmAsync(Film film)
        {
            _context.Films.Add(film);
            await _context.SaveChangesAsync();

            // Reload relations after insert so service can map to DTO
            return await GetFilmByIdAsync(film.FilmId) ?? film;
        }

        public async Task<Film?> UpdateFilmAsync(Film film)
        {
            film.LastUpdate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Reload relations after update
            return await GetFilmByIdAsync(film.FilmId);
        }

        public async Task<bool> DeleteFilmAsync(int id)
        {
            var film = await _context.Films.FindAsync(id);
            if (film == null) return false;

            _context.Films.Remove(film);
            await _context.SaveChangesAsync();
            return true;
        }

        // Raw IQueryable — service applies pagination and maps to DropdownDto
        public IQueryable<Language> GetAllLanguages()
            => _context.Languages.AsQueryable();

        public IQueryable<Category> GetAllCategories()
            => _context.Categories.AsQueryable();

        public IQueryable<Actor> GetAllActors()
            => _context.Actors.AsQueryable();
    }
}
