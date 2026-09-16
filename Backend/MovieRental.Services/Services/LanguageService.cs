using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

// Handles business logic for movie language options and movie associations.
public class LanguageService : ILanguageService
{
    private readonly ILanguageRepository _languageRepository;

    // Receives the language repository needed for language operations.
    public LanguageService(ILanguageRepository languageRepository)
    {
        _languageRepository = languageRepository;
    }

    // Retrieves all available languages with associated movie counts.
    public async Task<IEnumerable<LanguageResponseDto>> GetAllLanguagesAsync()
    {
        // Fetch entities first, then map in memory — avoids EF Core translation issues
        var entities = await _languageRepository.GetAllLanguages()
            .OrderBy(l => l.Name)
            .ToListAsync();

        return entities.Select(l => new LanguageResponseDto
        {
            LanguageId = l.LanguageId,
            Name = l.Name,
            LastUpdate = l.LastUpdate,
            MovieCount = l.Movies.Count
        }).ToList();
    }

    // Retrieves a single language by ID.
    public async Task<LanguageResponseDto?> GetLanguageByIdAsync(int id)
    {
        var language = await _languageRepository.GetLanguageByIdAsync(id);
        if (language == null) return null;

        return new LanguageResponseDto
        {
            LanguageId = language.LanguageId,
            Name = language.Name,
            LastUpdate = language.LastUpdate,
            MovieCount = language.Movies.Count
        };
    }

    // Creates a new language entry in the database.
    public async Task<LanguageResponseDto> CreateLanguageAsync(CreateLanguageDto dto)
    {
        var language = new Language
        {
            Name = dto.Name.Trim(),
            LastUpdate = DateTime.UtcNow
        };

        var created = await _languageRepository.CreateLanguageAsync(language);
        return new LanguageResponseDto
        {
            LanguageId = created.LanguageId,
            Name = created.Name,
            LastUpdate = created.LastUpdate,
            MovieCount = 0
        };
    }

    // Updates an existing language name.
    public async Task<LanguageResponseDto?> UpdateLanguageAsync(UpdateLanguageDto dto)
    {
        var language = new Language
        {
            LanguageId = dto.LanguageId,
            Name = dto.Name.Trim()
        };

        var updated = await _languageRepository.UpdateLanguageAsync(language);
        if (updated == null) return null;

        return new LanguageResponseDto
        {
            LanguageId = updated.LanguageId,
            Name = updated.Name,
            LastUpdate = updated.LastUpdate,
            MovieCount = updated.Movies.Count
        };
    }

    // Deletes a language by ID through repository.
    public async Task<bool> DeleteLanguageAsync(int id)
        => await _languageRepository.DeleteLanguageAsync(id);


    // Retrieves a paginated list of movies associated with a specific language.
    public async Task<PaginatedResponseDto<MovieResponseDto>> GetMoviesByLanguageAsync(
        int languageId, int page, int pageSize, string? search)
    {
        var query = _languageRepository.GetMoviesByLanguageId(languageId);

        // Filter movies by title if search query is provided.
        if (!string.IsNullOrEmpty(search))
            query = query.Where(m => m.Title.ToLower().Contains(search.ToLower()));

        var totalRecords = await query.CountAsync();

        // Paginate and project movie entities to response DTOs.
        var data = await query
            .OrderBy(m => m.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MovieResponseDto
            {
                MovieId = m.MovieId,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                LanguageId = m.LanguageId,
                LanguageName = m.Language.Name,
                RentalDuration = m.RentalDuration,
                RentalRate = m.RentalRate,
                Length = m.Length,
                ReplacementCost = m.ReplacementCost,
                Rating = m.Rating,
                Categories = m.MovieCategories
                    .Select(mc => mc.Category.Name)
                    .ToList()
            })
            .ToListAsync();

        return new PaginatedResponseDto<MovieResponseDto>
        {
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
            CurrentPage = page,
            PageSize = pageSize,
            Data = data
        };
    }
}

