using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Cities;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

// Handles business logic for city records and country linkages.
public class CityService : ICityService
{
    private readonly ICityRepository _cityRepository;

    // Receives the city repository needed to perform city queries and updates.
    public CityService(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository;
    }

    // Retrieves a paginated and filtered list of cities with country and address statistics.
    public async Task<PaginatedResponseDto<CityResponseDto>> GetAllCitiesAsync(CityQueryParametersDto queryParams)
    {
        // Get the base query from the repository.
        var query = _cityRepository.GetAllCities();

        // Filter by specific country ID if provided.
        if (queryParams.CountryId.HasValue)
            query = query.Where(c => c.CountryId == queryParams.CountryId.Value);

        // Filter by city name using case-insensitive search.
        if (!string.IsNullOrEmpty(queryParams.Search))
            query = query.Where(c => c.Name.ToLower().Contains(queryParams.Search.ToLower()));

        // Apply sorting based on city name, country name, or address count.
        query = queryParams.SortField?.ToLower() switch
        {
            "name" => queryParams.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "country" => queryParams.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(c => c.Country.Name)
                : query.OrderBy(c => c.Country.Name),
            "addresscount" => queryParams.SortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(c => c.Addresses.Count)
                : query.OrderBy(c => c.Addresses.Count),
            _ => query.OrderBy(c => c.CityId)
        };

        var totalRecords = await query.CountAsync();

        // Paginate and project city entities into response DTOs.
        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(c => new CityResponseDto
            {
                CityId = c.CityId,
                Name = c.Name,
                CountryId = c.CountryId,
                CountryName = c.Country.Name,
                AddressCount = c.Addresses.Count,
                LastUpdate = c.LastUpdate,
            })
            .ToListAsync();

        return new PaginatedResponseDto<CityResponseDto>
        {
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize),
            CurrentPage = queryParams.Page,
            PageSize = queryParams.PageSize,
            Data = data
        };
    }

    // Retrieves detailed city information by ID including associated address count.
    public async Task<CityDetailDto?> GetCityByIdAsync(int id)
    {
        var city = await _cityRepository.GetCityByIdAsync(id);
        if (city == null) return null;

        // Convert the database entity into the detailed response DTO.
        return new CityDetailDto
        {
            CityId = city.CityId,
            Name = city.Name,
            CountryId = city.CountryId,
            CountryName = city.Country.Name,
            AddressCount = city.Addresses.Count,
            LastUpdate = city.LastUpdate,
        };
    }

    // Creates a new city record associated with the specified country.
    public async Task<CityResponseDto> CreateCityAsync(CreateCityDto dto)
    {
        // Map request DTO to database entity.
        var city = new City
        {
            Name = dto.Name,
            CountryId = dto.CountryId,
            LastUpdate = DateTime.UtcNow,
        };

        // Persist the new city in the database.
        var created = await _cityRepository.CreateCityAsync(city);

        // Map the created entity to the response DTO.
        return new CityResponseDto
        {
            CityId = created.CityId,
            Name = created.Name,
            CountryId = created.CountryId,
            CountryName = created.Country.Name,
            AddressCount = 0,
            LastUpdate = created.LastUpdate,
        };
    }

    // Updates an existing city record with the new name and country assignment.
    public async Task<CityResponseDto?> UpdateCityAsync(UpdateCityDto dto)
    {
        var city = new City { CityId = dto.CityId, Name = dto.Name, CountryId = dto.CountryId };

        // Save updates via the repository.
        var updated = await _cityRepository.UpdateCityAsync(city);
        if (updated == null) return null;

        return new CityResponseDto
        {
            CityId = updated.CityId,
            Name = updated.Name,
            CountryId = updated.CountryId,
            CountryName = updated.Country.Name,
            AddressCount = updated.Addresses.Count,
            LastUpdate = updated.LastUpdate,
        };
    }

    // Deletes a city record by ID through the repository.
    public async Task<bool> DeleteCityAsync(int id)
        => await _cityRepository.DeleteCityAsync(id);
}
