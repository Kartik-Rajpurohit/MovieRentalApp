using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    public interface IFilmService
    {
        Task<PaginatedResponseDto<MovieResponseDto>> GetAllFilmsAsync(MovieQueryParametersDto queryParams);
        Task<MovieDetailDto?> GetFilmByIdAsync(int id);
        Task<MovieResponseDto> CreateFilmAsync(CreateMovieDto dto);
        Task<MovieResponseDto?> UpdateFilmAsync(UpdateMovieDto dto);
        Task<bool> DeleteFilmAsync(int id);

        // Dropdowns for add/edit form
        Task<IEnumerable<DropdownDto>> GetAllLanguagesAsync(int page, int pageSize);
        Task<IEnumerable<DropdownDto>> GetAllCategoriesAsync(int page, int pageSize);
        Task<IEnumerable<DropdownDto>> GetAllActorsAsync(int page, int pageSize);
    }
}
