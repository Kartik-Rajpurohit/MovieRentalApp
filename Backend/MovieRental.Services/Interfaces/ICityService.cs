using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Cities;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces;

// Defines business operations for managing cities and location data.
public interface ICityService
{
    // Retrieves a paginated and filtered list of cities.
    Task<PaginatedResponseDto<CityResponseDto>> GetAllCitiesAsync(CityQueryParametersDto queryParams);

    // Retrieves detailed city information by ID, including its country.
    Task<CityDetailDto?> GetCityByIdAsync(int id);

    // Creates a new city record associated with a country.
    Task<CityResponseDto> CreateCityAsync(CreateCityDto dto);

    // Updates an existing city record.
    Task<CityResponseDto?> UpdateCityAsync(UpdateCityDto dto);

    // Removes a city record by its ID.
    Task<bool> DeleteCityAsync(int id);
}
