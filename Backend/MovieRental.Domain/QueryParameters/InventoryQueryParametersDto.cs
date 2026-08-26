namespace MovieRental.Domain.QueryParameters;

// Query params for inventory list — extends base with inventory-specific filters
public class InventoryQueryParametersDto : QueryParametersDto
{
    public int? FilmId { get; set; }    // filter by film
    public int? StoreId { get; set; }   // filter by store
    public bool? IsAvailable { get; set; }  // filter by availability
}
