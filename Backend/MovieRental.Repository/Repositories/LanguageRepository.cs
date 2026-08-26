using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories;

public class LanguageRepository : ILanguageRepository
{
    private readonly AppDbContext _context;

    public LanguageRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Language> GetAllLanguages()
        => _context.Languages.Include(l => l.Films).AsQueryable();

    public async Task<Language?> GetLanguageByIdAsync(int id)
        => await _context.Languages
            .Include(l => l.Films)
            .FirstOrDefaultAsync(l => l.LanguageId == id);

    public async Task<Language> CreateLanguageAsync(Language language)
    {
        _context.Languages.Add(language);
        await _context.SaveChangesAsync();
        return language;
    }

    public async Task<Language?> UpdateLanguageAsync(Language language)
    {
        var existing = await _context.Languages.FindAsync(language.LanguageId);
        if (existing == null) return null;
        existing.Name = language.Name;
        existing.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteLanguageAsync(int id)
    {
        var language = await _context.Languages.FindAsync(id);
        if (language == null) return false;
        _context.Languages.Remove(language);
        await _context.SaveChangesAsync();
        return true;
    }

    public IQueryable<Film> GetFilmsByLanguageId(int languageId)
        => _context.Films
            .Where(f => f.LanguageId == languageId)
            .Include(f => f.Language)
            .Include(f => f.FilmCategories)
                .ThenInclude(fc => fc.Category) // Load categories for mapping in service
            .AsQueryable();
}

