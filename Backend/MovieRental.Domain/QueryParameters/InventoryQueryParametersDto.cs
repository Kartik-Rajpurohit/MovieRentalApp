namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate physical inventory copies returned by the API
public class InventoryQueryParametersDto : QueryParametersDto
{
    // Filters inventory copies of a specific film ID
    public int? FilmId { get; set; }

    // Filters inventory copies located at a specific store ID
    public int? StoreId { get; set; }

    // Filters inventory copies by availability (true = available to rent, false = currently rented out)
    public bool? IsAvailable { get; set; }
}
