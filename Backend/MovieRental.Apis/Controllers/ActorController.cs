using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

// Handles actor management and movie queries.
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Actors.Read)] // All authenticated users can read actors; write operations require Admin/Staff
public class ActorController : ControllerBase
{
    // Injected service for actor business logic and database queries
    private readonly IActorService _actorService;

    public ActorController(IActorService actorService)
    {
        _actorService = actorService;
    }

    // Gets a paginated, sorted, and filtered list of actors.
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationInputDto pagination,
        [FromQuery] ActorFilterDto filter)
    {
        var result = await _actorService.GetAllActorsAsync(pagination, filter);
        return Ok(result);
    }

    // Gets single actor profile by ActorId.
    // Returns 404 NotFound if the actor does not exist.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _actorService.GetActorByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Adds a new actor to the database.
    // Restricted to Actors.Create permission.
    // Returns 201 Created with the new actor's ID and location header.
    [HttpPost]
    [Authorize(Policy = Permissions.Actors.Create)]
    public async Task<IActionResult> Create([FromBody] CreateActorDto dto)
    {
        var result = await _actorService.CreateActorAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.ActorId }, result);
    }

    // Updates an existing actor's name.
    // Receives UpdateActorDto with ActorId and new names.
    // Returns 404 NotFound if actor does not exist.
    [HttpPatch]
    [Authorize(Policy = Permissions.Actors.Update)]
    public async Task<IActionResult> Update([FromBody] UpdateActorDto dto)
    {
        var result = await _actorService.UpdateActorAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Deletes an actor by ID.
    // Returns 204 NoContent on success, or 404 NotFound if the actor does not exist.
    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Actors.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _actorService.DeleteActorAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }


    // Gets a paginated list of movies featuring this actor.
    [HttpGet("{id}/movies")]
    public async Task<IActionResult> GetMovies(int id, [FromQuery] PaginationInputDto pagination)
    {
        var result = await _actorService.GetMoviesByActorAsync(id, pagination);
        return Ok(result);
    }
}