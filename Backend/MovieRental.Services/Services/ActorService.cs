using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Domain.QueryParameters;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

// Handles business logic for actor lookups, CRUD operations, and filmography associations.
public class ActorService : IActorService
{
    private readonly IActorRepository _actorRepository;

    // Receives the actor repository needed to access actor data.
    public ActorService(IActorRepository actorRepository)
    {
        _actorRepository = actorRepository;
    }

    // Retrieves actors using the requested search filters, sorting, and pagination.
    public async Task<PaginatedResponseDto<ActorResponseDto>> GetAllActorsAsync(ActorQueryParametersDto queryParams)
    {
        // Get the base query from the repository.
        var query = _actorRepository.GetAllActors();

        // Apply case-insensitive search by actor full name when provided.
        if (!string.IsNullOrEmpty(queryParams.Search))
            query = query.Where(a =>
                (a.FirstName + " " + a.LastName).ToLower()
                .Contains(queryParams.Search.ToLower()));

        // Apply dynamic sorting based on the requested field and direction.
        query = queryParams.SortField?.ToLower() switch
        {
            "fullname" => queryParams.SortOrder == "desc"
                ? query.OrderByDescending(a => a.FirstName).ThenByDescending(a => a.LastName)
                : query.OrderBy(a => a.FirstName).ThenBy(a => a.LastName),
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

        // Calculate total count for pagination metadata.
        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalRecords / queryParams.PageSize);

        // Fetch the current page data and map entities to response DTOs.
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

    // Retrieves a single actor by ID and maps to a response DTO.
    public async Task<ActorResponseDto?> GetActorByIdAsync(int id)
    {
        var actor = await _actorRepository.GetActorByIdAsync(id);
        if (actor == null) return null;

        // Convert the database entity into the response DTO.
        return new ActorResponseDto
        {
            ActorId = actor.ActorId,
            FirstName = actor.FirstName,
            LastName = actor.LastName,
            LastUpdate = actor.LastUpdate,
            FilmCount = actor.FilmActors.Count
        };
    }

    // Normalizes actor names to uppercase and saves a new actor record.
    public async Task<ActorResponseDto> CreateActorAsync(CreateActorDto dto)
    {
        // Map request DTO to database entity with uppercase names.
        var actor = new Actor
        {
            FirstName = dto.FirstName.ToUpper(),
            LastName = dto.LastName.ToUpper(),
            LastUpdate = DateTime.UtcNow
        };

        // Persist the new actor in the database.
        var created = await _actorRepository.CreateActorAsync(actor);

        // Map the saved entity to a response DTO.
        return new ActorResponseDto
        {
            ActorId = created.ActorId,
            FirstName = created.FirstName,
            LastName = created.LastName,
            LastUpdate = created.LastUpdate,
            FilmCount = 0
        };
    }

    // Updates an existing actor's name and saves the changes.
    public async Task<ActorResponseDto?> UpdateActorAsync(UpdateActorDto dto)
    {
        var actor = new Actor
        {
            ActorId = dto.ActorId,
            FirstName = dto.FirstName.ToUpper(),
            LastName = dto.LastName.ToUpper()
        };

        // Save updates through the repository.
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

    // Deletes an actor record using the repository.
    public async Task<bool> DeleteActorAsync(int id)
        => await _actorRepository.DeleteActorAsync(id);

    // Retrieves detailed actor information including film count.
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

    // Retrieves a paginated list of movies starring the specified actor.
    public async Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByActorAsync(int actorId, int page, int pageSize, string? search)
    {
        var query = _actorRepository.GetFilmsByActorId(actorId);

        // Apply title search filter if specified.
        if (!string.IsNullOrEmpty(search))
            query = query.Where(f => f.Title.ToLower().Contains(search.ToLower()));

        var totalRecords = await query.CountAsync();

        // Paginate and project movie entities to response DTOs.
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
                LanguageName = f.Language.Name,
                RentalDuration = f.RentalDuration,
                RentalRate = f.RentalRate,
                Length = f.Length,
                ReplacementCost = f.ReplacementCost,
                Rating = f.Rating,
                Categories = f.FilmCategories
                    .Select(fc => fc.Category.Name)
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