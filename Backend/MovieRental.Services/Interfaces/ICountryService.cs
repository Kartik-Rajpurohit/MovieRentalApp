using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Countries;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for managing countries.
    public interface ICountryService
    {
        // Retrieves a paginated and searchable list of countries with sorting.
        Task<PaginatedResponseDto<CountryResponseDto>> GetAllCountriesAsync(int page, int pageSize, string? search, string? sortField, string? sortOrder);

        // Retrieves a single country by its ID.
        Task<CountryResponseDto?> GetCountryByIdAsync(int id);

        // Creates a new country record.
        Task<CountryResponseDto> CreateCountryAsync(CreateCountryDto dto);

        // Updates an existing country record.
        Task<CountryResponseDto?> UpdateCountryAsync(UpdateCountryDto dto);

        // Removes a country record by ID.
        Task<bool> DeleteCountryAsync(int id);
    }
}
