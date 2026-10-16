using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class ActorRepositoryTests
{
    [Fact]
    public async Task GetAllActors_ReturnsActorsWithMovieActors()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var actor = new Actor { ActorId = 1, FirstName = "Tom", LastName = "Hanks", IsDeleted = false };
        var movie = new Movie { MovieId = 1, Title = "Cast Away" };
        actor.MovieActors = new List<MovieActor>
        {
            new() { ActorId = 1, MovieId = 1, Movie = movie }
        };
        context.Actors.Add(actor);
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        var sut = new ActorRepository(context);

        // Act
        var actors = await sut.GetAllActors().ToListAsync();

        // Assert
        actors.Should().HaveCount(1);
        actors.First().FirstName.Should().Be("Tom");
        actors.First().MovieActors.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetActorByIdAsync_WhenExists_ReturnsActorWithMovies()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var actor = new Actor { ActorId = 10, FirstName = "Morgan", LastName = "Freeman" };
        var movie = new Movie { MovieId = 20, Title = "Shawshank Redemption" };
        actor.MovieActors = new List<MovieActor>
        {
            new() { ActorId = 10, MovieId = 20, Movie = movie }
        };
        context.Actors.Add(actor);
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        var sut = new ActorRepository(context);

        // Act
        var result = await sut.GetActorByIdAsync(10);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Morgan");
        result.MovieActors.First().Movie.Title.Should().Be("Shawshank Redemption");
    }

    [Fact]
    public async Task CreateActorAsync_AddsAndReturnsActor()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var sut = new ActorRepository(context);
        var actor = new Actor { FirstName = "Brad", LastName = "Pitt", LastUpdate = DateTime.UtcNow };

        // Act
        var created = await sut.CreateActorAsync(actor);

        // Assert
        created.ActorId.Should().BeGreaterThan(0);
        var inDb = await context.Actors.FindAsync(created.ActorId);
        inDb.Should().NotBeNull();
        inDb!.FirstName.Should().Be("Brad");
    }

    [Fact]
    public async Task UpdateActorAsync_WhenExists_UpdatesNames()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var actor = new Actor { ActorId = 1, FirstName = "OldFirst", LastName = "OldLast" };
        context.Actors.Add(actor);
        await context.SaveChangesAsync();

        var sut = new ActorRepository(context);
        var updateModel = new Actor { ActorId = 1, FirstName = "NewFirst", LastName = "NewLast" };

        // Act
        var updated = await sut.UpdateActorAsync(updateModel);

        // Assert
        updated.Should().NotBeNull();
        updated!.FirstName.Should().Be("NewFirst");
        updated.LastName.Should().Be("NewLast");
    }

    [Fact]
    public async Task DeleteActorAsync_WhenExists_SoftDeletesActor()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Actors.Add(new Actor { ActorId = 1, FirstName = "Matt", LastName = "Damon", IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new ActorRepository(context);

        // Act
        var success = await sut.DeleteActorAsync(1);

        // Assert
        success.Should().BeTrue();
        var inDb = await context.Actors.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.ActorId == 1);
        inDb!.IsDeleted.Should().BeTrue();
    }
}
