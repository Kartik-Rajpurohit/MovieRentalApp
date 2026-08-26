using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces;

public interface ILanguageRepository
{
    IQueryable<Language> GetAllLanguages();
    Task<Language?> GetLanguageByIdAsync(int id);
    Task<Language> CreateLanguageAsync(Language language);
    Task<Language?> UpdateLanguageAsync(Language language);
    Task<bool> DeleteLanguageAsync(int id);
    IQueryable<Film> GetFilmsByLanguageId(int languageId);
}

