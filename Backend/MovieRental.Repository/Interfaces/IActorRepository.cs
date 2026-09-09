using MovieRental.Domain.Entities;

namespace MovieRental.Repository.Interfaces;

// Defines database operations for Actor entities and their filmography.
public interface IActorRepository
{
    // Returns a queryable collection of all actors for filtering, sorting, and pagination.
    IQueryable<Actor> GetAllActors();

    // Finds an actor by their ID, including related films.
    Task<Actor?> GetActorByIdAsync(int id);

    // Adds a new actor record to the database.
    Task<Actor> CreateActorAsync(Actor actor);

    // Updates an existing actor's information.
    Task<Actor?> UpdateActorAsync(Actor actor);

    // Deletes an actor by ID; returns true if deleted, false if not found.
    Task<bool> DeleteActorAsync(int id);

    // Returns a queryable collection of films featuring the specified actor.
    IQueryable<Film> GetFilmsByActorId(int actorId);
}