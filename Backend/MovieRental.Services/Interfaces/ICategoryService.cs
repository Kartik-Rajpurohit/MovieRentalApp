using MovieRental.Domain.DTOs.Categories;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<PaginatedResponseDto<CategoryResponseDto>> GetAllCategoriesAsync(CategoryQueryParametersDto queryParams);
        Task<CategoryDetailDto?> GetCategoryByIdAsync(int id);
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto);
        Task<CategoryResponseDto?> UpdateCategoryAsync(UpdateCategoryDto dto);
        Task<bool> DeleteCategoryAsync(int id);
        Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByCategoryAsync(int categoryId, int page, int pageSize, string? search);
    }
}