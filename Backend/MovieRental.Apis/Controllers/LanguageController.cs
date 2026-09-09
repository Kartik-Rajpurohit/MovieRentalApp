using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

// Handles movie language options and language lookup data.
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff,Customer")] // All users can read languages; Admin can modify
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
    // Restricted to Admin role only.
    // Returns 201 Created with the new language ID and location header.
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateLanguageDto dto)
    {
        var result = await _languageService.CreateLanguageAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.LanguageId }, result);
    }

    // Updates an existing language's name.
    // Restricted to Admin role only.
    // Returns 404 NotFound if language does not exist.
    [HttpPatch]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateLanguageDto dto)
    {
        var result = await _languageService.UpdateLanguageAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Deletes a language by ID.
    // Restricted to Admin role only.
    // Returns 204 NoContent on success, or 404 NotFound if language does not exist.
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _languageService.DeleteLanguageAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // Gets detailed language information including total films available in this language.
    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var result = await _languageService.GetLanguageDetailAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // Gets a paginated list of films released in this language.
    // Supports optional title search within the language.
    [HttpGet("{id}/films")]
    public async Task<IActionResult> GetFilms(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _languageService.GetFilmsByLanguageAsync(id, page, pageSize, search);
        return Ok(result);
    }
}

