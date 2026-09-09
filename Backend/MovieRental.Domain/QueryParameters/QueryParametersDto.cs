namespace MovieRental.Domain.QueryParameters
{
    // Base class containing common options used to filter, sort, and paginate API results
    public class QueryParametersDto
    {
        // Name of the field/column used to sort the results (e.g. "title", "createdAt")
        public string? SortField { get; set; }

        // Sort direction such as ascending ("asc") or descending ("desc")
        public string? SortOrder { get; set; }

        // Text keyword used to search across supported fields
        public string? Search { get; set; }

        // Current page number to retrieve (starts at 1)
        public int Page { get; set; } = 1;

        // Number of records to return per page (default is 10)
        public int PageSize { get; set; } = 10;
    }
}