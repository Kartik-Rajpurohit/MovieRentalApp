using FluentAssertions;
using MockQueryable;
using MockQueryable.EntityFrameworkCore;
using Moq;
using MovieRental.Domain.DTOs.Common;
using MovieRental.Domain.DTOs.Locations.Countries;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Interfaces;
using MovieRental.Services.Services;
using Xunit;

namespace MovieRental.Tests.Unit.Services;

public class CountryServiceTests
{
    private readonly Mock<ICountryRepository> _countryRepositoryMock;
    private readonly CountryService _sut;

    public CountryServiceTests()
    {
        _countryRepositoryMock = new Mock<ICountryRepository>();
        _sut = new CountryService(_countryRepositoryMock.Object);
    }

    private static List<Country> CreateSampleCountries()
    {
        return new List<Country>
        {
            new()
            {
                CountryId = 1,
                Name = "Canada",
                LastUpdate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Cities = new List<City> { new(), new() }
            },
            new()
            {
                CountryId = 2,
                Name = "Japan",
                LastUpdate = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc),
                Cities = new List<City> { new() }
            },
            new()
            {
                CountryId = 3,
                Name = "Germany",
                LastUpdate = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc),
                Cities = new List<City>()
            }
        };
    }

    // =========================================================
    // GetAllCountriesAsync Tests
    // =========================================================

    [Fact]
    public async Task GetAllCountriesAsync_WhenCountriesExist_ReturnsCountryList()
    {
        // Arrange
        var countries = CreateSampleCountries();
        _countryRepositoryMock.Setup(r => r.GetAllCountries()).Returns(countries.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CountryFilterDto();

        // Act
        var result = await _sut.GetAllCountriesAsync(pagination, filter);

        // Assert
        result.Should().NotBeNull();
        result.TotalRecords.Should().Be(3);
        result.TotalPages.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllCountriesAsync_WithSearchTerm_FiltersByName()
    {
        // Arrange
        var countries = CreateSampleCountries();
        _countryRepositoryMock.Setup(r => r.GetAllCountries()).Returns(countries.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10, Search = "jap" };
        var filter = new CountryFilterDto();

        // Act
        var result = await _sut.GetAllCountriesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.Name == "Japan");
    }

    [Fact]
    public async Task GetAllCountriesAsync_WithNameFilter_FiltersByName()
    {
        // Arrange
        var countries = CreateSampleCountries();
        _countryRepositoryMock.Setup(r => r.GetAllCountries()).Returns(countries.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CountryFilterDto { Name = "Ger" };

        // Act
        var result = await _sut.GetAllCountriesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.Name == "Germany");
    }

    [Theory]
    [InlineData("name", "asc", "Canada")]
    [InlineData("name", "desc", "Japan")]
    [InlineData("default", "desc", "Japan")]
    public async Task GetAllCountriesAsync_WithSorting_OrdersResultsCorrectly(string sortBy, string sortOrder, string expectedFirst)
    {
        // Arrange
        var countries = CreateSampleCountries();
        _countryRepositoryMock.Setup(r => r.GetAllCountries()).Returns(countries.BuildMock());

        var pagination = new PaginationInputDto
        {
            Page = 1,
            PageSize = 10,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
        var filter = new CountryFilterDto();

        // Act
        var result = await _sut.GetAllCountriesAsync(pagination, filter);

        // Assert
        result.Data.First().Name.Should().Be(expectedFirst);
    }

    [Fact]
    public async Task GetAllCountriesAsync_WhenEmpty_ReturnsZeroTotals()
    {
        // Arrange
        var empty = new List<Country>();
        _countryRepositoryMock.Setup(r => r.GetAllCountries()).Returns(empty.BuildMock());

        var pagination = new PaginationInputDto { Page = 1, PageSize = 10 };
        var filter = new CountryFilterDto();

        // Act
        var result = await _sut.GetAllCountriesAsync(pagination, filter);

        // Assert
        result.TotalRecords.Should().Be(0);
        result.TotalPages.Should().Be(0);
        result.Data.Should().BeEmpty();
    }

    // =========================================================
    // GetCountryByIdAsync Tests
    // =========================================================

    [Fact]
    public async Task GetCountryByIdAsync_WhenCountryExists_ReturnsCountryDto()
    {
        // Arrange
        var country = new Country { CountryId = 1, Name = "Canada", Cities = new List<City>() };
        _countryRepositoryMock.Setup(r => r.GetCountryByIdAsync(1)).ReturnsAsync(country);

        // Act
        var result = await _sut.GetCountryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.CountryId.Should().Be(1);
        result.Name.Should().Be("Canada");
    }

    [Fact]
    public async Task GetCountryByIdAsync_WhenCountryDoesNotExist_ReturnsNull()
    {
        // Arrange
        _countryRepositoryMock.Setup(r => r.GetCountryByIdAsync(999)).ReturnsAsync((Country?)null);

        // Act
        var result = await _sut.GetCountryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // CreateCountryAsync Tests
    // =========================================================

    [Fact]
    public async Task CreateCountryAsync_ValidDto_CreatesCountrySetsTimestampAndReturnsDto()
    {
        // Arrange
        var dto = new CreateCountryDto { Name = "Australia" };

        _countryRepositoryMock.Setup(r => r.CreateCountryAsync(It.IsAny<Country>()))
            .ReturnsAsync((Country c) =>
            {
                c.CountryId = 4;
                c.Cities = new List<City>();
                return c;
            });

        // Act
        var result = await _sut.CreateCountryAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.CountryId.Should().Be(4);
        result.Name.Should().Be("Australia");

        _countryRepositoryMock.Verify(r => r.CreateCountryAsync(It.Is<Country>(c =>
            c.Name == "Australia" && c.LastUpdate != default
        )), Times.Once);
    }

    // =========================================================
    // UpdateCountryAsync Tests
    // =========================================================

    [Fact]
    public async Task UpdateCountryAsync_WhenCountryExists_UpdatesAndReturnsUpdatedDto()
    {
        // Arrange
        var dto = new UpdateCountryDto { CountryId = 2, Name = "Japan (Updated)" };

        _countryRepositoryMock.Setup(r => r.UpdateCountryAsync(It.IsAny<Country>()))
            .ReturnsAsync((Country c) =>
            {
                c.Cities = new List<City>();
                return c;
            });

        // Act
        var result = await _sut.UpdateCountryAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.CountryId.Should().Be(2);
        result.Name.Should().Be("Japan (Updated)");

        _countryRepositoryMock.Verify(r => r.UpdateCountryAsync(It.Is<Country>(c =>
            c.CountryId == 2 && c.Name == "Japan (Updated)"
        )), Times.Once);
    }

    [Fact]
    public async Task UpdateCountryAsync_WhenRepositoryReturnsNull_ReturnsNull()
    {
        // Arrange
        var dto = new UpdateCountryDto { CountryId = 999, Name = "Ghost Country" };

        _countryRepositoryMock.Setup(r => r.UpdateCountryAsync(It.IsAny<Country>()))
            .ReturnsAsync((Country?)null);

        // Act
        var result = await _sut.UpdateCountryAsync(dto);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================
    // DeleteCountryAsync Tests
    // =========================================================

    [Fact]
    public async Task DeleteCountryAsync_WhenSuccessful_ReturnsTrue()
    {
        // Arrange
        _countryRepositoryMock.Setup(r => r.DeleteCountryAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteCountryAsync(1);

        // Assert
        result.Should().BeTrue();
        _countryRepositoryMock.Verify(r => r.DeleteCountryAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteCountryAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        _countryRepositoryMock.Setup(r => r.DeleteCountryAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteCountryAsync(999);

        // Assert
        result.Should().BeFalse();
    }
}
