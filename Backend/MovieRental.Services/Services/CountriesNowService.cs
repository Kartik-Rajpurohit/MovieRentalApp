using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MovieRental.Domain.DTOs.Auth;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

// Implements integration with the external CountriesNow API to provide country and city reference data with in-memory caching
public class CountriesNowService : ICountriesNowService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CountriesNowService> _logger;

    private const string CountriesCacheKey = "CountriesNow_AllCountries";
    private static readonly TimeSpan CountriesCacheDuration = TimeSpan.FromHours(24);
    private static readonly TimeSpan CitiesCacheDuration = TimeSpan.FromHours(2);

    public CountriesNowService(HttpClient httpClient, IMemoryCache cache, ILogger<CountriesNowService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    // Fetches all countries from CountriesNow API or returns from cache if available
    public async Task<IReadOnlyList<CountryLookupDto>> GetCountriesAsync()
    {
        if (_cache.TryGetValue(CountriesCacheKey, out IReadOnlyList<CountryLookupDto>? cached) && cached != null)
        {
            return cached;
        }

        try
        {
            // CountriesNow returns country list via countries/info?returns=none
            var response = await _httpClient.GetFromJsonAsync<CountriesNowCountryListResponse>("countries/info?returns=none");

            if (response == null || response.Error || response.Data == null)
            {
                _logger.LogWarning("CountriesNow API returned error or empty data for countries: {Msg}", response?.Msg);
                return Array.Empty<CountryLookupDto>();
            }

            var list = response.Data
                .Where(c => !string.IsNullOrWhiteSpace(c.Name))
                .Select(c => new CountryLookupDto { Name = c.Name!.Trim() })
                .OrderBy(c => c.Name)
                .ToList()
                .AsReadOnly();

            _cache.Set(CountriesCacheKey, list, CountriesCacheDuration);
            return list;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failure when calling CountriesNow countries endpoint");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling CountriesNow countries endpoint");
            throw;
        }
    }

    // Fetches cities for a country from CountriesNow API or returns from cache if available
    public async Task<IReadOnlyList<CityLookupDto>> GetCitiesAsync(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
        {
            return Array.Empty<CityLookupDto>();
        }

        var normalizedCountry = countryName.Trim();
        var cacheKey = $"CountriesNow_Cities_{normalizedCountry.ToLower()}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<CityLookupDto>? cached) && cached != null)
        {
            return cached;
        }

        try
        {
            // CountriesNow cities endpoint: POST countries/cities with {"country": "..."}
            var requestBody = new { country = normalizedCountry };
            var httpResponse = await _httpClient.PostAsJsonAsync("countries/cities", requestBody);

            if (!httpResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("CountriesNow API returned status {StatusCode} for country {Country}",
                    httpResponse.StatusCode, normalizedCountry);
                return Array.Empty<CityLookupDto>();
            }

            var response = await httpResponse.Content.ReadFromJsonAsync<CountriesNowCityListResponse>();

            if (response == null || response.Error || response.Data == null)
            {
                _logger.LogWarning("CountriesNow API returned error or empty data for country {Country}: {Msg}",
                    normalizedCountry, response?.Msg);
                return Array.Empty<CityLookupDto>();
            }

            var list = response.Data
                .Where(city => !string.IsNullOrWhiteSpace(city))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(city => new CityLookupDto { Name = city.Trim() })
                .OrderBy(c => c.Name)
                .ToList()
                .AsReadOnly();

            _cache.Set(cacheKey, list, CitiesCacheDuration);
            return list;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failure when calling CountriesNow cities endpoint for {Country}", normalizedCountry);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error when calling CountriesNow cities endpoint for {Country}", normalizedCountry);
            throw;
        }
    }

    // Internal response models matching CountriesNow API JSON schema
    private class CountriesNowCountryListResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("data")]
        public List<CountryItem>? Data { get; set; }
    }

    private class CountryItem
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private class CountriesNowCityListResponse
    {
        [JsonPropertyName("error")]
        public bool Error { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("data")]
        public List<string>? Data { get; set; }
    }
}
