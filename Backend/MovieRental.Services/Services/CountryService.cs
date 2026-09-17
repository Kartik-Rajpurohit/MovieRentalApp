using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Countries;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for country lookups and geographical configurations.
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _countryRepository;

        // Receives the country repository needed to perform country data operations.
        public CountryService(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        // Retrieves a paginated and searchable list of countries with sorting options.
        public async Task<PaginatedResponseDto<CountryResponseDto>> GetAllCountriesAsync(
            PaginationInputDto pagination,
            CountryFilterDto filter)
        {
            // Get the base query from the repository.
            var query = _countryRepository.GetAllCountries();

            // 1. Search by country name.
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var s = pagination.Search.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(s));
            }

            // 2. Module Filters
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                var name = filter.Name.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(name));
            }

            // 3. Dynamic Sorting
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "name" or "country" => isDesc
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                "citycount" => isDesc
                    ? query.OrderByDescending(c => c.Cities.Count())
                    : query.OrderBy(c => c.Cities.Count()),
                "id" or "countryid" => isDesc
                    ? query.OrderByDescending(c => c.CountryId)
                    : query.OrderBy(c => c.CountryId),
                _ => isDesc
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name)
            };

            // 4. Count
            var totalRecords = await query.CountAsync();
            var totalPages   = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // 5. Pagination & Projection
            var data = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(c => new CountryResponseDto
                {
                    CountryId  = c.CountryId,
                    Name       = c.Name,
                    CityCount  = c.Cities.Count(),
                    LastUpdate = c.LastUpdate,
                })
                .ToListAsync();

            return new PaginatedResponseDto<CountryResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages   = totalPages,
                CurrentPage  = pagination.Page,
                PageSize     = pagination.PageSize,
                Data         = data
            };
        }

        // Retrieves a single country by ID including its associated city count.
        public async Task<CountryResponseDto?> GetCountryByIdAsync(int id)
        {
            var country = await _countryRepository.GetCountryByIdAsync(id);
            if (country == null) return null;

            return country.ToResponseDto();
        }

        // Validates and saves a new country record.
        public async Task<CountryResponseDto> CreateCountryAsync(CreateCountryDto dto)
        {
            // Map request DTO to database entity.
            var country = new Country
            {
                Name       = dto.Name,
                LastUpdate = DateTime.UtcNow
            };

            // Save the country using the repository.
            var created = await _countryRepository.CreateCountryAsync(country);

            return created.ToResponseDto();
        }

        // Updates an existing country's name.
        public async Task<CountryResponseDto?> UpdateCountryAsync(UpdateCountryDto dto)
        {
            var country = new Country
            {
                CountryId = dto.CountryId,
                Name      = dto.Name,
            };

            // Save updates via repository.
            var updated = await _countryRepository.UpdateCountryAsync(country);
            if (updated == null) return null;

            return updated.ToResponseDto();
        }

        // Deletes a country record by ID through the repository.
        public async Task<bool> DeleteCountryAsync(int id)
            => await _countryRepository.DeleteCountryAsync(id);
    }
}
