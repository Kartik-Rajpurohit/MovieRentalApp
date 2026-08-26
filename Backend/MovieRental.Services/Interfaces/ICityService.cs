using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Cities;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces;

public interface ICityService
{
    Task<PaginatedResponseDto<CityResponseDto>> GetAllCitiesAsync(CityQueryParametersDto queryParams);
    Task<CityDetailDto?> GetCityByIdAsync(int id);
    Task<CityResponseDto> CreateCityAsync(CreateCityDto dto);
    Task<CityResponseDto?> UpdateCityAsync(UpdateCityDto dto);
    Task<bool> DeleteCityAsync(int id);
}
