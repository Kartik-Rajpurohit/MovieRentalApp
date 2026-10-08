using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Languages;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class LanguageServiceTests
{
    private readonly Mock<ILanguageRepository> _languageRepositoryMock;
    private readonly LanguageService _sut;

    public LanguageServiceTests()
    {
        _languageRepositoryMock = new Mock<ILanguageRepository>();
        _sut = new LanguageService(_languageRepositoryMock.Object);
    }

    private static List<Language> CreateSampleLanguages()
    {
        return new List<Language>
        {
            new()
            {
                LanguageId = 1,
                Name = "English",
                LastUpdate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Movies = new List<Movie> { new(), new() }
            },
            new()
            {
                LanguageId = 2,
                Name = "Spanish",
                LastUpdate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                Movies = new List<Movie> { new() }
            },
            new()
            {
                LanguageId = 3,
                Name = "French",
                LastUpdate = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                Movies = new List<Movie>()
            }
        };
    }

    private static List<Movie> CreateSampleMovies(int languageId)
    {
        var lang = new Language { LanguageId = languageId, Name = "English" };
        return new List<Movie>
        {
            new()
            {
                MovieId = 1,
                Title = "Inception",
                Description = "A mind-bending sci-fi thriller about dreams within dreams.",
                ReleaseYear = 2010,
                RentalRate = 3.99m,
                RentalDuration = 5,
                Length = 148,
                ReplacementCost = 19.99m,
                Rating = "PG-13",
                LanguageId = languageId,
                Language = lang,
                MovieCategories = new List<MovieCategory>()
            },
            new()
            {
                MovieId = 2,
                Title = "Avatar",
                Description = "Epic adventure on Pandora.",
                ReleaseYear = 2009,
                RentalRate = 4.99m,
                RentalDuration = 7,
                Length = 162,
                ReplacementCost = 24.99m,
                Rating = "PG-13",
                LanguageId = languageId,
                Language = lang,
                MovieCategories = new List<MovieCategory>()
            },
            new()
            {
                MovieId = 3,
                Title = "Memento",
                Description = "A man with short-term memory loss attempts to track down a murderer.",
                ReleaseYear = 2000,
                RentalRate = 2.99m,
                RentalDuration = 3,
                Length = 113,
                ReplacementCost = 14.99m,
                Rating = "R",
                LanguageId = languageId,
                Language = lang,
                MovieCategories = new List<MovieCategory>()
            }
        };
    }

    // =========================================================
    // GetAllLanguagesAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllLanguagesAsync_WhenLanguagesExist_ReturnsPaginatedList()
    {
        // Arrange
        var languages = CreateSampleLanguages();
        _languageRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(languages.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetAllLanguagesAsync(pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllLanguagesAsync_WithSearchTerm_FiltersByName()
    {
        // Arrange
        var languages = CreateSampleLanguages();
        _languageRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(languages.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "spa" };

        // Act
        var result = await _sut.GetAllLanguagesAsync(pagination);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(l => l.Name == "Spanish");
    }

    [Theory]
    [InlineData("name", "asc", "English")]
    [InlineData("name", "desc", "Spanish")]
    [InlineData("default", "desc", "Spanish")]
    public async Task GetAllLanguagesAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirst)
    {
        // Arrange
        var languages = CreateSampleLanguages();
        _languageRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(languages.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        // Act
        var result = await _sut.GetAllLanguagesAsync(pagination);

        // Assert
        result.Data.First().Name.Should().Be(expectedFirst);
    }

    [Fact]
    public async Task GetAllLanguagesAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Language>();
        _languageRepositoryMock.Setup(r => r.GetAllLanguages()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetAllLanguagesAsync(pagination);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetLanguageByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetLanguageByIdAsync_WhenLanguageExists_ReturnsLanguageDto()
    {
        // Arrange
        var lang = new Language { LanguageId = 1, Name = "English", Movies = new List<Movie>() };
        _languageRepositoryMock.Setup(r => r.GetLanguageByIdAsync(1)).ReturnsAsync(lang);

        // Act
        var result = await _sut.GetLanguageByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.LanguageId.Should().Be(1);
        result.Name.Should().Be("English");
    }

    [Fact]
    public async Task GetLanguageByIdAsync_WhenLanguageDoesNotExist_ReturnsNull()
    {
        // Arrange
        _languageRepositoryMock.Setup(r => r.GetLanguageByIdAsync(99)).ReturnsAsync((Language?)null);

        // Act
        var result = await _sut.GetLanguageByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateLanguageAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateLanguageAsync_ValidDto_TrimsNameSetsTimestampAndReturnsDto()
    {
        // Arrange
        var dto = new CreateLanguageDto { Name = "  Japanese  " };

        _languageRepositoryMock.Setup(r => r.CreateLanguageAsync(It.IsAny<Language>()))
            .ReturnsAsync((Language l) =>
            {
                l.LanguageId = 4;
                l.Movies = new List<Movie>();
                return l;
            });

        // Act
        var result = await _sut.CreateLanguageAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.LanguageId.Should().Be(4);
        result.Name.Should().Be("Japanese");

        _languageRepositoryMock.Verify(r => r.CreateLanguageAsync(It.Is<Language>(l =>
            l.Name == "Japanese" && l.LastUpdate != default
        )), Times.Once);
    }

    // =========================================================
    // UpdateLanguageAsync Tests
    // =========================================================

    [Fact]
    public async Task UpdateLanguageAsync_WhenLanguageExists_TrimsNameAndReturnsUpdatedDto()
    {
        // Arrange
        var dto = new UpdateLanguageDto { LanguageId = 2, Name = "  Castilian Spanish  " };

        _languageRepositoryMock.Setup(r => r.UpdateLanguageAsync(It.IsAny<Language>()))
            .ReturnsAsync((Language l) =>
            {
                l.Movies = new List<Movie>();
                return l;
            });

        // Act
        var result = await _sut.UpdateLanguageAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.LanguageId.Should().Be(2);
        result.Name.Should().Be("Castilian Spanish");

        _languageRepositoryMock.Verify(r => r.UpdateLanguageAsync(It.Is<Language>(l =>
            l.LanguageId == 2 && l.Name == "Castilian Spanish"
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateLanguageAsync_WhenLanguageDoesNotExist_ReturnsNull()
    {
        // Arrange
        var dto = new UpdateLanguageDto { LanguageId = 999, Name = "Unknown" };

        _languageRepositoryMock.Setup(r => r.UpdateLanguageAsync(It.IsAny<Language>()))
            .ReturnsAsync((Language?)null);

        // Act
        var result = await _sut.UpdateLanguageAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // DeleteLanguageAsync Tests
    // =========================================================

    [Fact]
    public async Task DeleteLanguageAsync_WhenDeleted_ReturnsTrue()
    {
        // Arrange
        _languageRepositoryMock.Setup(r => r.DeleteLanguageAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteLanguageAsync(1);

        // Assert
        result.Should().BeTrue();
        _languageRepositoryMock.Verify(r => r.DeleteLanguageAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteLanguageAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        _languageRepositoryMock.Setup(r => r.DeleteLanguageAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteLanguageAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    // =========================================================
    // GetMoviesByLanguageAsync Tests
    // =========================================================

    [Fact]
    public async Task GetMoviesByLanguageAsync_WhenMoviesExist_ReturnsPaginatedMovies()
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _languageRepositoryMock.Setup(r => r.GetMoviesByLanguageId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetMoviesByLanguageAsync(1, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMoviesByLanguageAsync_WithSearchMatchingTitle_FiltersMovies()
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _languageRepositoryMock.Setup(r => r.GetMoviesByLanguageId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "Avatar" };

        // Act
        var result = await _sut.GetMoviesByLanguageAsync(1, pagination);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(m => m.Title == "Avatar");
    }

    [Fact]
    public async Task GetMoviesByLanguageAsync_WithSearchMatchingDescription_FiltersMovies()
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _languageRepositoryMock.Setup(r => r.GetMoviesByLanguageId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "memory loss" };

        // Act
        var result = await _sut.GetMoviesByLanguageAsync(1, pagination);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(m => m.Title == "Memento");
    }

    [Theory]
    [InlineData("title", "asc", "Avatar")]
    [InlineData("releaseyear", "desc", "Inception")]
    [InlineData("default", "desc", "Memento")]
    public async Task GetMoviesByLanguageAsync_WithSorting_OrdersMoviesCorrectly(string sortBy, string sortOrder, string expectedFirst)
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _languageRepositoryMock.Setup(r => r.GetMoviesByLanguageId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        // Act
        var result = await _sut.GetMoviesByLanguageAsync(1, pagination);

        // Assert
        result.Data.First().Title.Should().Be(expectedFirst);
    }

    [Fact]
    public async Task GetMoviesByLanguageAsync_WhenNoMoviesExist_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Movie>();
        _languageRepositoryMock.Setup(r => r.GetMoviesByLanguageId(99)).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetMoviesByLanguageAsync(99, pagination);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }
}
