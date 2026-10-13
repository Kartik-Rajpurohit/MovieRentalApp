using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Stores;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class StoreServiceTests
{
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly StoreService _sut;

    public StoreServiceTests()
    {
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _sut = new StoreService(_storeRepositoryMock.Object);
    }

    private static List<Store> CreateSampleStores()
    {
        var country1 = new Country { CountryId = 1, Name = "United States" };
        var country2 = new Country { CountryId = 2, Name = "Canada" };

        var city1 = new City { CityId = 1, Name = "New York", Country = country1 };
        var city2 = new City { CityId = 2, Name = "Toronto", Country = country2 };

        var address1 = new Address { AddressId = 1, Street = "100 5th Ave", City = city1, Phone = "123456" };
        var address2 = new Address { AddressId = 2, Street = "200 Queen St", City = city2, Phone = "654321" };

        var user1 = new User { UserId = 1, FirstName = "Alice", LastName = "Smith" };
        var user2 = new User { UserId = 2, FirstName = "Bob", LastName = "Jones" };

        var manager1 = new Staff { StaffId = 1, User = user1 };
        var manager2 = new Staff { StaffId = 2, User = user2 };

        return new List<Store>
        {
            new()
            {
                StoreId = 1,
                ManagerStaffId = 1,
                ManagerStaff = manager1,
                AddressId = 1,
                Address = address1,
                Staff = new List<Staff> { manager1 },
                Customers = new List<Customer> { new(), new() },
                Inventories = new List<Inventory> { new(), new(), new() },
                LastUpdate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                StoreId = 2,
                ManagerStaffId = 2,
                ManagerStaff = manager2,
                AddressId = 2,
                Address = address2,
                Staff = new List<Staff> { manager2 },
                Customers = new List<Customer> { new() },
                Inventories = new List<Inventory> { new() },
                LastUpdate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            }
        };
    }

    // =========================================================
    // GetAllStoresAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllStoresAsync_WhenStoresExist_ReturnsStoreList()
    {
        // Arrange
        var stores = CreateSampleStores();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(stores.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StoreFilterDto();

        // Act
        var result = await _sut.GetAllStoresAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(2);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllStoresAsync_WithSearchTermMatchingCity_FiltersByCity()
    {
        // Arrange
        var stores = CreateSampleStores();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(stores.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "toronto" };
        var filter = new StoreFilterDto();

        // Act
        var result = await _sut.GetAllStoresAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.CityName == "Toronto");
    }

    [Fact]
    public async Task GetAllStoresAsync_WithSearchTermMatchingCountry_FiltersByCountry()
    {
        // Arrange
        var stores = CreateSampleStores();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(stores.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "canada" };
        var filter = new StoreFilterDto();

        // Act
        var result = await _sut.GetAllStoresAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.CountryName == "Canada");
    }

    [Fact]
    public async Task GetAllStoresAsync_WithSearchTermMatchingManager_FiltersByManager()
    {
        // Arrange
        var stores = CreateSampleStores();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(stores.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "Alice Smith" };
        var filter = new StoreFilterDto();

        // Act
        var result = await _sut.GetAllStoresAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.ManagerName == "Alice Smith");
    }

    [Fact]
    public async Task GetAllStoresAsync_WithCityFilter_FiltersByCity()
    {
        // Arrange
        var stores = CreateSampleStores();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(stores.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StoreFilterDto { City = "New York" };

        // Act
        var result = await _sut.GetAllStoresAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.CityName == "New York");
    }

    [Fact]
    public async Task GetAllStoresAsync_WithCountryFilter_FiltersByCountry()
    {
        // Arrange
        var stores = CreateSampleStores();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(stores.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StoreFilterDto { Country = "United States" };

        // Act
        var result = await _sut.GetAllStoresAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.CountryName == "United States");
    }

    [Theory]
    [InlineData("city", "asc", "New York")]
    [InlineData("country", "asc", "Toronto")]
    [InlineData("default", "desc", "Toronto")]
    public async Task GetAllStoresAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirstCity)
    {
        // Arrange
        var stores = CreateSampleStores();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(stores.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new StoreFilterDto();

        // Act
        var result = await _sut.GetAllStoresAsync(pagination, filter);

        // Assert
        result.Data.First().CityName.Should().Be(expectedFirstCity);
    }

    [Fact]
    public async Task GetAllStoresAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Store>();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new StoreFilterDto();

        // Act
        var result = await _sut.GetAllStoresAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetStoreByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetStoreByIdAsync_WhenStoreExists_ReturnsStoreDetailDto()
    {
        // Arrange
        var stores = CreateSampleStores();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(stores.BuildMock());

        // Act
        var result = await _sut.GetStoreByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.StoreId.Should().Be(1);
        result.ManagerName.Should().Be("Alice Smith");
        result.CityName.Should().Be("New York");
        result.CountryName.Should().Be("United States");
        result.TotalStaff.Should().Be(1);
        result.TotalCustomers.Should().Be(2);
        result.TotalInventory.Should().Be(3);
    }

    [Fact]
    public async Task GetStoreByIdAsync_WhenStoreDoesNotExist_ReturnsNull()
    {
        // Arrange
        var stores = CreateSampleStores();
        _storeRepositoryMock.Setup(r => r.GetAllStores()).Returns(stores.BuildMock());

        // Act
        var result = await _sut.GetStoreByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateStoreAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateStoreAsync_ValidDto_CreatesStoreSetsTimestampAndReturnsDto()
    {
        // Arrange
        var dto = new CreateStoreDto
        {
            ManagerStaffId = 1,
            AddressId = 5
        };

        _storeRepositoryMock.Setup(r => r.StaffExistsAsync(1)).ReturnsAsync(true);
        _storeRepositoryMock.Setup(r => r.AddressExistsAsync(5)).ReturnsAsync(true);

        _storeRepositoryMock.Setup(r => r.CreateStoreAsync(It.IsAny<Store>()))
            .ReturnsAsync((Store s) =>
            {
                s.StoreId = 10;
                s.Staff = new List<Staff>();
                s.Customers = new List<Customer>();
                s.Inventories = new List<Inventory>();
                return s;
            });

        // Act
        var result = await _sut.CreateStoreAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.StoreId.Should().Be(10);
        result.ManagerStaffId.Should().Be(1);

        _storeRepositoryMock.Verify(r => r.CreateStoreAsync(It.Is<Store>(s =>
            s.ManagerStaffId == 1 &&
            s.AddressId == 5 &&
            s.LastUpdate != default
        )), Times.Once);
    }

    [Fact]
    public async Task CreateStoreAsync_WhenManagerStaffIdNullOrZero_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateStoreDto
        {
            ManagerStaffId = null,
            AddressId = 5
        };

        // Act
        var act = () => _sut.CreateStoreAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Manager staff ID is required*");
        _storeRepositoryMock.Verify(r => r.CreateStoreAsync(It.IsAny<Store>()), Times.Never);
    }

    [Fact]
    public async Task CreateStoreAsync_WhenStaffDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateStoreDto
        {
            ManagerStaffId = 99,
            AddressId = 5
        };

        _storeRepositoryMock.Setup(r => r.StaffExistsAsync(99)).ReturnsAsync(false);

        // Act
        var act = () => _sut.CreateStoreAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Staff member with ID 99 does not exist*");
        _storeRepositoryMock.Verify(r => r.CreateStoreAsync(It.IsAny<Store>()), Times.Never);
    }

    [Fact]
    public async Task CreateStoreAsync_WhenAddressDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateStoreDto
        {
            ManagerStaffId = 1,
            AddressId = 99
        };

        _storeRepositoryMock.Setup(r => r.StaffExistsAsync(1)).ReturnsAsync(true);
        _storeRepositoryMock.Setup(r => r.AddressExistsAsync(99)).ReturnsAsync(false);

        // Act
        var act = () => _sut.CreateStoreAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Address with ID 99 does not exist*");
        _storeRepositoryMock.Verify(r => r.CreateStoreAsync(It.IsAny<Store>()), Times.Never);
    }
}
