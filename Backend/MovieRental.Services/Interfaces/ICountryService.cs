using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Countries;

namespace MovieRental.Services.Interfaces
{
    public interface ICountryService
    {
        Task<PaginatedResponseDto<CountryResponseDto>> GetAllCountriesAsync(int page, int pageSize, string? search, string? sortField, string? sortOrder);
        Task<CountryResponseDto?> GetCountryByIdAsync(int id);
        Task<CountryResponseDto> CreateCountryAsync(CreateCountryDto dto);
        Task<CountryResponseDto?> UpdateCountryAsync(UpdateCountryDto dto);
        Task<bool> DeleteCountryAsync(int id);
    }
}
