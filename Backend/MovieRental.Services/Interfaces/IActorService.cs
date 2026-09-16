using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces;

// Defines business operations for managing actors and their movies.
public interface IActorService
{
    // Retrieves a paginated, filtered, and sorted list of actors.
    Task<PaginatedResponseDto<ActorResponseDto>> GetAllActorsAsync(ActorQueryParametersDto queryParams);

    // Retrieves an actor by their ID and maps to a response DTO.
    Task<ActorResponseDto?> GetActorByIdAsync(int id);

    // Validates input data and creates a new actor record.
    Task<ActorResponseDto> CreateActorAsync(CreateActorDto dto);

    // Updates an existing actor's information.
    Task<ActorResponseDto?> UpdateActorAsync(UpdateActorDto dto);

    // Removes an actor from the system by ID.
    Task<bool> DeleteActorAsync(int id);


    // Retrieves a paginated list of movies featuring the specified actor.
    Task<PaginatedResponseDto<MovieResponseDto>> GetMoviesByActorAsync(int actorId, int page, int pageSize, string? search);
}