namespace MovieRental.Domain.QueryParameters;

// Contains options used to filter, sort, and paginate actors returned by the API
public class ActorQueryParametersDto
{
    // Current page number to return (starts at 1)
    public int Page { get; set; } = 1;

    // Number of actor records per page
    public int PageSize { get; set; } = 10;

    // Text used to search actors by first name or last name
    public string? Search { get; set; }

    // Field name used for sorting (e.g. "firstName", "lastName")
    public string? SortField { get; set; }

    // Sorting direction ("asc" for ascending, "desc" for descending)
    public string? SortOrder { get; set; }

    // Filters to get a specific actor by ID
    public int? ActorId { get; set; }
}