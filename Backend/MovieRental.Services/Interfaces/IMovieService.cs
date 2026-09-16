using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for movies, including search, details, and lookup options.
    public interface IMovieService
    {
        // Retrieves a paginated, filtered, and sorted catalog of movies.
        Task<PaginatedResponseDto<MovieResponseDto>> GetAllMoviesAsync(PaginationInputDto pagination, MovieFilterDto filter);

        // Retrieves detailed movie information by ID, including actors and categories.
        Task<MovieDetailDto?> GetMovieByIdAsync(int id);

        // Validates and creates a new movie with related actors and categories.
        Task<MovieResponseDto> CreateMovieAsync(CreateMovieDto dto);

        // Updates an existing movie and synchronizes related actors/categories.
        Task<MovieResponseDto?> UpdateMovieAsync(UpdateMovieDto dto);

        // Deletes a movie record by ID.
        Task<bool> DeleteMovieAsync(int id);

        // Retrieves languages formatted as dropdown options for movie forms.
        Task<IEnumerable<DropdownDto>> GetAllLanguagesAsync(int page, int pageSize);

        // Retrieves categories formatted as dropdown options for movie forms.
        Task<IEnumerable<DropdownDto>> GetAllCategoriesAsync(int page, int pageSize);

        // Retrieves actors formatted as dropdown options for movie forms.
        Task<IEnumerable<DropdownDto>> GetAllActorsAsync(int page, int pageSize);
    }
}
