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
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<PaginatedResponseDto<CategoryResponseDto>> GetAllCategoriesAsync(
            CategoryQueryParametersDto queryParams)
        {
            var query = _categoryRepository.GetAllCategories();

            if (!string.IsNullOrEmpty(queryParams.Search))
            {
                var s = queryParams.Search.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(s));
            }

            query = queryParams.SortField?.ToLower() switch
            {
                "name" => queryParams.SortOrder?.ToLower() == "desc"
                    ? query.OrderByDescending(c => c.Name)
                    : query.OrderBy(c => c.Name),
                _ => query.OrderBy(c => c.Name)
            };

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

            var entities = await query
                .Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

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

        // Sirf category info — films alag call se aayenge
        public async Task<CategoryDetailDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetCategoryByIdAsync(id);
            if (category == null) return null;

            return new CategoryDetailDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                LastUpdate = category.LastUpdate,
                FilmCount = category.FilmCategories.Count
            };
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                LastUpdate = DateTime.UtcNow
            };
            var created = await _categoryRepository.CreateCategoryAsync(category);
            return new CategoryResponseDto
            {
                CategoryId = created.CategoryId,
                Name = created.Name,
                LastUpdate = created.LastUpdate,
                FilmCount = 0
            };
        }

        public async Task<CategoryResponseDto?> UpdateCategoryAsync(UpdateCategoryDto dto)
        {
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

        public async Task<bool> DeleteCategoryAsync(int id)
            => await _categoryRepository.DeleteCategoryAsync(id);

        // Actor pattern jaisa — paginated films
        public async Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByCategoryAsync(
            int categoryId, int page, int pageSize, string? search)
        {
            var query = _categoryRepository.GetFilmsByCategoryId(categoryId);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(f => f.Title.ToLower().Contains(search.ToLower()));

            var totalRecords = await query.CountAsync();

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