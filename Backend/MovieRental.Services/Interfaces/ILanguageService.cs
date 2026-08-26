using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Domain.DTOs.Movies;

namespace MovieRental.Services.Interfaces;

public interface ILanguageService
{
    Task<IEnumerable<LanguageResponseDto>> GetAllLanguagesAsync();
    Task<LanguageResponseDto?> GetLanguageByIdAsync(int id);
    Task<LanguageResponseDto> CreateLanguageAsync(CreateLanguageDto dto);
    Task<LanguageResponseDto?> UpdateLanguageAsync(UpdateLanguageDto dto);
    Task<bool> DeleteLanguageAsync(int id);
    Task<LanguageDetailDto?> GetLanguageDetailAsync(int id);
    Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByLanguageAsync(int languageId, int page, int pageSize, string? search);
}

