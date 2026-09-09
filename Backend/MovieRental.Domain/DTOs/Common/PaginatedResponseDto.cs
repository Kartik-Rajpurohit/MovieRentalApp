namespace MovieRental.Domain.DTOs.Common
{
    // Generic wrapper for paginated API responses — used across all list endpoints
    public class PaginatedResponseDto<T>
    {
        // Total count of matching records across all pages
        public int TotalRecords { get; set; }

        // Total number of pages calculated from total records and page size
        public int TotalPages { get; set; }

        // Current page number returned in this response
        public int CurrentPage { get; set; }

        // Number of items per page
        public int PageSize { get; set; }

        // The list of items on the current page
        public IEnumerable<T> Data { get; set; } = new List<T>();
    }
}