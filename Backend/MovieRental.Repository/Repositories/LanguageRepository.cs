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

    // Reads all active languages without tracking, including related movies for counting.
    public IQueryable<Language> GetAllLanguages()
        => _context.Languages.AsNoTracking().Where(l => !l.IsDeleted).Include(l => l.Movies).AsQueryable();

    // Finds an active language by its ID along with its associated movies.
    public async Task<Language?> GetLanguageByIdAsync(int id)
        => await _context.Languages
            .Where(l => !l.IsDeleted)
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
        if (existing == null || existing.IsDeleted) return null;
        existing.Name = language.Name;
        existing.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return existing;
    }

    // Soft-deletes a language from the database if found.
    public async Task<bool> DeleteLanguageAsync(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null || language.IsDeleted) return false;
        language.IsDeleted = true;
        language.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    // Queries active movies released in the specified language, including language and categories.
    public IQueryable<Movie> GetMoviesByLanguageId(int languageId)
        => _context.Movies
            .AsNoTracking()
            .Where(m => m.LanguageId == languageId && !m.IsDeleted)
            .Include(m => m.Language)
            .Include(m => m.MovieCategories)
                .ThenInclude(mc => mc.Category) // Load categories for mapping in service
            .AsQueryable();
}

