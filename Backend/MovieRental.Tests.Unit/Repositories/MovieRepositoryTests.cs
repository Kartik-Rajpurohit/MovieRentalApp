using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class MovieRepositoryTests
{
    [Fact]
    public async Task GetAllMovies_ReturnsActiveMovies()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Movies.AddRange(
            new Movie { MovieId = 1, Title = "Movie 1", IsDeleted = false },
            new Movie { MovieId = 2, Title = "Movie 2", IsDeleted = false }
        );
        await context.SaveChangesAsync();

        var sut = new MovieRepository(context);

        // Act
        var movies = await sut.GetAllMovies().ToListAsync();

        // Assert
        movies.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMovieByIdAsync_WhenExists_ReturnsMovieWithFullRelations()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var language = new Language { LanguageId = 1, Name = "English" };
        var category = new Category { CategoryId = 1, Name = "Action" };
        var actor = new Actor { ActorId = 1, FirstName = "Keanu", LastName = "Reeves" };

        var movie = new Movie
        {
            MovieId = 10,
            Title = "The Matrix",
            LanguageId = 1,
            Language = language,
            MovieCategories = new List<MovieCategory>
            {
                new() { MovieId = 10, CategoryId = 1, Category = category }
            },
            MovieActors = new List<MovieActor>
            {
                new() { MovieId = 10, ActorId = 1, Actor = actor }
            },
            Inventories = new List<Inventory>
            {
                new() { InventoryId = 1, MovieId = 10, StoreId = 1 }
            }
        };

        context.Languages.Add(language);
        context.Categories.Add(category);
        context.Actors.Add(actor);
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        var sut = new MovieRepository(context);

        // Act
        var result = await sut.GetMovieByIdAsync(10);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("The Matrix");
        result.Language.Should().NotBeNull();
        result.MovieCategories.Should().HaveCount(1);
        result.MovieActors.Should().HaveCount(1);
        result.Inventories.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateMovieAsync_AddsAndReturnsMovie()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var language = new Language { LanguageId = 1, Name = "English" };
        context.Languages.Add(language);
        await context.SaveChangesAsync();

        var sut = new MovieRepository(context);
        var movie = new Movie
        {
            Title = "John Wick",
            LanguageId = 1,
            RentalDuration = 3,
            RentalRate = 3.99m,
            ReplacementCost = 19.99m
        };

        // Act
        var created = await sut.CreateMovieAsync(movie);

        // Assert
        created.MovieId.Should().BeGreaterThan(0);
        created.Title.Should().Be("John Wick");
        var inDb = await context.Movies.FindAsync(created.MovieId);
        inDb.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateMovieAsync_WhenExists_UpdatesMovie()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var language = new Language { LanguageId = 1, Name = "English" };
        context.Languages.Add(language);
        var movie = new Movie { MovieId = 1, Title = "Original Title", LanguageId = 1, RentalRate = 2.99m };
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        var sut = new MovieRepository(context);
        movie.Title = "Updated Title";
        movie.RentalRate = 4.99m;

        // Act
        var updated = await sut.UpdateMovieAsync(movie);

        // Assert
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated Title");
        updated.RentalRate.Should().Be(4.99m);
    }

    [Fact]
    public async Task DeleteMovieAsync_WhenExists_SoftDeletesMovie()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Movies.Add(new Movie { MovieId = 1, Title = "Active", IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new MovieRepository(context);

        // Act
        var success = await sut.DeleteMovieAsync(1);

        // Assert
        success.Should().BeTrue();
        var inDb = await context.Movies.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.MovieId == 1);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllLanguages_GetAllCategories_GetAllActors_ReturnActiveEntities()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Languages.Add(new Language { LanguageId = 1, Name = "English" });
        context.Categories.Add(new Category { CategoryId = 1, Name = "Action" });
        context.Actors.Add(new Actor { ActorId = 1, FirstName = "Bruce", LastName = "Willis" });
        await context.SaveChangesAsync();

        var sut = new MovieRepository(context);

        // Act & Assert
        (await sut.GetAllLanguages().ToListAsync()).Should().HaveCount(1);
        (await sut.GetAllCategories().ToListAsync()).Should().HaveCount(1);
        (await sut.GetAllActors().ToListAsync()).Should().HaveCount(1);
    }
}
