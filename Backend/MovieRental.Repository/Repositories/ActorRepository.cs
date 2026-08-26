using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Data;
using MovieRental.Repository.Interfaces;

namespace MovieRental.Repository.Repositories;

public class ActorRepository : IActorRepository
{
    private readonly AppDbContext _context;

    public ActorRepository(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Actor> GetAllActors()
        => _context.Actors.Include(a => a.FilmActors).AsQueryable();

    public async Task<Actor?> GetActorByIdAsync(int id)
        => await _context.Actors
            .Include(a => a.FilmActors)
            .ThenInclude(fa => fa.Film)
            .FirstOrDefaultAsync(a => a.ActorId == id);

    public async Task<Actor> CreateActorAsync(Actor actor)
    {
        _context.Actors.Add(actor);
        await _context.SaveChangesAsync();
        return actor;
    }

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

    public async Task<bool> DeleteActorAsync(int id)
    {
        var actor = await _context.Actors.FindAsync(id);
        if (actor == null) return false;
        _context.Actors.Remove(actor);
        await _context.SaveChangesAsync();
        return true;
    }
    public IQueryable<Film> GetFilmsByActorId(int actorId)
    => _context.FilmActors
        .Where(fa => fa.ActorId == actorId)
        .Select(fa => fa.Film)
        .AsQueryable();
}