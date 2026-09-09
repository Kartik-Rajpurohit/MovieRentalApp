using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces;

// Defines database operations for film languages.
public interface ILanguageRepository
{
    // Returns a queryable collection of all languages with related films.
    IQueryable<Language> GetAllLanguages();

    // Finds a language by its ID.
    Task<Language?> GetLanguageByIdAsync(int id);

    // Adds a new language record to the database.
    Task<Language> CreateLanguageAsync(Language language);

    // Updates an existing language's name.
    Task<Language?> UpdateLanguageAsync(Language language);

    // Deletes a language by ID; returns true if deleted, false if not found.
    Task<bool> DeleteLanguageAsync(int id);

    // Returns a queryable collection of films matching the specified language.
    IQueryable<Film> GetFilmsByLanguageId(int languageId);
}

