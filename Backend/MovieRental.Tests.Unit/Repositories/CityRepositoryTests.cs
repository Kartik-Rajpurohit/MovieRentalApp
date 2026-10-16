using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MovieRental.Domain.Entities;
using MovieRental.Repository.Repositories;
using Xunit;

namespace MovieRental.Tests.Unit.Repositories;

public class CityRepositoryTests
{
    [Fact]
    public async Task GetAllCities_ReturnsCitiesWithCountry()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "Canada" };
        context.Countries.Add(country);
        context.Cities.AddRange(
            new City { CityId = 1, Name = "Toronto", CountryId = 1, Country = country },
            new City { CityId = 2, Name = "Vancouver", CountryId = 1, Country = country }
        );
        await context.SaveChangesAsync();

        var sut = new CityRepository(context);

        // Act
        var cities = await sut.GetAllCities().ToListAsync();

        // Assert
        cities.Should().HaveCount(2);
        cities.First().Country.Should().NotBeNull();
        cities.First().Country.Name.Should().Be("Canada");
    }

    [Fact]
    public async Task GetCityByIdAsync_WhenExists_ReturnsCityWithRelations()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "USA" };
        var city = new City
        {
            CityId = 10,
            Name = "Los Angeles",
            CountryId = 1,
            Country = country,
            Addresses = new List<Address> { new() { AddressId = 100, Street = "Hollywood Blvd" } }
        };
        context.Countries.Add(country);
        context.Cities.Add(city);
        await context.SaveChangesAsync();

        var sut = new CityRepository(context);

        // Act
        var result = await sut.GetCityByIdAsync(10);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Los Angeles");
        result.Country.Should().NotBeNull();
        result.Addresses.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("Tokyo", 1, true)]
    [InlineData("tokyo", 1, true)]
    [InlineData("  TOKYO  ", 1, true)]
    [InlineData("Tokyo", 2, false)]     // Different country ID
    [InlineData("Osaka", 1, false)]     // Non-existent
    public async Task GetCityByNameAndCountryIdAsync_MatchesCorrectly(string cityName, int countryId, bool shouldFind)
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Cities.Add(new City { CityId = 1, Name = "Tokyo", CountryId = 1 });
        await context.SaveChangesAsync();

        var sut = new CityRepository(context);

        // Act
        var result = await sut.GetCityByNameAndCountryIdAsync(cityName, countryId);

        // Assert
        if (shouldFind)
        {
            result.Should().NotBeNull();
            result!.CityId.Should().Be(1);
        }
        else
        {
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task CreateCityAsync_InsertsAndReloadsCity()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country = new Country { CountryId = 1, Name = "Germany" };
        context.Countries.Add(country);
        await context.SaveChangesAsync();

        var sut = new CityRepository(context);
        var city = new City { Name = "Munich", CountryId = 1 };

        // Act
        var created = await sut.CreateCityAsync(city);

        // Assert
        created.CityId.Should().BeGreaterThan(0);
        created.Name.Should().Be("Munich");
        created.Country.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateCityAsync_WhenExists_UpdatesAndReturnsCity()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var country1 = new Country { CountryId = 1, Name = "Country1" };
        var country2 = new Country { CountryId = 2, Name = "Country2" };
        context.Countries.AddRange(country1, country2);
        context.Cities.Add(new City { CityId = 5, Name = "OldCity", CountryId = 1 });
        await context.SaveChangesAsync();

        var sut = new CityRepository(context);
        var updateModel = new City { CityId = 5, Name = "UpdatedCity", CountryId = 2 };

        // Act
        var updated = await sut.UpdateCityAsync(updateModel);

        // Assert
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("UpdatedCity");
        updated.CountryId.Should().Be(2);
    }

    [Fact]
    public async Task UpdateCityAsync_WhenNotFoundOrDeleted_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Cities.Add(new City { CityId = 6, Name = "DeletedCity", IsDeleted = true });
        await context.SaveChangesAsync();

        var sut = new CityRepository(context);

        // Act
        var result = await sut.UpdateCityAsync(new City { CityId = 6, Name = "New" });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCityAsync_WhenExists_SoftDeletesCity()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Cities.Add(new City { CityId = 1, Name = "ActiveCity", IsDeleted = false });
        await context.SaveChangesAsync();

        var sut = new CityRepository(context);

        // Act
        var success = await sut.DeleteCityAsync(1);

        // Assert
        success.Should().BeTrue();
        var inDb = await context.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CityId == 1);
        inDb!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task CountryExistsAsync_ChecksPresenceInDatabase()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        context.Countries.Add(new Country { CountryId = 10, Name = "Brazil", IsDeleted = false });
        context.Countries.Add(new Country { CountryId = 20, Name = "DeletedCountry", IsDeleted = true });
        await context.SaveChangesAsync();

        var sut = new CityRepository(context);

        // Act & Assert
        (await sut.CountryExistsAsync(10)).Should().BeTrue();
        (await sut.CountryExistsAsync(20)).Should().BeFalse(); // Filtered by soft delete
        (await sut.CountryExistsAsync(999)).Should().BeFalse();
    }
}
