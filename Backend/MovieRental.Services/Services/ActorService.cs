using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

public class ActorService : IActorService
{
    private readonly IActorRepository _actorRepository;

    public ActorService(IActorRepository actorRepository)
    {
        _actorRepository = actorRepository;
    }

    public async Task<PaginatedResponseDto<ActorResponseDto>> GetAllActorsAsync(ActorQueryParametersDto queryParams)
    {
        var query = _actorRepository.GetAllActors();

        // Search
        if (!string.IsNullOrEmpty(queryParams.Search))
            query = query.Where(a =>
                (a.FirstName + " " + a.LastName).ToLower()
                .Contains(queryParams.Search.ToLower()));

        // Sorting
        query = queryParams.SortField?.ToLower() switch
        {
            "firstname" => queryParams.SortOrder == "desc"
                ? query.OrderByDescending(a => a.FirstName)
                : query.OrderBy(a => a.FirstName),
            "lastname" => queryParams.SortOrder == "desc"
                ? query.OrderByDescending(a => a.LastName)
                : query.OrderBy(a => a.LastName),
            "filmcount" => queryParams.SortOrder == "desc"
                ? query.OrderByDescending(a => a.FilmActors.Count)
                : query.OrderBy(a => a.FilmActors.Count),
            _ => query.OrderBy(a => a.ActorId)
        };

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

        var data = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(a => new ActorResponseDto
            {
                ActorId = a.ActorId,
                FirstName = a.FirstName,
                LastName = a.LastName,
                LastUpdate = a.LastUpdate,
                FilmCount = a.FilmActors.Count
            })
            .ToListAsync();

        return new PaginatedResponseDto<ActorResponseDto>
        {
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            CurrentPage = queryParams.Page,
            PageSize = queryParams.PageSize,
            Data = data
        };
    }

    public async Task<ActorResponseDto?> GetActorByIdAsync(int id)
    {
        var actor = await _actorRepository.GetActorByIdAsync(id);
        if (actor == null) return null;

        return new ActorResponseDto
        {
            ActorId = actor.ActorId,
            FirstName = actor.FirstName,
            LastName = actor.LastName,
            LastUpdate = actor.LastUpdate,
            FilmCount = actor.FilmActors.Count
        };
    }

    public async Task<ActorResponseDto> CreateActorAsync(CreateActorDto dto)
    {
        var actor = new Actor
        {
            FirstName = dto.FirstName.ToUpper(),
            LastName = dto.LastName.ToUpper(),
            LastUpdate = DateTime.UtcNow
        };

        var created = await _actorRepository.CreateActorAsync(actor);
        return new ActorResponseDto
        {
            ActorId = created.ActorId,
            FirstName = created.FirstName,
            LastName = created.LastName,
            LastUpdate = created.LastUpdate,
            FilmCount = 0
        };
    }

    public async Task<ActorResponseDto?> UpdateActorAsync(UpdateActorDto dto)
    {
        var actor = new Actor
        {
            ActorId = dto.ActorId,
            FirstName = dto.FirstName.ToUpper(),
            LastName = dto.LastName.ToUpper()
        };

        var updated = await _actorRepository.UpdateActorAsync(actor);
        if (updated == null) return null;

        return new ActorResponseDto
        {
            ActorId = updated.ActorId,
            FirstName = updated.FirstName,
            LastName = updated.LastName,
            LastUpdate = updated.LastUpdate,
            FilmCount = updated.FilmActors.Count
        };
    }

    public async Task<bool> DeleteActorAsync(int id)
        => await _actorRepository.DeleteActorAsync(id);

    public async Task<ActorDetailDto?> GetActorDetailAsync(int id)
    {
        var actor = await _actorRepository.GetActorByIdAsync(id);
        if (actor == null) return null;

        return new ActorDetailDto
        {
            ActorId = actor.ActorId,
            FirstName = actor.FirstName,
            LastName = actor.LastName,
            LastUpdate = actor.LastUpdate,
            FilmCount = actor.FilmActors.Count
        };
    }

    public async Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByActorAsync(int actorId, int page, int pageSize, string? search)
    {
        var query = _actorRepository.GetFilmsByActorId(actorId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(f => f.Title.ToLower().Contains(search.ToLower()));

        var totalRecords = await query.CountAsync();

        var data = await query
            .OrderBy(f => f.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new MovieResponseDto
            {
                FilmId = f.FilmId,
                Title = f.Title,
                Description = f.Description,
                ReleaseYear = f.ReleaseYear,
                LanguageId = f.LanguageId,
                LanguageName = f.Language.Name,        // EF Core auto-join karega
                RentalDuration = f.RentalDuration,
                RentalRate = f.RentalRate,
                Length = f.Length,
                ReplacementCost = f.ReplacementCost,
                Rating = f.Rating,
                Categories = f.FilmCategories
                    .Select(fc => fc.Category.Name)    // EF Core auto-join karega
                    .ToList()
            })
            .ToListAsync();

        return new PaginatedResponseDto<MovieResponseDto>
        {
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
            CurrentPage = page,
            PageSize = pageSize,
            Data = data
        };
    }
}