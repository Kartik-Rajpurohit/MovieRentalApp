namespace MovieRental.Domain.DTOs.Common
{
    // Generic wrapper for paginated API responses — used across all list endpoints
    public class PaginatedResponseDto<T>
    {
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public IEnumerable<T> Data { get; set; } = new List<T>();
    }
}