using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles movie catalog queries, creation, updates, and associated lookup data.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff,Customer")] // All users can browse the movie catalog; write actions require Admin
    public class MovieController : ControllerBase
    {
        // Injected service handling movie database queries, joins, and mutations
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // Gets a paginated, filtered, and sorted list of movies.
        // Query parameters: page, pageSize, search, languageId, categoryId, rating, releaseYear, rentalRate, and length.
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] MovieQueryParametersDto queryParams)
        {
            var result = await _movieService.GetAllMoviesAsync(queryParams);
            return Ok(result);
        }

        // Gets full movie details by MovieId (including categories, actors, and rental statistics).
        // Returns 404 NotFound if the movie does not exist.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _movieService.GetMovieByIdAsync(id);
            return result is null ? NotFound($"Movie with id {id} not found") : Ok(result);
        }

        // Creates a new movie with category and actor associations.
        // Restricted to Admin role.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateMovieDto dto)
        {
            var result = await _movieService.CreateMovieAsync(dto);
            return Ok(result);
        }

        // Partially updates an existing movie (only fields provided in the request body are modified).
        // Restricted to Admin role.
        // Returns 404 NotFound if the movie does not exist.
        [HttpPatch]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromBody] UpdateMovieDto dto)
        {
            var result = await _movieService.UpdateMovieAsync(dto);
            return result is null ? NotFound($"Movie with id {dto.MovieId} not found") : Ok(result);
        }

        // Deletes a movie by ID.
        // Restricted to Admin role.
        // Returns 200 OK on success, or 404 NotFound if movie does not exist.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _movieService.DeleteMovieAsync(id);
            return deleted ? Ok() : NotFound($"Movie with id {id} not found");
        }

        // Gets a list of languages for dropdown selection in the Add/Edit Movie form.
        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            var result = await _movieService.GetAllLanguagesAsync(page, pageSize);
            return Ok(result);
        }

        // Gets a list of categories/genres for multi-select in the Add/Edit Movie form.
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            var result = await _movieService.GetAllCategoriesAsync(page, pageSize);
            return Ok(result);
        }

        // Gets a list of actors for multi-select in the Add/Edit Movie form.
        [HttpGet("actors")]
        public async Task<IActionResult> GetActors(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 100)
        {
            var result = await _movieService.GetAllActorsAsync(page, pageSize);
            return Ok(result);
        }
    }
}
