using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers
{
    // Handles movie catalog queries, creation, updates, and associated lookup data.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Permissions.Movies.Read)] // All authorized users can browse the movie catalog; write actions require Admin
    public class MovieController : ControllerBase
    {
        // Injected service handling movie catalog operations
        private readonly IMovieService _movieService;

        public MovieController(IMovieService movieService)
        {
            _movieService = movieService;
        }

        // Gets a paginated, filtered, and sorted list of movies.
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] PaginationInputDto pagination,
            [FromQuery] MovieFilterDto filter)
        {
            var result = await _movieService.GetAllMoviesAsync(pagination, filter);
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
        // Restricted to Movies.Create permission.
        [HttpPost]
        [Authorize(Policy = Permissions.Movies.Create)]
        public async Task<IActionResult> Create([FromBody] CreateMovieDto dto)
        {
            var result = await _movieService.CreateMovieAsync(dto);
            return Ok(result);
        }

        // Partially updates an existing movie (only fields provided in the request body are modified).
        // Restricted to Movies.Update permission.
        // Returns 404 NotFound if the movie does not exist.
        [HttpPatch]
        [Authorize(Policy = Permissions.Movies.Update)]
        public async Task<IActionResult> Update([FromBody] UpdateMovieDto dto)
        {
            var result = await _movieService.UpdateMovieAsync(dto);
            return result is null ? NotFound($"Movie with id {dto.MovieId} not found") : Ok(result);
        }

        // Deletes a movie by ID.
        // Restricted to Movies.Delete permission.
        // Returns 200 OK on success, or 404 NotFound if movie does not exist.
        [HttpDelete("{id}")]
        [Authorize(Policy = Permissions.Movies.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _movieService.DeleteMovieAsync(id);
            return deleted ? Ok() : NotFound($"Movie with id {id} not found");
        }
    }
}
