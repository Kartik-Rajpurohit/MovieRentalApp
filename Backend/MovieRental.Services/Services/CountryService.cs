using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Countries;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
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
            int page, int pageSize, string? search, string? sortField, string? sortOrder)
        {
            // Get the base query from the repository.
            var query = _countryRepository.GetAllCountries();

            // Apply case-insensitive country name search filter.
            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));

            // Sort by country name or city count.
            query = sortField?.ToLower() switch
            {
                "name" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                "citycount" => sortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.Cities.Count())
                    : query.OrderBy(c => c.Cities.Count()),
                _ => query.OrderBy(c => c.Name)
            };

            var totalRecords = await query.CountAsync();

            // Paginate and project country entities into response DTOs.
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                TotalPages   = (int)Math.Ceiling((double)totalRecords / pageSize),
                CurrentPage  = page,
                PageSize     = pageSize,
                Data         = data
            };
        }

        // Retrieves a single country by ID including its associated city count.
        public async Task<CountryResponseDto?> GetCountryByIdAsync(int id)
        {
            var country = await _countryRepository.GetCountryByIdAsync(id);
            if (country == null) return null;

            // Map entity to response DTO.
            return new CountryResponseDto
            {
                CountryId  = country.CountryId,
                Name       = country.Name,
                CityCount  = country.Cities.Count,
                LastUpdate = country.LastUpdate,
            };
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

            return new CountryResponseDto
            {
                CountryId  = created.CountryId,
                Name       = created.Name,
                CityCount  = 0,
                LastUpdate = created.LastUpdate,
            };
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

            return new CountryResponseDto
            {
                CountryId  = updated.CountryId,
                Name       = updated.Name,
                CityCount  = updated.Cities.Count,
                LastUpdate = updated.LastUpdate,
            };
        }

        // Deletes a country record by ID through the repository.
        public async Task<bool> DeleteCountryAsync(int id)
            => await _countryRepository.DeleteCountryAsync(id);
    }
}
