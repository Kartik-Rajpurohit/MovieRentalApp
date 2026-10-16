using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class LanguageRepositoryTests
{
    [Fact]
    public async Task GetAllLanguages_ReturnsActiveLanguages()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Languages.AddRange(
            new Language { LanguageId = 1, Name = "English", IsDeleted = false },
            new Language { LanguageId = 2, Name = "Spanish", IsDeleted = false }
        );
        await context.SaveChangesAsync();

        var sut = new LanguageRepository(context);

        // Act
        var languages = await sut.GetAllLanguages().ToListAsync();

        // Assert
        languages.Should().HaveCount(2);
        languages.Select(l => l.Name).Should().Contain(new[] { "English", "Spanish" });
    }

    [Fact]
    public async Task GetLanguageByIdAsync_WhenExists_ReturnsLanguageWithMovies()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var language = new Language
        {
            LanguageId = 1,
            Name = "French",
            Movies = new List<Movie> { new() { MovieId = 10, Title = "Amelie" } }
        };
        context.Languages.Add(language);
        await context.SaveChangesAsync();

        var sut = new LanguageRepository(context);

        // Act
        var result = await sut.GetLanguageByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("French");
        result.Movies.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateLanguageAsync_AddsAndReturnsLanguage()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var sut = new LanguageRepository(context);
        var language = new Language { Name = "German", LastUpdate = DateTime.UtcNow };

        // Act
        var created = await sut.CreateLanguageAsync(language);

        // Assert
        created.LanguageId.Should().BeGreaterThan(0);
        var inDb = await context.Languages.FindAsync(created.LanguageId);
        inDb.Should().NotBeNull();
        inDb!.Name.Should().Be("German");
    }

    [Fact]
    public async Task UpdateLanguageAsync_WhenExists_UpdatesName()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Languages.Add(new Language { LanguageId = 1, Name = "OldName" });
        await context.SaveChangesAsync();

        var sut = new LanguageRepository(context);
        var updateModel = new Language { LanguageId = 1, Name = "Italian" };

        // Act
        var updated = await sut.UpdateLanguageAsync(updateModel);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Italian");
    }

    [Fact]
    public async Task DeleteLanguageAsync_WhenExists_SoftDeletesLanguage()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Languages.Add(new Language { LanguageId = 1, Name = "Mandarin", IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new LanguageRepository(context);

        // Act
        var success = await sut.DeleteLanguageAsync(1);

        // Assert
        success.Should().BeTrue();
        var inDb = await context.Languages.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.LanguageId == 1);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetMoviesByLanguageId_ReturnsMoviesForSpecifiedLanguage()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var lang1 = new Language { LanguageId = 1, Name = "English" };
        var lang2 = new Language { LanguageId = 2, Name = "Japanese" };
        context.Languages.AddRange(lang1, lang2);

        context.Movies.AddRange(
            new Movie { MovieId = 1, Title = "Movie 1", LanguageId = 1 },
            new Movie { MovieId = 2, Title = "Movie 2", LanguageId = 1 },
            new Movie { MovieId = 3, Title = "Movie 3", LanguageId = 2 }
        );
        await context.SaveChangesAsync();

        var sut = new LanguageRepository(context);

        // Act
        var movies = await sut.GetMoviesByLanguageId(1).ToListAsync();

        // Assert
        movies.Should().HaveCount(2);
        movies.Select(m => m.Title).Should().Contain(new[] { "Movie 1", "Movie 2" });
    }
}
