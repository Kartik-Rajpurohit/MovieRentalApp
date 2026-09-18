namespace MovieRental.Domain.DTOs.Auth;

// Represents a structured address suggestion returned by the Geoapify autocomplete lookup
public class AddressAutocompleteDto
{
    // Street name (or primary address line)
    public string? Street { get; set; }

    // House or building number
    public string? HouseNumber { get; set; }

    // City name
    public string? City { get; set; }

    // Country name
    public string? Country { get; set; }

    // Two-letter country code (ISO 3166-1 alpha-2)
    public string? CountryCode { get; set; }

    // Postal / zip code
    public string? PostalCode { get; set; }

    // Full formatted address string for display
    public string? FormattedAddress { get; set; }
}
