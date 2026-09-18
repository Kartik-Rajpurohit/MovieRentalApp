using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieRental.Domain.DTOs.Auth;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

// Implements global address autocomplete integration with Geoapify API
public class GeoapifyService : IGeoapifyService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeoapifyService> _logger;

    public GeoapifyService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeoapifyService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    // Searches addresses worldwide and returns up to 10 matching suggestions
    public async Task<IReadOnlyList<AddressAutocompleteDto>> AutocompleteAsync(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText) || searchText.Trim().Length < 3)
        {
            return Array.Empty<AddressAutocompleteDto>();
        }

        var apiKey = _configuration["ExternalApis:Geoapify:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "YOUR_GEOAPIFY_API_KEY")
        {
            _logger.LogWarning("Geoapify API key is not configured or using default placeholder. Autocomplete disabled.");
            return Array.Empty<AddressAutocompleteDto>();
        }

        var normalizedQuery = searchText.Trim();
        var encodedText = Uri.EscapeDataString(normalizedQuery);
        var requestUri = $"autocomplete?text={encodedText}&apiKey={apiKey}&limit=10";

        try
        {
            // Step 1 — Fetch raw JSON response as a stream
            var jsonStream = await _httpClient.GetStreamAsync(requestUri);

            // Step 2 — Parse the full JSON document
            using var doc = await JsonDocument.ParseAsync(jsonStream);

            // Step 3 — Jump directly to "features" array, skip root object entirely
            if (!doc.RootElement.TryGetProperty("features", out var featuresElement))
            {
                _logger.LogWarning("Geoapify response did not contain a 'features' property.");
                return Array.Empty<AddressAutocompleteDto>();
            }

            var results = new List<AddressAutocompleteDto>();

            // Step 4 — Loop through each feature in the features array
            foreach (var feature in featuresElement.EnumerateArray())
            {
                // Step 5 — Jump directly to "properties", skip feature level
                if (!feature.TryGetProperty("properties", out var propsElement))
                    continue;

                // Step 6 — Deserialize "properties" JSON directly into single private DTO
                var props = propsElement.Deserialize<GeoapifyProperties>();
                if (props == null) continue;

                // Step 7 — Prefer dedicated street property, fall back to address_line1 if absent
                var street = !string.IsNullOrWhiteSpace(props.Street)
                    ? props.Street.Trim()
                    : props.AddressLine1?.Trim();

                // Step 8 — Map to clean output DTO
                results.Add(new AddressAutocompleteDto
                {
                    Street = street,
                    HouseNumber = props.HouseNumber?.Trim(),
                    City = props.City?.Trim(),
                    Country = props.Country?.Trim(),
                    CountryCode = props.CountryCode?.Trim(),
                    PostalCode = props.Postcode?.Trim(),
                    FormattedAddress = props.Formatted?.Trim()
                });
            }

            return results.AsReadOnly();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request failed while querying Geoapify address autocomplete for query length {Length}", normalizedQuery.Length);
            return Array.Empty<AddressAutocompleteDto>();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse Geoapify JSON response for query length {Length}", normalizedQuery.Length);
            return Array.Empty<AddressAutocompleteDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during Geoapify address autocomplete for query length {Length}", normalizedQuery.Length);
            return Array.Empty<AddressAutocompleteDto>();
        }
    }

    // Single private class — maps directly to the "properties" object inside each Geoapify feature
    // GeoapifyResponse and GeoapifyFeature wrapper classes are no longer needed
    // JsonDocument handles root and feature levels manually via TryGetProperty
    private class GeoapifyProperties
    {
        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("country_code")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("postcode")]
        public string? Postcode { get; set; }

        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("housenumber")]
        public string? HouseNumber { get; set; }

        [JsonPropertyName("formatted")]
        public string? Formatted { get; set; }

        [JsonPropertyName("address_line1")]
        public string? AddressLine1 { get; set; }

        [JsonPropertyName("address_line2")]
        public string? AddressLine2 { get; set; }
    }
}