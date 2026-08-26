using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Services.Interfaces;

namespace MovieRental.Apis.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Staff,Customer")]
public class LanguageController : ControllerBase
{
    private readonly ILanguageService _languageService;

    public LanguageController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    // GET api/language
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _languageService.GetAllLanguagesAsync();
        return Ok(result);
    }

    // GET api/language/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _languageService.GetLanguageByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // POST api/language
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateLanguageDto dto)
    {
        var result = await _languageService.CreateLanguageAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.LanguageId }, result);
    }

    // PATCH api/language
    [HttpPatch]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateLanguageDto dto)
    {
        var result = await _languageService.UpdateLanguageAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // DELETE api/language/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _languageService.DeleteLanguageAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    // GET api/language/5/detail
    [HttpGet("{id}/detail")]
    public async Task<IActionResult> GetDetail(int id)
    {
        var result = await _languageService.GetLanguageDetailAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    // GET api/language/5/films?page=1&pageSize=10&search=matrix
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

