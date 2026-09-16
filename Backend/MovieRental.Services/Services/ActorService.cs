using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Interfaces;

namespace MovieRental.Services.Services;

// Handles business logic for actor lookups, CRUD operations, and movie associations.
public class ActorService : IActorService
{
    private readonly IActorRepository _actorRepository;

    // Receives the actor repository needed to access actor data.
    public ActorService(IActorRepository actorRepository)
    {
        _actorRepository = actorRepository;
    }

    // Retrieves actors using the requested search filters, sorting, and pagination.
    public async Task<PaginatedResponseDto<ActorResponseDto>> GetAllActorsAsync(
        PaginationInputDto pagination, ActorFilterDto filter)
    {
        // 1. Get the base query from the repository.
        var query = _actorRepository.GetAllActors();

        // 2. Apply search by actor full name when provided.
        if (!string.IsNullOrWhiteSpace(pagination.Search))
            query = query.Where(a =>
                (a.FirstName + " " + a.LastName).ToLower()
                .Contains(pagination.Search.ToLower()));

        // 3. Apply module filters.
        if (filter.ActorId.HasValue)
            query = query.Where(a => a.ActorId == filter.ActorId.Value);

        // 4. Apply dynamic sorting based on the requested field and direction.
        var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = pagination.SortBy?.ToLower() switch
        {
            "fullname" => isDesc
                ? query.OrderByDescending(a => a.FirstName).ThenByDescending(a => a.LastName)
                : query.OrderBy(a => a.FirstName).ThenBy(a => a.LastName),
            "firstname" => isDesc
                ? query.OrderByDescending(a => a.FirstName)
                : query.OrderBy(a => a.FirstName),
            "lastname" => isDesc
                ? query.OrderByDescending(a => a.LastName)
                : query.OrderBy(a => a.LastName),
            "moviecount" => isDesc
                ? query.OrderByDescending(a => a.MovieActors.Count)
                : query.OrderBy(a => a.MovieActors.Count),
            _ => isDesc
                ? query.OrderByDescending(a => a.ActorId)
                : query.OrderBy(a => a.ActorId)
        };

        // 5. Calculate total count for pagination metadata.
        var totalRecords = await query.CountAsync();

        // 6. Fetch current page data and project to response DTOs.
        var data = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => new ActorResponseDto
            {
                ActorId = a.ActorId,
                FirstName = a.FirstName,
                LastName = a.LastName,
                LastUpdate = a.LastUpdate,
                MovieCount = a.MovieActors.Count
            })
            .ToListAsync();

        return new PaginatedResponseDto<ActorResponseDto>
        {
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize),
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize,
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
            MovieCount = actor.MovieActors.Count
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
            MovieCount = 0
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
            MovieCount = updated.MovieActors.Count
        };
    }

    // Deletes an actor record using the repository.
    public async Task<bool> DeleteActorAsync(int id)
        => await _actorRepository.DeleteActorAsync(id);


    // Retrieves a paginated list of movies starring the specified actor.
    public async Task<PaginatedResponseDto<MovieResponseDto>> GetMoviesByActorAsync(int actorId, PaginationInputDto pagination)
    {
        var query = _actorRepository.GetMoviesByActorId(actorId);

        // Apply search filter if specified.
        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var search = pagination.Search.Trim().ToLower();
            query = query.Where(m => m.Title.ToLower().Contains(search) || (m.Description != null && m.Description.ToLower().Contains(search)));
        }

        // Apply sorting
        var isDesc = string.Equals(pagination.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        query = pagination.SortBy?.ToLower() switch
        {
            "title" => isDesc ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
            "releaseyear" => isDesc ? query.OrderByDescending(m => m.ReleaseYear) : query.OrderBy(m => m.ReleaseYear),
            "rentalrate" => isDesc ? query.OrderByDescending(m => m.RentalRate) : query.OrderBy(m => m.RentalRate),
            "length" => isDesc ? query.OrderByDescending(m => m.Length) : query.OrderBy(m => m.Length),
            "id" or "movieid" => isDesc ? query.OrderByDescending(m => m.MovieId) : query.OrderBy(m => m.MovieId),
            _ => isDesc ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title)
        };

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalRecords / pagination.PageSize);

        // Paginate and project movie entities to response DTOs.
        var data = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(m => new MovieResponseDto
            {
                MovieId = m.MovieId,
                Title = m.Title,
                Description = m.Description,
                ReleaseYear = m.ReleaseYear,
                LanguageId = m.LanguageId,
                LanguageName = m.Language.Name,
                RentalDuration = m.RentalDuration,
                RentalRate = m.RentalRate,
                Length = m.Length,
                ReplacementCost = m.ReplacementCost,
                Rating = m.Rating,
                Categories = m.MovieCategories
                    .Select(mc => mc.Category.Name)
                    .ToList()
            })
            .ToListAsync();

        return new PaginatedResponseDto<MovieResponseDto>
        {
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            CurrentPage = pagination.Page,
            PageSize = pagination.PageSize,
            Data = data
        };
    }
}