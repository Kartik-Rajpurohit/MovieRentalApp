using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
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

        return entities.Select(l => l.ToResponseDto()).ToList();
    }

    // Retrieves a single language by ID.
    public async Task<LanguageResponseDto?> GetLanguageByIdAsync(int id)
    {
        var language = await _languageRepository.GetLanguageByIdAsync(id);
        if (language == null) return null;

        return language.ToResponseDto();
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
        return created.ToResponseDto();
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

        return updated.ToResponseDto();
    }

    // Deletes a language by ID through repository.
    public async Task<bool> DeleteLanguageAsync(int id)
        => await _languageRepository.DeleteLanguageAsync(id);


    // Retrieves a paginated list of movies associated with a specific language.
    public async Task<PaginatedResponseDto<MovieResponseDto>> GetMoviesByLanguageAsync(
        int languageId,
        PaginationInputDto pagination)
    {
        var query = _languageRepository.GetMoviesByLanguageId(languageId);

        // Filter movies by search term if provided.
        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var s = pagination.Search.Trim().ToLower();
            query = query.Where(m => m.Title.ToLower().Contains(s) || (m.Description != null && m.Description.ToLower().Contains(s)));
        }

        // Apply sorting
        var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = pagination.SortBy?.ToLower() switch
        {
            "title" => isDesc ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
            "releaseyear" => isDesc ? query.OrderByDescending(m => m.ReleaseYear) : query.OrderBy(m => m.ReleaseYear),
            "rentalrate" => isDesc ? query.OrderByDescending(m => m.RentalRate) : query.OrderBy(m => m.RentalRate),
            "length" => isDesc ? query.OrderByDescending(m => m.Length) : query.OrderBy(m => m.Length),
            "id" or "movieid" => isDesc ? query.OrderByDescending(m => m.MovieId) : query.OrderBy(m => m.MovieId),
            _ => isDesc ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title)
        };

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

        // Paginate and project movie entities to response DTOs.
        var data = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectToMovieResponseDto()
            .ToListAsync();

        return new PaginatedResponseDto<MovieResponseDto>
        {
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize,
            Data = data
        };
    }
}

