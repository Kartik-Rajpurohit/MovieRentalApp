using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Categories;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles movie genres/categories and their film counts.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff,Customer")] // All users can browse categories; only Admin and Staff can modify
    public class CategoryController : ControllerBase
    {
        // Injected service for category business logic
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // Gets a paginated list of categories with film counts.
        // Query parameters: page, pageSize, search.
        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] CategoryQueryParametersDto queryParams)
        {
            var result = await _categoryService.GetAllCategoriesAsync(queryParams);
            return Ok(result);
        }

        // Gets category details and associated movies by CategoryId.
        // Returns 404 NotFound if category does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (result == null) return NotFound($"Category with id {id} not found");
            return Ok(result);
        }

        // Creates a new film category/genre.
        // Restricted to Admin and Staff roles.
        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateCategoryAsync(dto);
            return Ok(result);
        }

        // Updates an existing category's name.
        // Returns 404 NotFound if the category is not found.
        [HttpPatch]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateCategoryAsync(dto);
            if (result == null) return NotFound($"Category with id {dto.CategoryId} not found");
            return Ok(result);
        }

        // Deletes a category by ID.
        // Returns 204 NoContent on success, or 404 NotFound if category does not exist.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result) return NotFound($"Category with id {id} not found");
            return NoContent();
        }

        // Gets a paginated list of films belonging to this category.
        // Supports optional title search within the genre.
        [HttpGet("{id}/films")]
        public async Task<IActionResult> GetFilms(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _categoryService.GetFilmsByCategoryAsync(id, page, pageSize, search);
            return Ok(result);
        }
    }
}