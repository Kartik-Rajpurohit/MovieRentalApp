using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Repository.Permissions;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

// Handles movie language options and language lookup data.
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Languages.Read)] // All users can read languages; Admin can modify
public class LanguageController : ControllerBase
{
    // Injected service for language operations
    private readonly ILanguageService _languageService;

    public LanguageController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    // Gets all languages available for movie cataloging.
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _languageService.GetAllLanguagesAsync();
        return Ok(result);
    }

    // Gets a language by LanguageId.
    // Returns 404 NotFound if language does not exist.
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _languageService.GetLanguageByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Creates a new language.
    // Restricted to Languages.Create permission.
    // Returns 201 Created with the new language ID and location header.
    [HttpPost]
    [Authorize(Policy = Permissions.Languages.Create)]
    public async Task<IActionResult> Create([FromBody] CreateLanguageDto dto)
    {
        var result = await _languageService.CreateLanguageAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.LanguageId }, result);
    }

    // Updates an existing language's name.
    // Restricted to Languages.Update permission.
    // Returns 404 NotFound if language does not exist.
    [HttpPatch]
    [Authorize(Policy = Permissions.Languages.Update)]
    public async Task<IActionResult> Update([FromBody] UpdateLanguageDto dto)
    {
        var result = await _languageService.UpdateLanguageAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Deletes a language by ID.
    // Restricted to Languages.Delete permission.
    // Returns 204 NoContent on success, or 404 NotFound if language does not exist.
    [HttpDelete("{id}")]
    [Authorize(Policy = Permissions.Languages.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _languageService.DeleteLanguageAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }


    // Gets a paginated list of movies released in this language.
    // Supports optional title search within the language.
    [HttpGet("{id}/movies")]
    public async Task<IActionResult> GetMovies(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _languageService.GetMoviesByLanguageAsync(id, page, pageSize, search);
        return Ok(result);
    }
}

