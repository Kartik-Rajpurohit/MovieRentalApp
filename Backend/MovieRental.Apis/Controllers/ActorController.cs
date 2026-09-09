using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

// Handles actor management and movie filmography queries.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff,Customer")] // All authenticated users can read actors; write operations require Admin/Staff
public class ActorController : ControllerBase
{
    // Injected service for actor business logic and database queries
    private readonly IActorService _actorService;

    public ActorController(IActorService actorService)
    {
        _actorService = actorService;
    }

    // Gets a paginated, sorted, and filtered list of actors.
    // Query parameters: page, pageSize, search text, sort field, and sort order.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ActorQueryParametersDto queryParams)
    {
        var result = await _actorService.GetAllActorsAsync(queryParams);
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
    // Restricted to Admin and Staff; customers have read-only access.
    // Returns 201 Created with the new actor's ID and location header.
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateActorDto dto)
    {
        var result = await _actorService.CreateActorAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.ActorId }, result);
    }

    // Updates an existing actor's name.
    // Receives UpdateActorDto with ActorId and new names.
    // Returns 404 NotFound if actor does not exist.
    [HttpPatch]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update([FromBody] UpdateActorDto dto)
    {
        var result = await _actorService.UpdateActorAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Deletes an actor by ID.
    // Returns 204 NoContent on success, or 404 NotFound if the actor does not exist.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _actorService.DeleteActorAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // Gets detailed actor info including film count and filmography summary.
    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var result = await _actorService.GetActorDetailAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Gets a paginated list of films featuring this actor.
    // Supports optional title search within the actor's filmography.
    [HttpGet("{id}/films")]
    public async Task<IActionResult> GetFilms(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _actorService.GetFilmsByActorAsync(id, page, pageSize, search);
        return Ok(result);
    }
}