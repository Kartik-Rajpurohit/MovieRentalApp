using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces;

public interface IActorRepository
{
    IQueryable<Actor> GetAllActors();
    Task<Actor?> GetActorByIdAsync(int id);
    Task<Actor> CreateActorAsync(Actor actor);
    Task<Actor?> UpdateActorAsync(Actor actor);
    Task<bool> DeleteActorAsync(int id);
    IQueryable<Film> GetFilmsByActorId(int actorId);
}