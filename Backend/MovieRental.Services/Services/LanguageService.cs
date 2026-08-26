using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

public class LanguageService : ILanguageService
{
    private readonly ILanguageRepository _languageRepository;

    public LanguageService(ILanguageRepository languageRepository)
    {
        _languageRepository = languageRepository;
    }

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
            FilmCount = l.Films.Count
        }).ToList();
    }

    public async Task<LanguageResponseDto?> GetLanguageByIdAsync(int id)
    {
        var language = await _languageRepository.GetLanguageByIdAsync(id);
        if (language == null) return null;

        return new LanguageResponseDto
        {
            LanguageId = language.LanguageId,
            Name = language.Name,
            LastUpdate = language.LastUpdate,
            FilmCount = language.Films.Count
        };
    }

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
            FilmCount = 0
        };
    }

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
            FilmCount = updated.Films.Count
        };
    }

    public async Task<bool> DeleteLanguageAsync(int id)
        => await _languageRepository.DeleteLanguageAsync(id);

    public async Task<LanguageDetailDto?> GetLanguageDetailAsync(int id)
    {
        var language = await _languageRepository.GetLanguageByIdAsync(id);
        if (language == null) return null;

        return new LanguageDetailDto
        {
            LanguageId = language.LanguageId,
            Name = language.Name,
            LastUpdate = language.LastUpdate,
            FilmCount = language.Films.Count
        };
    }

    public async Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByLanguageAsync(
        int languageId, int page, int pageSize, string? search)
    {
        var query = _languageRepository.GetFilmsByLanguageId(languageId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(f => f.Title.ToLower().Contains(search.ToLower()));

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(f => f.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new MovieResponseDto
            {
                FilmId = f.FilmId,
                Title = f.Title,
                Description = f.Description,
                ReleaseYear = f.ReleaseYear,
                LanguageId = f.LanguageId,
                LanguageName = f.Language.Name,
                RentalDuration = f.RentalDuration,
                RentalRate = f.RentalRate,
                Length = f.Length,
                ReplacementCost = f.ReplacementCost,
                Rating = f.Rating,
                Categories = f.FilmCategories
                    .Select(fc => fc.Category.Name)
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

