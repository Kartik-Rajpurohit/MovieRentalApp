using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for movies, including search, details, and lookup options.
    public interface IFilmService
    {
        // Retrieves a paginated and filtered catalog of movies.
        Task<PaginatedResponseDto<MovieResponseDto>> GetAllFilmsAsync(MovieQueryParametersDto queryParams);

        // Retrieves detailed movie information by ID, including actors and categories.
        Task<MovieDetailDto?> GetFilmByIdAsync(int id);

        // Validates and creates a new movie with related actors and categories.
        Task<MovieResponseDto> CreateFilmAsync(CreateMovieDto dto);

        // Updates an existing movie and synchronizes related actors/categories.
        Task<MovieResponseDto?> UpdateFilmAsync(UpdateMovieDto dto);

        // Deletes a movie record by ID.
        Task<bool> DeleteFilmAsync(int id);

        // Retrieves languages formatted as dropdown options for movie forms.
        Task<IEnumerable<DropdownDto>> GetAllLanguagesAsync(int page, int pageSize);

        // Retrieves categories formatted as dropdown options for movie forms.
        Task<IEnumerable<DropdownDto>> GetAllCategoriesAsync(int page, int pageSize);

        // Retrieves actors formatted as dropdown options for movie forms.
        Task<IEnumerable<DropdownDto>> GetAllActorsAsync(int page, int pageSize);
    }
}
