using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class CategoryRepositoryTests
{
    [Fact]
    public async Task GetAllCategories_ReturnsCategoriesWithMovieCategories()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var category = new Category { CategoryId = 1, Name = "Action", IsDeleted = false };
        var movie = new Movie { MovieId = 10, Title = "Die Hard" };
        category.MovieCategories = new List<MovieCategory>
        {
            new() { CategoryId = 1, MovieId = 10, Movie = movie }
        };
        context.Categories.Add(category);
        context.Movies.Add(movie);
        await context.SaveChangesAsync();

        var sut = new CategoryRepository(context);

        // Act
        var categories = await sut.GetAllCategories().ToListAsync();

        // Assert
        categories.Should().HaveCount(1);
        categories.First().Name.Should().Be("Action");
        categories.First().MovieCategories.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WhenExists_ReturnsCategory()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { CategoryId = 5, Name = "Sci-Fi" });
        await context.SaveChangesAsync();

        var sut = new CategoryRepository(context);

        // Act
        var result = await sut.GetCategoryByIdAsync(5);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryId.Should().Be(5);
        result.Name.Should().Be("Sci-Fi");
    }

    [Fact]
    public async Task CreateCategoryAsync_AddsAndReturnsCategory()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var sut = new CategoryRepository(context);
        var category = new Category { Name = "Horror" };

        // Act
        var created = await sut.CreateCategoryAsync(category);

        // Assert
        created.CategoryId.Should().BeGreaterThan(0);
        created.Name.Should().Be("Horror");
        var inDb = await context.Categories.FindAsync(created.CategoryId);
        inDb.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenExists_UpdatesAndReturnsCategory()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var category = new Category { CategoryId = 1, Name = "OldGenre" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        var sut = new CategoryRepository(context);
        category.Name = "Drama";

        // Act
        var updated = await sut.UpdateCategoryAsync(category);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Drama");
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenExists_SoftDeletesCategory()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { CategoryId = 1, Name = "Comedy", IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new CategoryRepository(context);

        // Act
        var success = await sut.DeleteCategoryAsync(1);

        // Assert
        success.Should().BeTrue();
        var inDb = await context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryId == 1);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenNotFoundOrDeleted_ReturnsFalse()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Categories.Add(new Category { CategoryId = 1, Name = "Deleted", IsDeleted = true });
        await context.SaveChangesAsync();

        var sut = new CategoryRepository(context);

        // Act
        var success = await sut.DeleteCategoryAsync(1);

        // Assert
        success.Should().BeFalse();
    }

    [Fact]
    public async Task GetMoviesByCategoryId_ReturnsMoviesLinkedThroughJunction()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var cat1 = new Category { CategoryId = 1, Name = "Thriller" };
        var cat2 = new Category { CategoryId = 2, Name = "Documentary" };
        var movie1 = new Movie { MovieId = 1, Title = "Movie One" };
        var movie2 = new Movie { MovieId = 2, Title = "Movie Two" };

        context.Categories.AddRange(cat1, cat2);
        context.Movies.AddRange(movie1, movie2);
        context.MovieCategories.AddRange(
            new MovieCategory { CategoryId = 1, MovieId = 1, Category = cat1, Movie = movie1 },
            new MovieCategory { CategoryId = 2, MovieId = 2, Category = cat2, Movie = movie2 }
        );
        await context.SaveChangesAsync();

        var sut = new CategoryRepository(context);

        // Act
        var movies = await sut.GetMoviesByCategoryId(1).ToListAsync();

        // Assert
        movies.Should().HaveCount(1);
        movies.First().Title.Should().Be("Movie One");
    }
}
