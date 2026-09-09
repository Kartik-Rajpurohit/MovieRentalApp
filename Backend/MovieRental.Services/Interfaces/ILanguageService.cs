using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Domain.DTOs.Movies;

namespace MovieRental.Services.Interfaces;

// Defines business operations for languages used by movies.
public interface ILanguageService
{
    // Retrieves all available languages.
    Task<IEnumerable<LanguageResponseDto>> GetAllLanguagesAsync();

    // Retrieves a single language by ID.
    Task<LanguageResponseDto?> GetLanguageByIdAsync(int id);

    // Creates a new language entry.
    Task<LanguageResponseDto> CreateLanguageAsync(CreateLanguageDto dto);

    // Updates an existing language name.
    Task<LanguageResponseDto?> UpdateLanguageAsync(UpdateLanguageDto dto);

    // Deletes a language by ID.
    Task<bool> DeleteLanguageAsync(int id);

    // Retrieves detailed language information including movie count.
    Task<LanguageDetailDto?> GetLanguageDetailAsync(int id);

    // Retrieves a paginated list of movies associated with a specific language.
    Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByLanguageAsync(int languageId, int page, int pageSize, string? search);
}

