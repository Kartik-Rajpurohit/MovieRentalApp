using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories;

// Handles database operations related to movie languages.
public class LanguageRepository : ILanguageRepository
{
    // Receives the database context used to access language tables.
    private readonly AppDbContext _context;

    public LanguageRepository(AppDbContext context)
    {
        _context = context;
    }

    // Reads all languages without tracking, including related movies for counting.
    public IQueryable<Language> GetAllLanguages()
        => _context.Languages.AsNoTracking().Include(l => l.Movies).AsQueryable();

    // Finds a language by its ID along with its associated movies.
    public async Task<Language?> GetLanguageByIdAsync(int id)
        => await _context.Languages
            .Include(l => l.Movies)
            .FirstOrDefaultAsync(l => l.LanguageId == id);

    // Adds a new language record to the database and saves changes.
    public async Task<Language> CreateLanguageAsync(Language language)
    {
        _context.Languages.Add(language);
        await _context.SaveChangesAsync();
        return language;
    }

    // Updates an existing language's name and last-updated timestamp.
    public async Task<Language?> UpdateLanguageAsync(Language language)
    {
        var existing = await _context.Languages.FindAsync(language.LanguageId);
        if (existing == null) return null;
        existing.Name = language.Name;
        existing.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return existing;
    }

    // Deletes a language from the database if found.
    public async Task<bool> DeleteLanguageAsync(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null) return false;
        _context.Languages.Remove(language);
        await _context.SaveChangesAsync();
        return true;
    }

    // Queries movies released in the specified language, including language and categories.
    public IQueryable<Movie> GetMoviesByLanguageId(int languageId)
        => _context.Movies
            .AsNoTracking()
            .Where(m => m.LanguageId == languageId)
            .Include(m => m.Language)
            .Include(m => m.MovieCategories)
                .ThenInclude(mc => mc.Category) // Load categories for mapping in service
            .AsQueryable();
}

