using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories;

// Handles database operations related to actors.
public class ActorRepository : IActorRepository
{
    // Receives the database context used to access application tables.
    private readonly AppDbContext _context;

    public ActorRepository(AppDbContext context)
    {
        _context = context;
    }

    // Reads all actors from the database without tracking for better query performance.
    // Loads the MovieActors join table so related movies can be counted.
    public IQueryable<Actor> GetAllActors()
        => _context.Actors.AsNoTracking().Include(a => a.MovieActors).AsQueryable();

    // Finds an actor by ID and includes their movie details.
    public async Task<Actor?> GetActorByIdAsync(int id)
        => await _context.Actors
            .Include(a => a.MovieActors)
            .ThenInclude(ma => ma.Movie)
            .FirstOrDefaultAsync(a => a.ActorId == id);

    // Adds a new actor record to the database and saves changes.
    public async Task<Actor> CreateActorAsync(Actor actor)
    {
        _context.Actors.Add(actor);
        await _context.SaveChangesAsync();
        return actor;
    }

    // Finds the actor by ID and updates their first and last name.
    public async Task<Actor?> UpdateActorAsync(Actor actor)
    {
        var existing = await _context.Actors.FindAsync(actor.ActorId);
        if (existing == null) return null;
        existing.FirstName = actor.FirstName;
        existing.LastName = actor.LastName;
        existing.LastUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return existing;
    }

    // Removes the actor from the database if found.
    public async Task<bool> DeleteActorAsync(int id)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null) return false;
        _context.Actors.Remove(actor);
        await _context.SaveChangesAsync();
        return true;
    }

    // Queries movies linked to a specific actor through the MovieActors join table.
    public IQueryable<Movie> GetMoviesByActorId(int actorId)
    => _context.MovieActors
        .AsNoTracking()
        .Where(ma => ma.ActorId == actorId)
        .Select(ma => ma.Movie)
        .AsQueryable();
}