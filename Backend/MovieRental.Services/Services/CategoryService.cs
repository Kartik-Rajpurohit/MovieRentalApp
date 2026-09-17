using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Categories;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Extensions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services
{
    // Handles business logic for movie categories/genres and movie associations.
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
            PaginationInputDto pagination,
            CategoryFilterDto filter)
        {
            // Get the base query from the repository.
            var query = _categoryRepository.GetAllCategories();

            // Apply search filter if provided.
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var s = pagination.Search.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(s));
            }

            // Apply sorting
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "name" => isDesc
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                "id" or "categoryid" => isDesc
                    ? query.OrderByDescending(c => c.CategoryId)
                    : query.OrderBy(c => c.CategoryId),
                _ => isDesc
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name)
            };

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // Fetch the current page of category entities.
            var entities = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            // Map each category entity to its response DTO with movie count.
            var data = entities.Select(c => c.ToResponseDto()).ToList();

            return new PaginatedResponseDto<CategoryResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                Data = data
            };
        }

        // Retrieves category details by ID, including total assigned movies count.
        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null) return null;

            // Convert entity into response DTO.
            return category.ToResponseDto();
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
            return created.ToResponseDto();
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

            return updated.ToResponseDto();
        }

        // Removes a category by its ID through the repository.
        public async Task<bool> DeleteCategoryAsync(int id)
            => await _categoryRepository.DeleteCategoryAsync(id);

        // Retrieves a paginated list of movies belonging to the specified category.
        public async Task<PaginatedResponseDto<MovieResponseDto>> GetMoviesByCategoryAsync(
            int categoryId,
            PaginationInputDto pagination)
        {
            var query = _categoryRepository.GetMoviesByCategoryId(categoryId);

            // Filter movies by search term when specified.
            if (!string.IsNullOrWhiteSpace(pagination.Search))
            {
                var s = pagination.Search.Trim().ToLower();
                query = query.Where(m => m.Title.ToLower().Contains(s) || (m.Description != null && m.Description.ToLower().Contains(s)));
            }

            // Apply sorting
            var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
            query = pagination.SortBy?.ToLower() switch
            {
                "title" => isDesc ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
                "releaseyear" => isDesc ? query.OrderByDescending(m => m.ReleaseYear) : query.OrderBy(m => m.ReleaseYear),
                "rentalrate" => isDesc ? query.OrderByDescending(m => m.RentalRate) : query.OrderBy(m => m.RentalRate),
                "length" => isDesc ? query.OrderByDescending(m => m.Length) : query.OrderBy(m => m.Length),
                "id" or "movieid" => isDesc ? query.OrderByDescending(m => m.MovieId) : query.OrderBy(m => m.MovieId),
                _ => isDesc ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title)
            };

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

            // Paginate and project movie entities to movie response DTOs.
            var data = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectToMovieResponseDto()
                .ToListAsync();

            return new PaginatedResponseDto<MovieResponseDto>
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pagination.Page,
                PageSize = pagination.PageSize,
                Data = data
            };
        }
    }
}