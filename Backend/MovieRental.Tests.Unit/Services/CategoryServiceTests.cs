using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Categories;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Movies;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _sut = new CategoryService(_categoryRepositoryMock.Object);
    }

    private static List<Category> CreateSampleCategories()
    {
        return new List<Category>
        {
            new()
            {
                CategoryId = 1,
                Name = "Action",
                LastUpdate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                MovieCategories = new List<MovieCategory> { new(), new() }
            },
            new()
            {
                CategoryId = 2,
                Name = "Comedy",
                LastUpdate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                MovieCategories = new List<MovieCategory> { new() }
            },
            new()
            {
                CategoryId = 3,
                Name = "Drama",
                LastUpdate = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                MovieCategories = new List<MovieCategory>()
            }
        };
    }

    private static List<Movie> CreateSampleMovies(int categoryId)
    {
        var category = new Category { CategoryId = categoryId, Name = "Action" };
        return new List<Movie>
        {
            new()
            {
                MovieId = 1,
                Title = "The Dark Knight",
                Description = "When the menace known as the Joker wreaks havoc on Gotham City.",
                ReleaseYear = 2008,
                RentalRate = 4.99m,
                RentalDuration = 5,
                Length = 152,
                ReplacementCost = 21.99m,
                Rating = "PG-13",
                MovieCategories = new List<MovieCategory>
                {
                    new() { CategoryId = categoryId, Category = category }
                }
            },
            new()
            {
                MovieId = 2,
                Title = "Gladiator",
                Description = "A former Roman General sets out to exact vengeance.",
                ReleaseYear = 2000,
                RentalRate = 2.99m,
                RentalDuration = 3,
                Length = 155,
                ReplacementCost = 15.99m,
                Rating = "R",
                MovieCategories = new List<MovieCategory>
                {
                    new() { CategoryId = categoryId, Category = category }
                }
            },
            new()
            {
                MovieId = 3,
                Title = "Mad Max: Fury Road",
                Description = "In a post-apocalyptic wasteland, a woman rebels against a tyrannical ruler.",
                ReleaseYear = 2015,
                RentalRate = 3.99m,
                RentalDuration = 4,
                Length = 120,
                ReplacementCost = 18.99m,
                Rating = "R",
                MovieCategories = new List<MovieCategory>
                {
                    new() { CategoryId = categoryId, Category = category }
                }
            }
        };
    }

    // =========================================================
    // GetAllCategoriesAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllCategoriesAsync_WhenCategoriesExist_ReturnsPaginatedList()
    {
        // Arrange
        var categories = CreateSampleCategories();
        _categoryRepositoryMock.Setup(r => r.GetAllCategories()).Returns(categories.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CategoryFilterDto();

        // Act
        var result = await _sut.GetAllCategoriesAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WithSearchTerm_FiltersByName()
    {
        // Arrange
        var categories = CreateSampleCategories();
        _categoryRepositoryMock.Setup(r => r.GetAllCategories()).Returns(categories.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "act" };
        var filter = new CategoryFilterDto();

        // Act
        var result = await _sut.GetAllCategoriesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.Name == "Action");
    }

    [Theory]
    [InlineData("name", "asc", "Action")]
    [InlineData("name", "desc", "Drama")]
    [InlineData("default", "desc", "Drama")]
    public async Task GetAllCategoriesAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirst)
    {
        // Arrange
        var categories = CreateSampleCategories();
        _categoryRepositoryMock.Setup(r => r.GetAllCategories()).Returns(categories.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new CategoryFilterDto();

        // Act
        var result = await _sut.GetAllCategoriesAsync(pagination, filter);

        // Assert
        result.Data.First().Name.Should().Be(expectedFirst);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Category>();
        _categoryRepositoryMock.Setup(r => r.GetAllCategories()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CategoryFilterDto();

        // Act
        var result = await _sut.GetAllCategoriesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetCategoryByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetCategoryByIdAsync_WhenCategoryExists_ReturnsCategoryDto()
    {
        // Arrange
        var category = new Category { CategoryId = 1, Name = "Action", MovieCategories = new List<MovieCategory>() };
        _categoryRepositoryMock.Setup(r => r.GetCategoryByIdAsync(1)).ReturnsAsync(category);

        // Act
        var result = await _sut.GetCategoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryId.Should().Be(1);
        result.Name.Should().Be("Action");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WhenCategoryDoesNotExist_ReturnsNull()
    {
        // Arrange
        _categoryRepositoryMock.Setup(r => r.GetCategoryByIdAsync(999)).ReturnsAsync((Category?)null);

        // Act
        var result = await _sut.GetCategoryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateCategoryAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateCategoryAsync_ValidDto_CreatesCategorySetsTimestampAndReturnsDto()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "Sci-Fi" };

        _categoryRepositoryMock.Setup(r => r.CreateCategoryAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) =>
            {
                c.CategoryId = 4;
                c.MovieCategories = new List<MovieCategory>();
                return c;
            });

        // Act
        var result = await _sut.CreateCategoryAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.CategoryId.Should().Be(4);
        result.Name.Should().Be("Sci-Fi");

        _categoryRepositoryMock.Verify(r => r.CreateCategoryAsync(It.Is<Category>(c =>
            c.Name == "Sci-Fi" && c.LastUpdate != default
        )), Times.Once);
    }

    // =========================================================
    // UpdateCategoryAsync Tests
    // =========================================================

    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryExists_UpdatesAndReturnsUpdatedDto()
    {
        // Arrange
        var existing = new Category { CategoryId = 2, Name = "Comedy", MovieCategories = new List<MovieCategory>() };
        _categoryRepositoryMock.Setup(r => r.GetCategoryByIdAsync(2)).ReturnsAsync(existing);

        var dto = new UpdateCategoryDto { CategoryId = 2, Name = "Romantic Comedy" };

        _categoryRepositoryMock.Setup(r => r.UpdateCategoryAsync(existing))
            .ReturnsAsync(existing);

        // Act
        var result = await _sut.UpdateCategoryAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.CategoryId.Should().Be(2);
        result.Name.Should().Be("Romantic Comedy");

        _categoryRepositoryMock.Verify(r => r.UpdateCategoryAsync(existing), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenCategoryNotFound_ReturnsNullAndDoesNotCallUpdate()
    {
        // Arrange
        _categoryRepositoryMock.Setup(r => r.GetCategoryByIdAsync(999)).ReturnsAsync((Category?)null);

        var dto = new UpdateCategoryDto { CategoryId = 999, Name = "NonExistent" };

        // Act
        var result = await _sut.UpdateCategoryAsync(dto);

        // Assert
        result.Should().BeNull();
        _categoryRepositoryMock.Verify(r => r.UpdateCategoryAsync(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCategoryAsync_WhenRepositoryUpdateReturnsNull_ReturnsNull()
    {
        // Arrange
        var existing = new Category { CategoryId = 5, Name = "Thriller" };
        _categoryRepositoryMock.Setup(r => r.GetCategoryByIdAsync(5)).ReturnsAsync(existing);
        _categoryRepositoryMock.Setup(r => r.UpdateCategoryAsync(existing)).ReturnsAsync((Category?)null);

        var dto = new UpdateCategoryDto { CategoryId = 5, Name = "Psychological Thriller" };

        // Act
        var result = await _sut.UpdateCategoryAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // DeleteCategoryAsync Tests
    // =========================================================

    [Fact]
    public async Task DeleteCategoryAsync_WhenSuccessful_ReturnsTrue()
    {
        // Arrange
        _categoryRepositoryMock.Setup(r => r.DeleteCategoryAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteCategoryAsync(1);

        // Assert
        result.Should().BeTrue();
        _categoryRepositoryMock.Verify(r => r.DeleteCategoryAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteCategoryAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        _categoryRepositoryMock.Setup(r => r.DeleteCategoryAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteCategoryAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    // =========================================================
    // GetMoviesByCategoryAsync Tests
    // =========================================================

    [Fact]
    public async Task GetMoviesByCategoryAsync_WhenMoviesExist_ReturnsPaginatedMovies()
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _categoryRepositoryMock.Setup(r => r.GetMoviesByCategoryId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetMoviesByCategoryAsync(1, pagination);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetMoviesByCategoryAsync_WithSearchMatchingTitle_FiltersMovies()
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _categoryRepositoryMock.Setup(r => r.GetMoviesByCategoryId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "Dark Knight" };

        // Act
        var result = await _sut.GetMoviesByCategoryAsync(1, pagination);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(m => m.Title == "The Dark Knight");
    }

    [Fact]
    public async Task GetMoviesByCategoryAsync_WithSearchMatchingDescription_FiltersMovies()
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _categoryRepositoryMock.Setup(r => r.GetMoviesByCategoryId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "vengeance" };

        // Act
        var result = await _sut.GetMoviesByCategoryAsync(1, pagination);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(m => m.Title == "Gladiator");
    }

    [Theory]
    [InlineData("title", "asc", "Gladiator")]
    [InlineData("releaseyear", "desc", "Mad Max: Fury Road")]
    [InlineData("default", "desc", "The Dark Knight")]
    public async Task GetMoviesByCategoryAsync_WithSorting_OrdersMoviesCorrectly(string sortBy, string sortOrder, string expectedFirst)
    {
        // Arrange
        var movies = CreateSampleMovies(1);
        _categoryRepositoryMock.Setup(r => r.GetMoviesByCategoryId(1)).Returns(movies.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };

        // Act
        var result = await _sut.GetMoviesByCategoryAsync(1, pagination);

        // Assert
        result.Data.First().Title.Should().Be(expectedFirst);
    }

    [Fact]
    public async Task GetMoviesByCategoryAsync_WhenNoMoviesExist_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Movie>();
        _categoryRepositoryMock.Setup(r => r.GetMoviesByCategoryId(999)).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };

        // Act
        var result = await _sut.GetMoviesByCategoryAsync(999, pagination);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }
}
