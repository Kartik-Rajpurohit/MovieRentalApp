using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Cities;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
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
    public async Task<PaginatedResponseDto<CityResponseDto>> GetAllCitiesAsync(
        PaginationInputDto pagination,
        CityFilterDto filter)
    {
        // Get the base query from the repository.
        var query = _cityRepository.GetAllCities();

        // 1. General search across city name and country name
        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var s = pagination.Search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(s) || c.Country.Name.ToLower().Contains(s));
        }

        // 2. Module Filters
        if (filter.CountryId.HasValue)
            query = query.Where(c => c.CountryId == filter.CountryId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => c.Name.ToLower().Contains(filter.Name.Trim().ToLower()));

        // 3. Dynamic Sorting
        var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = pagination.SortBy?.ToLower() switch
        {
            "name" or "city" => isDesc
                ? query.OrderByDescending(c => c.Name)
                : query.OrderBy(c => c.Name),
            "country" => isDesc
                ? query.OrderByDescending(c => c.Country.Name)
                : query.OrderBy(c => c.Country.Name),
            "addresscount" => isDesc
                ? query.OrderByDescending(c => c.Addresses.Count)
                : query.OrderBy(c => c.Addresses.Count),
            "id" or "cityid" => isDesc
                ? query.OrderByDescending(c => c.CityId)
                : query.OrderBy(c => c.CityId),
            _ => isDesc
                ? query.OrderByDescending(c => c.CityId)
                : query.OrderBy(c => c.CityId)
        };

        // 4. Count
        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

        // 5. Pagination & Projection
        var data = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
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
            TotalPages = totalPages,
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize,
            Data = data
        };
    }

    // Retrieves detailed city information by ID including associated address count.
    public async Task<CityDetailDto?> GetCityByIdAsync(int id)
    {
        var city = await _cityRepository.GetCityByIdAsync(id);
        if (city == null) return null;

        // Convert the database entity into the detailed response DTO.
        return city.ToDetailDto();
    }

    // Creates a new city record associated with the specified country.
    public async Task<CityResponseDto> CreateCityAsync(CreateCityDto dto)
    {
        if (!await _cityRepository.CountryExistsAsync(dto.CountryId))
        {
            throw new InvalidOperationException($"Country with ID {dto.CountryId} does not exist or has been deleted.");
        }

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
        return created.ToResponseDto();
    }

    // Updates an existing city record with the new name and country assignment.
    public async Task<CityResponseDto?> UpdateCityAsync(UpdateCityDto dto)
    {
        if (!await _cityRepository.CountryExistsAsync(dto.CountryId))
        {
            throw new InvalidOperationException($"Country with ID {dto.CountryId} does not exist or has been deleted.");
        }

        var city = new City { CityId = dto.CityId, Name = dto.Name, CountryId = dto.CountryId };

        // Save updates via the repository.
        var updated = await _cityRepository.UpdateCityAsync(city);
        if (updated == null) return null;

        return updated.ToResponseDto();
    }

    // Deletes a city record by ID through the repository.
    public async Task<bool> DeleteCityAsync(int id)
        => await _cityRepository.DeleteCityAsync(id);
}
