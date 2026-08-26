using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.QueryParameters;

namespace MovieRental.Services.Interfaces;

public interface IActorService
{
    Task<PaginatedResponseDto<ActorResponseDto>> GetAllActorsAsync(ActorQueryParametersDto queryParams);
    Task<ActorResponseDto?> GetActorByIdAsync(int id);
    Task<ActorResponseDto> CreateActorAsync(CreateActorDto dto);
    Task<ActorResponseDto?> UpdateActorAsync(UpdateActorDto dto);
    Task<bool> DeleteActorAsync(int id);
    Task<ActorDetailDto?> GetActorDetailAsync(int id);
    Task<PaginatedResponseDto<MovieResponseDto>> GetFilmsByActorAsync(int actorId, int page, int pageSize, string? search);
}