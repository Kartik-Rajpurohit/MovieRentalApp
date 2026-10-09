using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Addresses;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class AddressServiceTests
{
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly AddressService _sut;

    public AddressServiceTests()
    {
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _sut = new AddressService(_addressRepositoryMock.Object);
    }

    private static List<Address> CreateSampleAddresses()
    {
        var country = new Country { CountryId = 1, Name = "United States" };
        var city1 = new City { CityId = 1, Name = "New York", Country = country };
        var city2 = new City { CityId = 2, Name = "Los Angeles", Country = country };

        return new List<Address>
        {
            new()
            {
                AddressId = 1,
                Street = "123 Broadway",
                PostalCode = "10001",
                Phone = "1234567890",
                CityId = 1,
                City = city1,
                LastUpdate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                AddressId = 2,
                Street = "456 Sunset Blvd",
                PostalCode = "90001",
                Phone = "9876543210",
                CityId = 2,
                City = city2,
                LastUpdate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                AddressId = 3,
                Street = "789 Wall St",
                PostalCode = "10005",
                Phone = "5555555555",
                CityId = 1,
                City = city1,
                LastUpdate = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc)
            }
        };
    }

    // =========================================================
    // GetAllAddressesAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllAddressesAsync_WhenAddressesExist_ReturnsAddressList()
    {
        // Arrange
        var addresses = CreateSampleAddresses();
        _addressRepositoryMock.Setup(r => r.GetAllAddresses()).Returns(addresses.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new AddressFilterDto();

        // Act
        var result = await _sut.GetAllAddressesAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAddressesAsync_WithSearchTermMatchingStreet_FiltersByStreet()
    {
        // Arrange
        var addresses = CreateSampleAddresses();
        _addressRepositoryMock.Setup(r => r.GetAllAddresses()).Returns(addresses.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "sunset" };
        var filter = new AddressFilterDto();

        // Act
        var result = await _sut.GetAllAddressesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(a => a.Street == "456 Sunset Blvd");
    }

    [Fact]
    public async Task GetAllAddressesAsync_WithSearchTermMatchingCity_FiltersByCity()
    {
        // Arrange
        var addresses = CreateSampleAddresses();
        _addressRepositoryMock.Setup(r => r.GetAllAddresses()).Returns(addresses.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "los angeles" };
        var filter = new AddressFilterDto();

        // Act
        var result = await _sut.GetAllAddressesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(a => a.CityName == "Los Angeles");
    }

    [Fact]
    public async Task GetAllAddressesAsync_WithCityIdFilter_FiltersByCityId()
    {
        // Arrange
        var addresses = CreateSampleAddresses();
        _addressRepositoryMock.Setup(r => r.GetAllAddresses()).Returns(addresses.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new AddressFilterDto { CityId = 2 };

        // Act
        var result = await _sut.GetAllAddressesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(a => a.CityId == 2);
    }

    [Fact]
    public async Task GetAllAddressesAsync_WithPostalCodeFilter_FiltersByPostalCode()
    {
        // Arrange
        var addresses = CreateSampleAddresses();
        _addressRepositoryMock.Setup(r => r.GetAllAddresses()).Returns(addresses.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new AddressFilterDto { PostalCode = "10005" };

        // Act
        var result = await _sut.GetAllAddressesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(a => a.PostalCode == "10005");
    }

    [Theory]
    [InlineData("street", "asc", "123 Broadway")]
    [InlineData("street", "desc", "789 Wall St")]
    [InlineData("default", "desc", "789 Wall St")]
    public async Task GetAllAddressesAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirstStreet)
    {
        // Arrange
        var addresses = CreateSampleAddresses();
        _addressRepositoryMock.Setup(r => r.GetAllAddresses()).Returns(addresses.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new AddressFilterDto();

        // Act
        var result = await _sut.GetAllAddressesAsync(pagination, filter);

        // Assert
        result.Data.First().Street.Should().Be(expectedFirstStreet);
    }

    [Fact]
    public async Task GetAllAddressesAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Address>();
        _addressRepositoryMock.Setup(r => r.GetAllAddresses()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new AddressFilterDto();

        // Act
        var result = await _sut.GetAllAddressesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetAddressByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAddressByIdAsync_WhenAddressExists_ReturnsAddressDetailDto()
    {
        // Arrange
        var country = new Country { CountryId = 1, Name = "United States" };
        var city = new City { CityId = 1, Name = "New York", Country = country };
        var address = new Address
        {
            AddressId = 1,
            Street = "123 Broadway",
            PostalCode = "10001",
            Phone = "1234567890",
            CityId = 1,
            City = city,
            Users = new List<User>(),
            Stores = new List<Store>()
        };

        _addressRepositoryMock.Setup(r => r.GetAddressByIdAsync(1)).ReturnsAsync(address);

        // Act
        var result = await _sut.GetAddressByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.AddressId.Should().Be(1);
        result.Street.Should().Be("123 Broadway");
        result.CityName.Should().Be("New York");
        result.CountryName.Should().Be("United States");
    }

    [Fact]
    public async Task GetAddressByIdAsync_WhenAddressDoesNotExist_ReturnsNull()
    {
        // Arrange
        _addressRepositoryMock.Setup(r => r.GetAddressByIdAsync(999)).ReturnsAsync((Address?)null);

        // Act
        var result = await _sut.GetAddressByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateAddressAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateAddressAsync_WhenCityDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateAddressDto
        {
            Street = "10 Downing St",
            PostalCode = "SW1A 2AA",
            Phone = "123456",
            CityId = 999
        };

        _addressRepositoryMock.Setup(r => r.CityExistsAsync(999)).ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _sut.CreateAddressAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("City with ID 999 does not exist or has been deleted.");

        _addressRepositoryMock.Verify(r => r.CreateAddressAsync(It.IsAny<Address>()), Times.Never);
    }

    [Fact]
    public async Task CreateAddressAsync_WhenCityExists_CreatesAddressSetsTimestampAndReturnsDto()
    {
        // Arrange
        var country = new Country { CountryId = 1, Name = "United States" };
        var city = new City { CityId = 1, Name = "New York", Country = country };

        var dto = new CreateAddressDto
        {
            Street = "350 5th Ave",
            PostalCode = "10118",
            Phone = "2127363100",
            CityId = 1
        };

        _addressRepositoryMock.Setup(r => r.CityExistsAsync(1)).ReturnsAsync(true);

        _addressRepositoryMock.Setup(r => r.CreateAddressAsync(It.IsAny<Address>()))
            .ReturnsAsync((Address a) =>
            {
                a.AddressId = 10;
                a.City = city;
                return a;
            });

        // Act
        var result = await _sut.CreateAddressAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.AddressId.Should().Be(10);
        result.Street.Should().Be("350 5th Ave");
        result.CityName.Should().Be("New York");

        _addressRepositoryMock.Verify(r => r.CreateAddressAsync(It.Is<Address>(a =>
            a.Street == "350 5th Ave" &&
            a.CityId == 1 &&
            a.LastUpdate != default
        )), Times.Once);
    }
}
