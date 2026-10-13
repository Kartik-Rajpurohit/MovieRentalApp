using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _movieRepositoryMock;
    private readonly MovieService _sut;

    public MovieServiceTests()
    {
        _movieRepositoryMock = new Mock<IMovieRepository>();
        _sut = new MovieService(_movieRepositoryMock.Object);
    }

    private static List<Movie> CreateSampleMovies()
    {
        var english = new Language { LanguageId = 1, Name = "English" };
        var french = new Language { LanguageId = 2, Name = "French" };

        var catAction = new Category { CategoryId = 1, Name = "Action" };
        var catSciFi = new Category { CategoryId = 2, Name = "Sci-Fi" };

        var actorLeo = new Actor { ActorId = 1, FirstName = "Leonardo", LastName = "DiCaprio" };
        var actorBale = new Actor { ActorId = 2, FirstName = "Christian", LastName = "Bale" };

        return new List<Movie>
        {
            new()
            {
                MovieId = 1,
                Title = "Inception",
                Description = "A thief who steals corporate secrets through dream-sharing",
                ReleaseYear = 2010,
                LanguageId = 1,
                Language = english,
                RentalDuration = 5,
                RentalRate = 4.99m,
                Length = 148,
                ReplacementCost = 19.99m,
                Rating = "PG-13",
                MovieCategories = new List<MovieCategory>
                {
                    new() { MovieId = 1, CategoryId = 1, Category = catAction },
                    new() { MovieId = 1, CategoryId = 2, Category = catSciFi }
                },
                MovieActors = new List<MovieActor>
                {
                    new() { MovieId = 1, ActorId = 1, Actor = actorLeo }
                }
            },
            new()
            {
                MovieId = 2,
                Title = "The Dark Knight",
                Description = "Batman battles the Joker in Gotham City",
                ReleaseYear = 2008,
                LanguageId = 1,
                Language = english,
                RentalDuration = 7,
                RentalRate = 3.99m,
                Length = 152,
                ReplacementCost = 21.99m,
                Rating = "PG-13",
                MovieCategories = new List<MovieCategory>
                {
                    new() { MovieId = 2, CategoryId = 1, Category = catAction }
                },
                MovieActors = new List<MovieActor>
                {
                    new() { MovieId = 2, ActorId = 2, Actor = actorBale }
                }
            },
            new()
            {
                MovieId = 3,
                Title = "Amelie",
                Description = "A whimsical depiction of contemporary Parisian life",
                ReleaseYear = 2001,
                LanguageId = 2,
                Language = french,
                RentalDuration = 3,
                RentalRate = 2.99m,
                Length = 122,
                ReplacementCost = 15.99m,
                Rating = "R",
                MovieCategories = new List<MovieCategory>(),
                MovieActors = new List<MovieActor>()
            }
        };
    }

    // =========================================================
    // GetAllMoviesAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllMoviesAsync_WhenMoviesExist_ReturnsMovieListWithMappings()
    {
        // Arrange
        var movies = CreateSampleMovies();
        _movieRepositoryMock.Setup(r => r.GetAllMovies()).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new MovieFilterDto();

        // Act
        var result = await _sut.GetAllMoviesAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.Data.Should().HaveCount(3);

        var inception = result.Data.First(m => m.MovieId == 1);
        inception.Title.Should().Be("Inception");
        inception.LanguageName.Should().Be("English");
        inception.Categories.Should().Contain(new[] { "Action", "Sci-Fi" });
        inception.Actors.Should().Contain("Leonardo DiCaprio");
    }

    [Theory]
    [InlineData("Inception", 1)]
    [InlineData("Gotham", 1)]          // Search by description
    [InlineData("Leonardo", 1)]        // Search by actor
    [InlineData("Sci-Fi", 1)]          // Search by category
    [InlineData("NonExistent", 0)]
    public async Task GetAllMoviesAsync_WithSearch_FiltersAcrossFields(string searchTerm, int expectedCount)
    {
        // Arrange
        var movies = CreateSampleMovies();
        _movieRepositoryMock.Setup(r => r.GetAllMovies()).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = searchTerm };
        var filter = new MovieFilterDto();

        // Act
        var result = await _sut.GetAllMoviesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetAllMoviesAsync_WithModuleFilters_FiltersResults()
    {
        // Arrange
        var movies = CreateSampleMovies();
        _movieRepositoryMock.Setup(r => r.GetAllMovies()).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new MovieFilterDto
        {
            LanguageId = 1,
            CategoryId = 1,
            Rating = "PG-13",
            ReleaseYear = 2010,
            MinRentalRate = 4.00m,
            MinLength = 140
        };

        // Act
        var result = await _sut.GetAllMoviesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.First().Title.Should().Be("Inception");
    }

    [Theory]
    [InlineData("title", "asc", "Amelie")]
    [InlineData("rentalrate", "desc", "Inception")]
    [InlineData("default", "desc", "Amelie")] // id desc (Amelie is id 3)
    public async Task GetAllMoviesAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirstTitle)
    {
        // Arrange
        var movies = CreateSampleMovies();
        _movieRepositoryMock.Setup(r => r.GetAllMovies()).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new MovieFilterDto();

        // Act
        var result = await _sut.GetAllMoviesAsync(pagination, filter);

        // Assert
        result.Data.First().Title.Should().Be(expectedFirstTitle);
    }

    [Fact]
    public async Task GetAllMoviesAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Movie>();
        _movieRepositoryMock.Setup(r => r.GetAllMovies()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new MovieFilterDto();

        // Act
        var result = await _sut.GetAllMoviesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetMovieByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetMovieByIdAsync_WhenExists_ReturnsDetailDto()
    {
        // Arrange
        var movie = CreateSampleMovies().First(m => m.MovieId == 1);
        movie.Inventories = new List<Inventory> { new(), new() };

        _movieRepositoryMock.Setup(r => r.GetMovieByIdAsync(1)).ReturnsAsync(movie);

        // Act
        var result = await _sut.GetMovieByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.MovieId.Should().Be(1);
        result.Title.Should().Be("Inception");
        result.Categories.Should().HaveCount(2);
        result.Actors.Should().HaveCount(1);
        result.TotalInventory.Should().Be(2);
    }

    [Fact]
    public async Task GetMovieByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _movieRepositoryMock.Setup(r => r.GetMovieByIdAsync(999)).ReturnsAsync((Movie?)null);

        // Act
        var result = await _sut.GetMovieByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateMovieAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateMovieAsync_WhenValid_CreatesMovieWithRelationships()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Interstellar",
            LanguageId = 1,
            OriginalLanguageId = 1,
            CategoryIds = new List<int> { 1, 2 },
            ActorIds = new List<int> { 1 }
        };

        var languages = new List<Language> { new() { LanguageId = 1, Name = "English" } };
        var categories = new List<Category>
        {
            new() { CategoryId = 1, Name = "Action" },
            new() { CategoryId = 2, Name = "Sci-Fi" }
        };
        var actors = new List<Actor> { new() { ActorId = 1, FirstName = "Matthew", LastName = "McConaughey" } };

        _movieRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(languages.BuildMock());
        _movieRepositoryMock.Setup(r => r.GetAllCategories()).Returns(categories.BuildMock());
        _movieRepositoryMock.Setup(r => r.GetAllActors()).Returns(actors.BuildMock());

        var created = new Movie
        {
            MovieId = 10,
            Title = dto.Title,
            LanguageId = 1,
            Language = languages[0],
            MovieCategories = new List<MovieCategory>(),
            MovieActors = new List<MovieActor>()
        };
        _movieRepositoryMock.Setup(r => r.CreateMovieAsync(It.IsAny<Movie>())).ReturnsAsync(created);

        // Act
        var result = await _sut.CreateMovieAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.MovieId.Should().Be(10);
        result.Title.Should().Be("Interstellar");
        _movieRepositoryMock.Verify(r => r.CreateMovieAsync(It.Is<Movie>(m =>
            m.Title == "Interstellar" &&
            m.MovieCategories.Count == 2 &&
            m.MovieActors.Count == 1)), Times.Once);
    }

    [Fact]
    public async Task CreateMovieAsync_WhenLanguageDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateMovieDto { Title = "Invalid Movie", LanguageId = 999 };
        var emptyLanguages = new List<Language>();
        _movieRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(emptyLanguages.BuildMock());

        // Act
        var act = () => _sut.CreateMovieAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Language with ID 999 does not exist*");
    }

    [Fact]
    public async Task CreateMovieAsync_WhenOriginalLanguageDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateMovieDto { Title = "Invalid Movie", LanguageId = 1, OriginalLanguageId = 888 };
        var languages = new List<Language> { new() { LanguageId = 1, Name = "English" } };
        _movieRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(languages.BuildMock());

        // Act
        var act = () => _sut.CreateMovieAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Original Language with ID 888 does not exist*");
    }

    [Fact]
    public async Task CreateMovieAsync_WhenCategoriesDoNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Invalid Movie",
            LanguageId = 1,
            CategoryIds = new List<int> { 1, 999 }
        };
        var languages = new List<Language> { new() { LanguageId = 1, Name = "English" } };
        var categories = new List<Category> { new() { CategoryId = 1, Name = "Action" } };

        _movieRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(languages.BuildMock());
        _movieRepositoryMock.Setup(r => r.GetAllCategories()).Returns(categories.BuildMock());

        // Act
        var act = () => _sut.CreateMovieAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*One or more categories do not exist*999*");
    }

    [Fact]
    public async Task CreateMovieAsync_WhenActorsDoNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateMovieDto
        {
            Title = "Invalid Movie",
            LanguageId = 1,
            ActorIds = new List<int> { 777 }
        };
        var languages = new List<Language> { new() { LanguageId = 1, Name = "English" } };
        var emptyActors = new List<Actor>();

        _movieRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(languages.BuildMock());
        _movieRepositoryMock.Setup(r => r.GetAllCategories()).Returns(new List<Category>().BuildMock());
        _movieRepositoryMock.Setup(r => r.GetAllActors()).Returns(emptyActors.BuildMock());

        // Act
        var act = () => _sut.CreateMovieAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*One or more actors do not exist*777*");
    }

    // =========================================================
    // UpdateMovieAsync Tests
    // =========================================================

    [Fact]
    public async Task UpdateMovieAsync_WhenMovieNotFound_ReturnsNull()
    {
        // Arrange
        _movieRepositoryMock.Setup(r => r.GetMovieByIdAsync(999)).ReturnsAsync((Movie?)null);

        // Act
        var result = await _sut.UpdateMovieAsync(new UpdateMovieDto { MovieId = 999 });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateMovieAsync_WhenValidPatch_UpdatesFieldsAndAssociations()
    {
        // Arrange
        var existing = CreateSampleMovies().First(m => m.MovieId == 1);
        _movieRepositoryMock.Setup(r => r.GetMovieByIdAsync(1)).ReturnsAsync(existing);

        var languages = new List<Language> { new() { LanguageId = 2, Name = "French" } };
        var categories = new List<Category> { new() { CategoryId = 1, Name = "Action" } };
        var actors = new List<Actor> { new() { ActorId = 2, FirstName = "Christian", LastName = "Bale" } };

        _movieRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(languages.BuildMock());
        _movieRepositoryMock.Setup(r => r.GetAllCategories()).Returns(categories.BuildMock());
        _movieRepositoryMock.Setup(r => r.GetAllActors()).Returns(actors.BuildMock());

        _movieRepositoryMock.Setup(r => r.UpdateMovieAsync(It.IsAny<Movie>()))
            .ReturnsAsync((Movie m) => m);

        var dto = new UpdateMovieDto
        {
            MovieId = 1,
            Title = "Inception: Remastered",
            LanguageId = 2,
            RentalRate = 5.99m,
            CategoryIds = new List<int> { 1 },
            ActorIds = new List<int> { 2 }
        };

        // Act
        var result = await _sut.UpdateMovieAsync(dto);

        // Assert
        result.Should().NotBeNull();
        existing.Title.Should().Be("Inception: Remastered");
        existing.LanguageId.Should().Be(2);
        existing.RentalRate.Should().Be(5.99m);
        existing.MovieCategories.Should().ContainSingle(c => c.CategoryId == 1);
        existing.MovieActors.Should().ContainSingle(a => a.ActorId == 2);
    }

    [Fact]
    public async Task UpdateMovieAsync_WhenInvalidLanguageId_ThrowsInvalidOperationException()
    {
        // Arrange
        var existing = CreateSampleMovies().First(m => m.MovieId == 1);
        _movieRepositoryMock.Setup(r => r.GetMovieByIdAsync(1)).ReturnsAsync(existing);
        _movieRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(new List<Language>().BuildMock());

        var dto = new UpdateMovieDto { MovieId = 1, LanguageId = 999 };

        // Act
        var act = () => _sut.UpdateMovieAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Language with ID 999 does not exist*");
    }

    // =========================================================
    // DeleteMovieAsync Tests
    // =========================================================

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteMovieAsync_DelegatesToRepository(bool expectedResult)
    {
        // Arrange
        _movieRepositoryMock.Setup(r => r.DeleteMovieAsync(1)).ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.DeleteMovieAsync(1);

        // Assert
        result.Should().Be(expectedResult);
        _movieRepositoryMock.Verify(r => r.DeleteMovieAsync(1), Times.Once);
    }
}
