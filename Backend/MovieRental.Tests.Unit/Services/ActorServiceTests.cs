using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Actors;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class ActorServiceTests
{
    private readonly Mock<IActorRepository> _actorRepositoryMock;
    private readonly ActorService _sut;

    public ActorServiceTests()
    {
        _actorRepositoryMock = new Mock<IActorRepository>();
        _sut = new ActorService(_actorRepositoryMock.Object);
    }

    private static List<Actor> CreateSampleActors()
    {
        return new List<Actor>
        {
            new()
            {
                ActorId = 1,
                FirstName = "TOM",
                LastName = "HANKS",
                LastUpdate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                MovieActors = new List<MovieActor> { new(), new() }
            },
            new()
            {
                ActorId = 2,
                FirstName = "LEONARDO",
                LastName = "DICAPRIO",
                LastUpdate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                MovieActors = new List<MovieActor> { new() }
            },
            new()
            {
                ActorId = 3,
                FirstName = "BRAD",
                LastName = "PITT",
                LastUpdate = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                MovieActors = new List<MovieActor>()
            }
        };
    }

    private static List<Movie> CreateSampleMovies(int actorId)
    {
        var actor = new Actor { ActorId = actorId, FirstName = "TOM", LastName = "HANKS" };
        return new List<Movie>
        {
            new()
            {
                MovieId = 1,
                Title = "Cast Away",
                Description = "A FedEx executive undergoes a physical and emotional transformation after a crash.",
                ReleaseYear = 2000,
                RentalRate = 3.99m,
                RentalDuration = 5,
                Length = 143,
                ReplacementCost = 19.99m,
                Rating = "PG-13",
                MovieActors = new List<MovieActor> { new() { ActorId = actorId, Actor = actor } },
                MovieCategories = new List<MovieCategory>()
            },
            new()
            {
                MovieId = 2,
                Title = "Forrest Gump",
                Description = "The presidencies of Kennedy and Johnson through the eyes of an Alabama man.",
                ReleaseYear = 1994,
                RentalRate = 4.99m,
                RentalDuration = 7,
                Length = 142,
                ReplacementCost = 24.99m,
                Rating = "PG-13",
                MovieActors = new List<MovieActor> { new() { ActorId = actorId, Actor = actor } },
                MovieCategories = new List<MovieCategory>()
            },
            new()
            {
                MovieId = 3,
                Title = "Apollo 13",
                Description = "NASA must devise a strategy to return Apollo 13 to Earth safely.",
                ReleaseYear = 1995,
                RentalRate = 2.99m,
                RentalDuration = 3,
                Length = 140,
                ReplacementCost = 14.99m,
                Rating = "PG",
                MovieActors = new List<MovieActor> { new() { ActorId = actorId, Actor = actor } },
                MovieCategories = new List<MovieCategory>()
            }
        };
    }

    // =========================================================
    // GetAllActorsAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllActorsAsync_WhenActorsExist_ReturnsActorList()
    {
        // Arrange
        var actors = CreateSampleActors();
        _actorRepositoryMock.Setup(r => r.GetAllActors()).Returns(actors.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new ActorFilterDto();

        // Act
        var result = await _sut.GetAllActorsAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllActorsAsync_WithSearchTerm_FiltersByFullName()
    {
        // Arrange
        var actors = CreateSampleActors();
        _actorRepositoryMock.Setup(r => r.GetAllActors()).Returns(actors.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "dicaprio" };
        var filter = new ActorFilterDto();

        // Act
        var result = await _sut.GetAllActorsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(a => a.FirstName == "LEONARDO" && a.LastName == "DICAPRIO");
    }

    [Fact]
    public async Task GetAllActorsAsync_WithActorIdFilter_FiltersByActorId()
    {
        // Arrange
        var actors = CreateSampleActors();
        _actorRepositoryMock.Setup(r => r.GetAllActors()).Returns(actors.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new ActorFilterDto { ActorId = 3 };

        // Act
        var result = await _sut.GetAllActorsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(a => a.ActorId == 3 && a.FirstName == "BRAD");
    }

    [Theory]
    [InlineData("fullname", "asc", "BRAD")]
    [InlineData("fullname", "desc", "TOM")]
    [InlineData("default", "desc", "BRAD")]
    public async Task GetAllActorsAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirstFirstName)
    {
        // Arrange
        var actors = CreateSampleActors();
        _actorRepositoryMock.Setup(r => r.GetAllActors()).Returns(actors.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new ActorFilterDto();

        // Act
        var result = await _sut.GetAllActorsAsync(pagination, filter);

        // Assert
        result.Data.First().FirstName.Should().Be(expectedFirstFirstName);
    }

    [Fact]
    public async Task GetAllActorsAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Actor>();
        _actorRepositoryMock.Setup(r => r.GetAllActors()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new ActorFilterDto();

        // Act
        var result = await _sut.GetAllActorsAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetActorByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetActorByIdAsync_WhenActorExists_ReturnsActorDto()
    {
        // Arrange
        var actor = new Actor { ActorId = 1, FirstName = "TOM", LastName = "HANKS", MovieActors = new List<MovieActor>() };
        _actorRepositoryMock.Setup(r => r.GetActorByIdAsync(1)).ReturnsAsync(actor);

        // Act
        var result = await _sut.GetActorByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.ActorId.Should().Be(1);
        result.FirstName.Should().Be("TOM");
        result.LastName.Should().Be("HANKS");
    }

    [Fact]
    public async Task GetActorByIdAsync_WhenActorDoesNotExist_ReturnsNull()
    {
        // Arrange
        _actorRepositoryMock.Setup(r => r.GetActorByIdAsync(999)).ReturnsAsync((Actor?)null);

        // Act
        var result = await _sut.GetActorByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateActorAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateActorAsync_ValidDto_ConvertsNamesToUppercaseSetsTimestampAndReturnsDto()
    {
        // Arrange
        var dto = new CreateActorDto { FirstName = "morgan", LastName = "freeman" };

        _actorRepositoryMock.Setup(r => r.CreateActorAsync(It.IsAny<Actor>()))
            .ReturnsAsync((Actor a) =>
            {
                a.ActorId = 4;
                a.MovieActors = new List<MovieActor>();
                return a;
            });

        // Act
        var result = await _sut.CreateActorAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.ActorId.Should().Be(4);
        result.FirstName.Should().Be("MORGAN");
        result.LastName.Should().Be("FREEMAN");

        _actorRepositoryMock.Verify(r => r.CreateActorAsync(It.Is<Actor>(a =>
            a.FirstName == "MORGAN" &&
            a.LastName == "FREEMAN" &&
            a.LastUpdate != default
        )), Times.Once);
    }

    // =========================================================
    // UpdateActorAsync Tests
    // =========================================================

    [Fact]
    public async Task UpdateActorAsync_WhenActorExists_ConvertsNamesToUppercaseAndReturnsUpdatedDto()
    {
        // Arrange
        var dto = new UpdateActorDto { ActorId = 2, FirstName = "leo", LastName = "dicaprio" };

        _actorRepositoryMock.Setup(r => r.UpdateActorAsync(It.IsAny<Actor>()))
            .ReturnsAsync((Actor a) =>
            {
                a.MovieActors = new List<MovieActor>();
                return a;
            });

        // Act
        var result = await _sut.UpdateActorAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.ActorId.Should().Be(2);
        result.FirstName.Should().Be("LEO");
        result.LastName.Should().Be("DICAPRIO");

        _actorRepositoryMock.Verify(r => r.UpdateActorAsync(It.Is<Actor>(a =>
            a.ActorId == 2 &&
            a.FirstName == "LEO" &&
            a.LastName == "DICAPRIO"
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateActorAsync_WhenRepositoryReturnsNull_ReturnsNull()
    {
        // Arrange
        var dto = new UpdateActorDto { ActorId = 999, FirstName = "Ghost", LastName = "Actor" };

        _actorRepositoryMock.Setup(r => r.UpdateActorAsync(It.IsAny<Actor>()))
            .ReturnsAsync((Actor?)null);

        // Act
        var result = await _sut.UpdateActorAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // DeleteActorAsync Tests
    // =========================================================

    [Fact]
    public async Task DeleteActorAsync_WhenSuccessful_ReturnsTrue()
    {
        // Arrange
        _actorRepositoryMock.Setup(r => r.DeleteActorAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteActorAsync(1);

        // Assert
        result.Should().BeTrue();
        _actorRepositoryMock.Verify(r => r.DeleteActorAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteActorAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        _actorRepositoryMock.Setup(r => r.DeleteActorAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteActorAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    // =========================================================
    // GetMoviesByActorAsync Tests
    // =========================================================

    [Fact]
    public async Task GetMoviesByActorAsync_WhenMoviesExist_ReturnsMovieList()
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _actorRepositoryMock.Setup(r => r.GetMoviesByActorId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetMoviesByActorAsync(1, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMoviesByActorAsync_WithSearchMatchingTitle_FiltersMovies()
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _actorRepositoryMock.Setup(r => r.GetMoviesByActorId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "Forrest" };

        // Act
        var result = await _sut.GetMoviesByActorAsync(1, pagination);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(m => m.Title == "Forrest Gump");
    }

    [Fact]
    public async Task GetMoviesByActorAsync_WithSearchMatchingDescription_FiltersMovies()
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _actorRepositoryMock.Setup(r => r.GetMoviesByActorId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "FedEx" };

        // Act
        var result = await _sut.GetMoviesByActorAsync(1, pagination);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(m => m.Title == "Cast Away");
    }

    [Theory]
    [InlineData("title", "asc", "Apollo 13")]
    [InlineData("releaseyear", "desc", "Cast Away")]
    [InlineData("default", "desc", "Forrest Gump")]
    public async Task GetMoviesByActorAsync_WithSorting_OrdersMoviesCorrectly(string sortBy, string sortOrder, string expectedFirst)
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _actorRepositoryMock.Setup(r => r.GetMoviesByActorId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        // Act
        var result = await _sut.GetMoviesByActorAsync(1, pagination);

        // Assert
        result.Data.First().Title.Should().Be(expectedFirst);
    }

    [Fact]
    public async Task GetMoviesByActorAsync_WhenNoMoviesExist_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Movie>();
        _actorRepositoryMock.Setup(r => r.GetMoviesByActorId(999)).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetMoviesByActorAsync(999, pagination);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }
}
