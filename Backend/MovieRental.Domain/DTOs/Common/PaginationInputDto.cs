namespace MovieRental.Domain.DTOs.Common
{
    // Standard pagination, search, and sorting input parameters for listing endpoints
    public class PaginationInputDto
    {
        // Current page number (starts at 1)
        public int Page { get; set; } = 1;

        // Number of records per page (default is 10)
        public int PageSize { get; set; } = 10;

        // General search term across supported text fields
        public string? Search { get; set; }

        // Field name to sort by
        public string? SortBy { get; set; }

        // Sort direction: "asc" or "desc" (default is "asc")
        public string? SortOrder { get; set; } = "asc";
    }
}
