using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Categories;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for movie categories/genres and film associations.
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        // Receives the category repository needed for database operations.
        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // Retrieves a paginated and filtered list of movie categories.
        public async Task<PaginatedResponseDto<CategoryResponseDto>> GetAllCategoriesAsync(
            CategoryQueryParametersDto queryParams)
        {
            // Get the base query from the repository.
            var query = _categoryRepository.GetAllCategories();

            // Apply case-insensitive name filter if search term is provided.
            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var s = queryParams.Search.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(s));
            }

            // Apply sorting by name in ascending or descending order.
            query = queryParams.SortField?.ToLower() switch
            {
                "name" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                _ => query.OrderBy(c => c.Name)
            };

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

            // Fetch the current page of category entities.
            var entities = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            // Map each category entity to its response DTO with film count.
            var data = entities.Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                LastUpdate = c.LastUpdate,
                FilmCount = c.FilmCategories.Count
            }).ToList();

            return new PaginatedResponseDto<CategoryResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = data
            };
        }

        // Retrieves category details by ID, including total assigned films count.
        public async Task<CategoryDetailDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null) return null;

            // Convert entity into detailed response DTO.
            return new CategoryDetailDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                LastUpdate = category.LastUpdate,
                FilmCount = category.FilmCategories.Count
            };
        }

        // Validates and saves a new movie category record.
        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            // Map request DTO to database entity.
            var category = new Category
            {
                Name = dto.Name,
                LastUpdate = DateTime.UtcNow
            };

            // Persist the new category via the repository.
            var created = await _categoryRepository.CreateCategoryAsync(category);

            // Return the created category response DTO.
            return new CategoryResponseDto
            {
                CategoryId = created.CategoryId,
                Name = created.Name,
                LastUpdate = created.LastUpdate,
                FilmCount = 0
            };
        }

        // Updates an existing category's name.
        public async Task<CategoryResponseDto?> UpdateCategoryAsync(UpdateCategoryDto dto)
        {
            // Verify that the category exists before attempting update.
            var category = await _categoryRepository.GetCategoryByIdAsync(dto.CategoryId);
            if (category == null) return null;

            category.Name = dto.Name;
            var updated = await _categoryRepository.UpdateCategoryAsync(category);
            if (updated == null) return null;

            return new CategoryResponseDto
            {
                CategoryId = updated.CategoryId,
                Name = updated.Name,
                LastUpdate = updated.LastUpdate,
                FilmCount = updated.FilmCategories.Count
            };
        }

        // Removes a category by its ID through the repository.
        public async Task<bool> DeleteCategoryAsync(int id)
            => await _categoryRepository.DeleteCategoryAsync(id);

        // Retrieves a paginated list of movies belonging to the specified category.
        public async Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByCategoryAsync(
            int categoryId, int page, int pageSize, string? search)
        {
            var query = _categoryRepository.GetFilmsByCategoryId(categoryId);

            // Filter movies by title when search is specified.
            if (!string.IsNullOrEmpty(search))
                query = query.Where(f => f.Title.ToLower().Contains(search.ToLower()));

            var totalRecords = await query.CountAsync();

            // Paginate and project movie entities to movie response DTOs.
            var data = await query
                .OrderBy(f => f.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => new MovieResponseDto
                {
                    FilmId = f.FilmId,
                    Title = f.Title,
                    Description = f.Description,
                    ReleaseYear = f.ReleaseYear,
                    LanguageId = f.LanguageId,
                    LanguageName = f.Language.Name,
                    RentalDuration = f.RentalDuration,
                    RentalRate = f.RentalRate,
                    Length = f.Length,
                    ReplacementCost = f.ReplacementCost,
                    Rating = f.Rating,
                    Categories = f.FilmCategories
                        .Select(fc => fc.Category.Name)
                        .ToList()
                })
                .ToListAsync();

            return new PaginatedResponseDto<MovieResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Data = data
            };
        }
    }
}