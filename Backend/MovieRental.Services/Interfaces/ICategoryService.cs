using MovieRental.Domain.DTOs.Categories;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces
{
    // Defines business operations for managing movie genres/categories.
    public interface ICategoryService
    {
        // Retrieves a paginated and filtered list of categories with movie counts.
        Task<PaginatedResponseDto<CategoryResponseDto>> GetAllCategoriesAsync(CategoryQueryParametersDto queryParams);

        // Retrieves a category by ID with its list of associated movies.
        Task<CategoryDetailDto?> GetCategoryByIdAsync(int id);

        // Adds a new movie genre category to the system.
        Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto);

        // Updates an existing category's name.
        Task<CategoryResponseDto?> UpdateCategoryAsync(UpdateCategoryDto dto);

        // Deletes a category by ID.
        Task<bool> DeleteCategoryAsync(int id);

        // Retrieves a paginated list of movies belonging to the specified category.
        Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByCategoryAsync(int categoryId, int page, int pageSize, string? search);
    }
}