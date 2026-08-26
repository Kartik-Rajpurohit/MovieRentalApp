using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Categories;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff,Customer")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET api/category — paginated list with film count
        [HttpGet]
        public async Task<IActionResult> GetAllCategories(
            [FromQuery] CategoryQueryParametersDto queryParams)
        {
            var result = await _categoryService.GetAllCategoriesAsync(queryParams);
            return Ok(result);
        }

        // GET api/category/{id} — detail with list of movies
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var result = await _categoryService.GetCategoryByIdAsync(id);
            if (result == null) return NotFound($"Category with id {id} not found");
            return Ok(result);
        }

        // POST api/category — create new category
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateCategoryAsync(dto);
            return Ok(result);
        }

        // PATCH api/category — partial update
        [HttpPatch]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateCategoryAsync(dto);
            if (result == null) return NotFound($"Category with id {dto.CategoryId} not found");
            return Ok(result);
        }

        // DELETE api/category/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategoryAsync(id);
            if (!result) return NotFound($"Category with id {id} not found");
            return NoContent();
        }
        // GET api/category/{id}/films — paginated films
        [HttpGet("{id}/films")]
        public async Task<IActionResult> GetFilms(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var result = await _categoryService.GetFilmsByCategoryAsync(id, page, pageSize, search);
            return Ok(result);
        }
    }
}