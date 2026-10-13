using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Inventory;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class InventoryServiceTests
{
    private readonly Mock<IInventoryRepository> _inventoryRepositoryMock;
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _inventoryRepositoryMock = new Mock<IInventoryRepository>();
        _sut = new InventoryService(_inventoryRepositoryMock.Object);
    }

    private static List<Inventory> CreateSampleInventory()
    {
        var movie1 = new Movie { MovieId = 1, Title = "Inception", RentalRate = 2.99m };
        var movie2 = new Movie { MovieId = 2, Title = "The Dark Knight", RentalRate = 3.99m };

        return new List<Inventory>
        {
            new()
            {
                InventoryId = 1,
                MovieId = 1,
                Movie = movie1,
                StoreId = 1,
                LastUpdate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Rentals = new List<Rental>() // Available (no unreturned rentals)
            },
            new()
            {
                InventoryId = 2,
                MovieId = 1,
                Movie = movie1,
                StoreId = 2,
                LastUpdate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                Rentals = new List<Rental>
                {
                    new() { RentalId = 101, ReturnDate = null } // Rented out (ReturnDate is null)
                }
            },
            new()
            {
                InventoryId = 3,
                MovieId = 2,
                Movie = movie2,
                StoreId = 1,
                LastUpdate = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                Rentals = new List<Rental>
                {
                    new() { RentalId = 102, ReturnDate = new DateTime(2025, 1, 5, 0, 0, 0, DateTimeKind.Utc) } // Returned -> available
                }
            }
        };
    }

    // =========================================================
    // GetAllInventoryAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllInventoryAsync_WhenInventoryExists_ReturnsInventoryList()
    {
        // Arrange
        var inventory = CreateSampleInventory();
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(inventory.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new InventoryFilterDto();

        // Act
        var result = await _sut.GetAllInventoryAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.Data.Should().HaveCount(3);
        result.Data.Should().Contain(i => i.InventoryId == 1 && i.MovieTitle == "Inception" && i.IsAvailable);
        result.Data.Should().Contain(i => i.InventoryId == 2 && !i.IsAvailable);
    }

    [Theory]
    [InlineData("Inception", 2)]
    [InlineData("Dark", 1)]
    [InlineData("2", 1)] // Search by inventory ID string
    [InlineData("NonExistent", 0)]
    public async Task GetAllInventoryAsync_WithSearch_FiltersCorrectly(string searchTerm, int expectedCount)
    {
        // Arrange
        var inventory = CreateSampleInventory();
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(inventory.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = searchTerm };
        var filter = new InventoryFilterDto();

        // Act
        var result = await _sut.GetAllInventoryAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetAllInventoryAsync_WithMovieAndStoreFilter_FiltersResults()
    {
        // Arrange
        var inventory = CreateSampleInventory();
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(inventory.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new InventoryFilterDto { MovieId = 1, StoreId = 1 };

        // Act
        var result = await _sut.GetAllInventoryAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.First().InventoryId.Should().Be(1);
    }

    [Fact]
    public async Task GetAllInventoryAsync_WithIsAvailableFilter_ReturnsOnlyMatchingCopies()
    {
        // Arrange
        var inventory = CreateSampleInventory();
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(inventory.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new InventoryFilterDto { IsAvailable = true };

        // Act
        var result = await _sut.GetAllInventoryAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(2); // Inventory 1 and 3
        result.Data.Should().OnlyContain(i => i.IsAvailable);
    }

    [Theory]
    [InlineData("movietitle", "asc", 1)] // Inception (id 1)
    [InlineData("storeid", "desc", 2)]   // Store 2 (id 2)
    [InlineData("default", "desc", 3)]   // Id desc (id 3)
    public async Task GetAllInventoryAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, int expectedFirstId)
    {
        // Arrange
        var inventory = CreateSampleInventory();
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(inventory.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new InventoryFilterDto();

        // Act
        var result = await _sut.GetAllInventoryAsync(pagination, filter);

        // Assert
        result.Data.First().InventoryId.Should().Be(expectedFirstId);
    }

    [Fact]
    public async Task GetAllInventoryAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Inventory>();
        _inventoryRepositoryMock.Setup(r => r.GetAllInventory()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new InventoryFilterDto();

        // Act
        var result = await _sut.GetAllInventoryAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetInventoryByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetInventoryByIdAsync_WhenExists_ReturnsDetailDto()
    {
        // Arrange
        var movie = new Movie { MovieId = 1, Title = "Inception" };
        var inventory = new Inventory
        {
            InventoryId = 1,
            MovieId = 1,
            Movie = movie,
            StoreId = 1,
            LastUpdate = DateTime.UtcNow,
            Rentals = new List<Rental> { new() { RentalId = 1 }, new() { RentalId = 2 } }
        };

        _inventoryRepositoryMock.Setup(r => r.GetInventoryByIdAsync(1)).ReturnsAsync(inventory);

        // Act
        var result = await _sut.GetInventoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.InventoryId.Should().Be(1);
        result.MovieTitle.Should().Be("Inception");
        result.TotalRentals.Should().Be(2);
    }

    [Fact]
    public async Task GetInventoryByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        _inventoryRepositoryMock.Setup(r => r.GetInventoryByIdAsync(999)).ReturnsAsync((Inventory?)null);

        // Act
        var result = await _sut.GetInventoryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateInventoryAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateInventoryAsync_WhenMovieAndStoreExist_CreatesAndReturnsDto()
    {
        // Arrange
        var dto = new CreateInventoryDto { MovieId = 1, StoreId = 1 };
        _inventoryRepositoryMock.Setup(r => r.MovieExistsAsync(1)).ReturnsAsync(true);
        _inventoryRepositoryMock.Setup(r => r.StoreExistsAsync(1)).ReturnsAsync(true);

        var created = new Inventory
        {
            InventoryId = 10,
            MovieId = 1,
            Movie = new Movie { MovieId = 1, Title = "Inception" },
            StoreId = 1,
            Rentals = new List<Rental>()
        };
        _inventoryRepositoryMock.Setup(r => r.CreateInventoryAsync(It.IsAny<Inventory>())).ReturnsAsync(created);

        // Act
        var result = await _sut.CreateInventoryAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.InventoryId.Should().Be(10);
        result.MovieTitle.Should().Be("Inception");
        result.StoreId.Should().Be(1);
        result.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task CreateInventoryAsync_WhenMovieDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateInventoryDto { MovieId = 999, StoreId = 1 };
        _inventoryRepositoryMock.Setup(r => r.MovieExistsAsync(999)).ReturnsAsync(false);

        // Act
        var act = () => _sut.CreateInventoryAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Movie with ID 999 does not exist*");
    }

    [Fact]
    public async Task CreateInventoryAsync_WhenStoreDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateInventoryDto { MovieId = 1, StoreId = 999 };
        _inventoryRepositoryMock.Setup(r => r.MovieExistsAsync(1)).ReturnsAsync(true);
        _inventoryRepositoryMock.Setup(r => r.StoreExistsAsync(999)).ReturnsAsync(false);

        // Act
        var act = () => _sut.CreateInventoryAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Store with ID 999 does not exist*");
    }

    // =========================================================
    // UpdateInventoryAsync Tests
    // =========================================================

    [Fact]
    public async Task UpdateInventoryAsync_WhenValid_UpdatesAndReturnsDto()
    {
        // Arrange
        var dto = new UpdateInventoryDto { InventoryId = 1, StoreId = 2 };
        _inventoryRepositoryMock.Setup(r => r.StoreExistsAsync(2)).ReturnsAsync(true);

        var updated = new Inventory
        {
            InventoryId = 1,
            MovieId = 1,
            Movie = new Movie { MovieId = 1, Title = "Inception" },
            StoreId = 2,
            Rentals = new List<Rental>()
        };
        _inventoryRepositoryMock.Setup(r => r.UpdateInventoryAsync(It.IsAny<Inventory>())).ReturnsAsync(updated);

        // Act
        var result = await _sut.UpdateInventoryAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.StoreId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateInventoryAsync_WhenStoreDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new UpdateInventoryDto { InventoryId = 1, StoreId = 999 };
        _inventoryRepositoryMock.Setup(r => r.StoreExistsAsync(999)).ReturnsAsync(false);

        // Act
        var act = () => _sut.UpdateInventoryAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Store with ID 999 does not exist*");
    }

    [Fact]
    public async Task UpdateInventoryAsync_WhenNotFoundInRepo_ReturnsNull()
    {
        // Arrange
        var dto = new UpdateInventoryDto { InventoryId = 999, StoreId = 1 };
        _inventoryRepositoryMock.Setup(r => r.StoreExistsAsync(1)).ReturnsAsync(true);
        _inventoryRepositoryMock.Setup(r => r.UpdateInventoryAsync(It.IsAny<Inventory>())).ReturnsAsync((Inventory?)null);

        // Act
        var result = await _sut.UpdateInventoryAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // DeleteInventoryAsync Tests
    // =========================================================

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteInventoryAsync_DelegatesToRepository(bool expectedResult)
    {
        // Arrange
        _inventoryRepositoryMock.Setup(r => r.DeleteInventoryAsync(1)).ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.DeleteInventoryAsync(1);

        // Assert
        result.Should().Be(expectedResult);
        _inventoryRepositoryMock.Verify(r => r.DeleteInventoryAsync(1), Times.Once);
    }
}
