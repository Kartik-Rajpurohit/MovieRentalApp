using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.QueryParameters;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff,Customer")] 
public class ActorController : ControllerBase
{
    private readonly IActorService _actorService;

    public ActorController(IActorService actorService)
    {
        _actorService = actorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ActorQueryParametersDto queryParams)
    {
        var result = await _actorService.GetAllActorsAsync(queryParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _actorService.GetActorByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateActorDto dto)
    {
        var result = await _actorService.CreateActorAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.ActorId }, result);
    }

    [HttpPatch]
    public async Task<IActionResult> Update([FromBody] UpdateActorDto dto)
    {
        var result = await _actorService.UpdateActorAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _actorService.DeleteActorAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var result = await _actorService.GetActorDetailAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}/films")]
    public async Task<IActionResult> GetFilms(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        var result = await _actorService.GetFilmsByActorAsync(id, page, pageSize, search);
        return Ok(result);
    }
}