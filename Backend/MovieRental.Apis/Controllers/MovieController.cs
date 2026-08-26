using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff,Customer")] // Admin, Staff, and Customer can access movie endpoints
    public class MovieController : ControllerBase
    {
        private readonly IFilmService _filmService;

        public MovieController(IFilmService filmService)
        {
            _filmService = filmService;
        }

        // GET api/movie — returns paginated, filtered, sorted list of films
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] MovieQueryParametersDto queryParams)
        {
            var result = await _filmService.GetAllFilmsAsync(queryParams);
            return Ok(result);
        }

        // GET api/movie/{id} — returns full film detail
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _filmService.GetFilmByIdAsync(id);
            return result is null ? NotFound($"Film with id {id} not found") : Ok(result);
        }

        // POST api/movie — creates a new film with category and actor links
        [HttpPost]
        [Authorize(Roles = "Admin")] // Only Admin can create films
        public async Task<IActionResult> Create([FromBody] CreateMovieDto dto)
        {
            var result = await _filmService.CreateFilmAsync(dto);
            return Ok(result);
        }

        // PATCH api/movie — partial update, only sent fields are updated
        [HttpPatch]
        [Authorize(Roles = "Admin")] // Only Admin can update films
        public async Task<IActionResult> Update([FromBody] UpdateMovieDto dto)
        {
            var result = await _filmService.UpdateFilmAsync(dto);
            return result is null ? NotFound($"Film with id {dto.FilmId} not found") : Ok(result);
        }

        // DELETE api/movie/{id} — hard delete
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Only Admin can delete films
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _filmService.DeleteFilmAsync(id);
            return deleted ? Ok() : NotFound($"Film with id {id} not found");
        }

        // GET api/movie/languages — for language dropdown in Add/Edit form
        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            var result = await _filmService.GetAllLanguagesAsync(page, pageSize);
            return Ok(result);
        }

        // GET api/movie/categories — for category multiselect in Add/Edit form
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            var result = await _filmService.GetAllCategoriesAsync(page, pageSize);
            return Ok(result);
        }

        // GET api/movie/actors — for actor multiselect in Add/Edit form
        [HttpGet("actors")]
        public async Task<IActionResult> GetActors(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            var result = await _filmService.GetAllActorsAsync(page, pageSize);
            return Ok(result);
        }
    }
}
