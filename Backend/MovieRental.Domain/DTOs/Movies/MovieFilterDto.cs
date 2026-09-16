namespace MovieRental.Domain.DTOs.Movies
{
    // Module-specific filter parameters for movie catalog listings
    public class MovieFilterDto
    {
        // Filters movies by primary language ID
        public int? LanguageId { get; set; }

        // Filters movies belonging to a specific genre/category ID
        public int? CategoryId { get; set; }

        // Filters movies by MPAA rating (G, PG, PG-13, R, NC-17)
        public string? Rating { get; set; }

        // Filters movies by release year
        public int? ReleaseYear { get; set; }

        // Filters movies by minimum rental rate
        public decimal? MinRentalRate { get; set; }

        // Filters movies by maximum rental rate
        public decimal? MaxRentalRate { get; set; }

        // Filters movies by minimum runtime length in minutes
        public int? MinLength { get; set; }

        // Filters movies by maximum runtime length in minutes
        public int? MaxLength { get; set; }
    }
}
