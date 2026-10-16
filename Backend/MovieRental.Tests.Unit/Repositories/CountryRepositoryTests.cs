using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class CountryRepositoryTests
{
    [Fact]
    public async Task GetAllCountries_ReturnsActiveCountries()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Countries.AddRange(
            new Country { CountryId = 1, Name = "Canada", IsDeleted = false },
            new Country { CountryId = 2, Name = "Mexico", IsDeleted = false }
        );
        await context.SaveChangesAsync();

        var sut = new CountryRepository(context);

        // Act
        var countries = await sut.GetAllCountries().ToListAsync();

        // Assert
        countries.Should().HaveCount(2);
        countries.Select(c => c.Name).Should().Contain(new[] { "Canada", "Mexico" });
    }

    [Fact]
    public async Task GetCountryByIdAsync_WhenExists_ReturnsCountryWithCities()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country
        {
            CountryId = 1,
            Name = "USA",
            Cities = new List<City>
            {
                new() { CityId = 10, Name = "New York" }
            }
        };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        var sut = new CountryRepository(context);

        // Act
        var result = await sut.GetCountryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("USA");
        result.Cities.Should().HaveCount(1);
        result.Cities.First().Name.Should().Be("New York");
    }

    [Fact]
    public async Task GetCountryByIdAsync_WhenNotFound_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var sut = new CountryRepository(context);

        // Act
        var result = await sut.GetCountryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("India", true)]
    [InlineData("india", true)]
    [InlineData("  INDIA  ", true)]
    [InlineData("Germany", false)]
    public async Task GetCountryByNameAsync_PerformsTrimmedCaseInsensitiveMatch(string queryName, bool shouldFind)
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Countries.Add(new Country { CountryId = 1, Name = "India" });
        await context.SaveChangesAsync();

        var sut = new CountryRepository(context);

        // Act
        var result = await sut.GetCountryByNameAsync(queryName);

        // Assert
        if (shouldFind)
        {
            result.Should().NotBeNull();
            result!.CountryId.Should().Be(1);
        }
        else
        {
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task CreateCountryAsync_AddsAndReturnsCountry()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var sut = new CountryRepository(context);
        var country = new Country { Name = "France", LastUpdate = DateTime.UtcNow };

        // Act
        var created = await sut.CreateCountryAsync(country);

        // Assert
        created.CountryId.Should().BeGreaterThan(0);
        var inDb = await context.Countries.FindAsync(created.CountryId);
        inDb.Should().NotBeNull();
        inDb!.Name.Should().Be("France");
    }

    [Fact]
    public async Task UpdateCountryAsync_WhenExists_UpdatesFields()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "United Kingdom", LastUpdate = DateTime.UtcNow.AddDays(-1) };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        var sut = new CountryRepository(context);
        var updateModel = new Country { CountryId = 1, Name = "UK" };

        // Act
        var updated = await sut.UpdateCountryAsync(updateModel);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("UK");
        var inDb = await context.Countries.FindAsync(1);
        inDb!.Name.Should().Be("UK");
    }

    [Fact]
    public async Task UpdateCountryAsync_WhenNotFoundOrDeleted_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Countries.Add(new Country { CountryId = 2, Name = "Old", IsDeleted = true });
        await context.SaveChangesAsync();

        var sut = new CountryRepository(context);

        // Act
        var result = await sut.UpdateCountryAsync(new Country { CountryId = 2, Name = "New" });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCountryAsync_WhenExists_SoftDeletesCountry()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Countries.Add(new Country { CountryId = 1, Name = "Spain", IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new CountryRepository(context);

        // Act
        var success = await sut.DeleteCountryAsync(1);

        // Assert
        success.Should().BeTrue();
        var inDb = await context.Countries.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CountryId == 1);
        inDb.Should().NotBeNull();
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCountryAsync_WhenNotFoundOrAlreadyDeleted_ReturnsFalse()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Countries.Add(new Country { CountryId = 1, Name = "AlreadyDeleted", IsDeleted = true });
        await context.SaveChangesAsync();

        var sut = new CountryRepository(context);

        // Act
        var success = await sut.DeleteCountryAsync(1);
        var notFoundSuccess = await sut.DeleteCountryAsync(999);

        // Assert
        success.Should().BeFalse();
        notFoundSuccess.Should().BeFalse();
    }
}
