using MovieRental.Domain.DTOs.Auth;

namespace MovieRental.Services.Interfaces;

// Defines integration operations for fetching country and city reference data from CountriesNow API
public interface ICountriesNowService
{
    // Fetches all available countries
    Task<IReadOnlyList<CountryLookupDto>> GetCountriesAsync();

    // Fetches cities for a specified country name
    Task<IReadOnlyList<CityLookupDto>> GetCitiesAsync(string countryName);
}
