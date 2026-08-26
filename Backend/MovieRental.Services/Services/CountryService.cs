using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Countries;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _countryRepository;

        public CountryService(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public async Task<PaginatedResponseDto<CountryResponseDto>> GetAllCountriesAsync(
            int page, int pageSize, string? search, string? sortField, string? sortOrder)
        {
            var query = _countryRepository.GetAllCountries();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Name.ToLower().Contains(search.ToLower()));

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

        public async Task<CountryResponseDto?> GetCountryByIdAsync(int id)
        {
            var country = await _countryRepository.GetCountryByIdAsync(id);
            if (country == null) return null;

            return new CountryResponseDto
            {
                CountryId  = country.CountryId,
                Name       = country.Name,
                CityCount  = country.Cities.Count,
                LastUpdate = country.LastUpdate,
            };
        }

        public async Task<CountryResponseDto> CreateCountryAsync(CreateCountryDto dto)
        {
            var country = new Country
            {
                Name       = dto.Name,
                LastUpdate = DateTime.UtcNow
            };

            var created = await _countryRepository.CreateCountryAsync(country);

            return new CountryResponseDto
            {
                CountryId  = created.CountryId,
                Name       = created.Name,
                CityCount  = 0,
                LastUpdate = created.LastUpdate,
            };
        }

        public async Task<CountryResponseDto?> UpdateCountryAsync(UpdateCountryDto dto)
        {
            var country = new Country
            {
                CountryId = dto.CountryId,
                Name      = dto.Name,
            };

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

        public async Task<bool> DeleteCountryAsync(int id)
            => await _countryRepository.DeleteCountryAsync(id);
    }
}
