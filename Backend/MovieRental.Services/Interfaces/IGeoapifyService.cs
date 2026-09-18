using MovieRental.Domain.DTOs.Auth;

namespace MovieRental.Services.Interfaces;

// Defines integration operations for global address autocomplete via Geoapify API
public interface IGeoapifyService
{
    // Searches addresses worldwide and returns matching suggestions
    Task<IReadOnlyList<AddressAutocompleteDto>> AutocompleteAsync(string searchText);
}
