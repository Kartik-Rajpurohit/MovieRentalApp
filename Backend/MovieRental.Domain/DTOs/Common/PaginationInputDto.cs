using System.ComponentModel.DataAnnotations;

namespace MovieRental.Domain.DTOs.Common
{
    // Standard pagination, search, and sorting input parameters for listing endpoints
    public class PaginationInputDto
    {
        public const int MaxPageSizeLimit = 100;
        private int _page = 1;
        private int _pageSize = 10;

        // Current page number (starts at 1)
        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than or equal to 1.")]
        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        // Number of records per page (default is 10, max is 100)
        [Range(1, MaxPageSizeLimit, ErrorMessage = "PageSize must be between 1 and 100.")]
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > MaxPageSizeLimit ? MaxPageSizeLimit : value);
        }

        // General search term across supported text fields
        public string? Search { get; set; }

        // Field name to sort by
        public string? SortBy { get; set; }

        // Sort direction: "asc" or "desc" (default is "desc")
        public string? SortOrder { get; set; } = "desc";
    }
}
