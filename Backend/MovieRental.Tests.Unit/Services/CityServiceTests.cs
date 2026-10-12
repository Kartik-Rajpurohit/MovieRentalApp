using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Cities;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class CityServiceTests
{
    private readonly Mock<ICityRepository> _cityRepositoryMock;
    private readonly CityService _sut;

    public CityServiceTests()
    {
        _cityRepositoryMock = new Mock<ICityRepository>();
        _sut = new CityService(_cityRepositoryMock.Object);
    }

    private static List<City> CreateSampleCities()
    {
        var usa = new Country { CountryId = 1, Name = "United States" };
        var japan = new Country { CountryId = 2, Name = "Japan" };

        return new List<City>
        {
            new()
            {
                CityId = 1,
                Name = "New York",
                CountryId = 1,
                Country = usa,
                Addresses = new List<Address> { new(), new() },
                LastUpdate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                CityId = 2,
                Name = "Tokyo",
                CountryId = 2,
                Country = japan,
                Addresses = new List<Address> { new() },
                LastUpdate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                CityId = 3,
                Name = "Los Angeles",
                CountryId = 1,
                Country = usa,
                Addresses = new List<Address>(),
                LastUpdate = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc)
            }
        };
    }

    // =========================================================
    // GetAllCitiesAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllCitiesAsync_WhenCitiesExist_ReturnsCityList()
    {
        // Arrange
        var cities = CreateSampleCities();
        _cityRepositoryMock.Setup(r => r.GetAllCities()).Returns(cities.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CityFilterDto();

        // Act
        var result = await _sut.GetAllCitiesAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllCitiesAsync_WithSearchTermMatchingCity_FiltersByCity()
    {
        // Arrange
        var cities = CreateSampleCities();
        _cityRepositoryMock.Setup(r => r.GetAllCities()).Returns(cities.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "tokyo" };
        var filter = new CityFilterDto();

        // Act
        var result = await _sut.GetAllCitiesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.Name == "Tokyo");
    }

    [Fact]
    public async Task GetAllCitiesAsync_WithSearchTermMatchingCountry_FiltersByCountry()
    {
        // Arrange
        var cities = CreateSampleCities();
        _cityRepositoryMock.Setup(r => r.GetAllCities()).Returns(cities.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "japan" };
        var filter = new CityFilterDto();

        // Act
        var result = await _sut.GetAllCitiesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.CountryName == "Japan");
    }

    [Fact]
    public async Task GetAllCitiesAsync_WithCountryIdFilter_FiltersByCountryId()
    {
        // Arrange
        var cities = CreateSampleCities();
        _cityRepositoryMock.Setup(r => r.GetAllCities()).Returns(cities.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CityFilterDto { CountryId = 2 };

        // Act
        var result = await _sut.GetAllCitiesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.CityId == 2 && c.Name == "Tokyo");
    }

    [Fact]
    public async Task GetAllCitiesAsync_WithNameFilter_FiltersByName()
    {
        // Arrange
        var cities = CreateSampleCities();
        _cityRepositoryMock.Setup(r => r.GetAllCities()).Returns(cities.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CityFilterDto { Name = "angeles" };

        // Act
        var result = await _sut.GetAllCitiesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.Name == "Los Angeles");
    }

    [Theory]
    [InlineData("name", "asc", "Los Angeles")]
    [InlineData("country", "desc", "Los Angeles")]
    [InlineData("default", "desc", "Los Angeles")]
    public async Task GetAllCitiesAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirst)
    {
        // Arrange
        var cities = CreateSampleCities();
        _cityRepositoryMock.Setup(r => r.GetAllCities()).Returns(cities.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new CityFilterDto();

        // Act
        var result = await _sut.GetAllCitiesAsync(pagination, filter);

        // Assert
        result.Data.First().Name.Should().Be(expectedFirst);
    }

    [Fact]
    public async Task GetAllCitiesAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<City>();
        _cityRepositoryMock.Setup(r => r.GetAllCities()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CityFilterDto();

        // Act
        var result = await _sut.GetAllCitiesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetCityByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetCityByIdAsync_WhenCityExists_ReturnsCityDetailDto()
    {
        // Arrange
        var usa = new Country { CountryId = 1, Name = "United States" };
        var city = new City
        {
            CityId = 1,
            Name = "New York",
            CountryId = 1,
            Country = usa,
            Addresses = new List<Address>(),
            LastUpdate = DateTime.UtcNow
        };

        _cityRepositoryMock.Setup(r => r.GetCityByIdAsync(1)).ReturnsAsync(city);

        // Act
        var result = await _sut.GetCityByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.CityId.Should().Be(1);
        result.Name.Should().Be("New York");
        result.CountryName.Should().Be("United States");
    }

    [Fact]
    public async Task GetCityByIdAsync_WhenCityDoesNotExist_ReturnsNull()
    {
        // Arrange
        _cityRepositoryMock.Setup(r => r.GetCityByIdAsync(999)).ReturnsAsync((City?)null);

        // Act
        var result = await _sut.GetCityByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateCityAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateCityAsync_WhenCountryDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreateCityDto { Name = "Atlantis", CountryId = 999 };

        _cityRepositoryMock.Setup(r => r.CountryExistsAsync(999)).ReturnsAsync(false);

        // Act
        Func<Task> act = async () => await _sut.CreateCityAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Country with ID 999 does not exist or has been deleted.");

        _cityRepositoryMock.Verify(r => r.CreateCityAsync(It.IsAny<City>()), Times.Never);
    }

    [Fact]
    public async Task CreateCityAsync_WhenCountryExists_CreatesCitySetsTimestampAndReturnsDto()
    {
        // Arrange
        var usa = new Country { CountryId = 1, Name = "United States" };
        var dto = new CreateCityDto { Name = "Chicago", CountryId = 1 };

        _cityRepositoryMock.Setup(r => r.CountryExistsAsync(1)).ReturnsAsync(true);

        _cityRepositoryMock.Setup(r => r.CreateCityAsync(It.IsAny<City>()))
            .ReturnsAsync((City c) =>
            {
                c.CityId = 10;
                c.Country = usa;
                c.Addresses = new List<Address>();
                return c;
            });

        // Act
        var result = await _sut.CreateCityAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.CityId.Should().Be(10);
        result.Name.Should().Be("Chicago");
        result.CountryName.Should().Be("United States");

        _cityRepositoryMock.Verify(r => r.CreateCityAsync(It.Is<City>(c =>
            c.Name == "Chicago" &&
            c.CountryId == 1 &&
            c.LastUpdate != default
        )), Times.Once);
    }
}
